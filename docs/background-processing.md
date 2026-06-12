# Background Processing

Background processing is handled by TickerQ. One recurring job runs `WeatherMonitorProcessor`, which finds matching monitors and schedules `WebhookDelivery` jobs. Each scheduled delivery is executed by `WebhookMonitorDispatcher` through a TickerQ time ticker.

Webhook delivery retry behavior is currently hardcoded when the delivery job is created: `Retries = 2` and `RetryIntervals = [60, 60]`.

## End-to-End Flow

```mermaid
flowchart TD
    Cron["TickerQ cron schedule"] --> Processor["WeatherMonitorProcessor"]
    Processor --> CityCodes["Load enabled monitor city codes"]
    CityCodes --> Forecast["Fetch forecast from BrasilAPI by city"]
    Forecast --> ForecastOk{"Forecast response successful?"}
    ForecastOk -->|No| SkipCity["Log and skip city"]
    ForecastOk -->|Yes| StreamMonitors["Stream enabled monitors for city"]
    StreamMonitors --> ForecastDate["Calculate monitor forecast date"]
    ForecastDate --> WeatherFound{"Forecast date found?"}
    WeatherFound -->|No| SkipMonitor["Log and skip monitor"]
    WeatherFound -->|Yes| ConditionMatch{"Weather condition matches?"}
    ConditionMatch -->|No| SkipMonitor
    ConditionMatch -->|Yes| ExistingDelivery{"Pending or delivered delivery exists?"}
    ExistingDelivery -->|Yes| SkipMonitor
    ExistingDelivery -->|No| CreateDelivery["Create pending webhook delivery"]
    CreateDelivery --> Schedule["Schedule WebhookDelivery job"]
    Schedule --> TickerQDelivery["TickerQ time ticker"]

    subgraph Retryable["Retryable scheduled delivery"]
        Execute["Execute scheduled delivery job"]
        Load["Load delivery and monitor"]
        RecordRetry["Register retry attempt when RetryCount > 0"]
        Send["POST webhook event"]
        Delivered["Mark delivered"]
        Retry["Throw to trigger retry"]
        Failed["Mark failed"]

        Execute --> Load
        Load --> RecordRetry
        RecordRetry --> Send
        Send --> Delivered
        Send --> Retry
        Retry --> Execute
        Send --> Failed
    end

    TickerQDelivery --> Execute
```

## Processor Execution

The processor groups work by city code. This avoids fetching the same forecast once per monitor during a processor execution. The BrasilAPI forecast endpoint is called once per enabled city code, then every enabled monitor in that city is matched against the returned forecast. External BrasilAPI `GET` calls also pass through the Redis-backed HybridCache layer, which can serve successful cached responses for repeated lookups.

```mermaid
sequenceDiagram
    participant TickerQ
    participant Processor as WeatherMonitorProcessor
    participant DB as PostgreSQL
    participant BrasilAPI
    participant Manager as TickerQ time ticker manager

    TickerQ->>Processor: Run scheduled processor
    Processor->>DB: Load distinct enabled city codes

    loop For each city code
        Processor->>BrasilAPI: Get forecast for configured number of days

        alt Forecast unavailable or unsuccessful
            Processor-->>TickerQ: Log city failure and continue
        else Forecast loaded
            Processor->>DB: Stream enabled monitors for city

            loop For each monitor
                Processor->>Processor: Calculate forecast date
                Processor->>Processor: Match weather condition
                Processor->>DB: Check pending or delivered delivery for forecast date

                alt Monitor matches and no delivery exists
                    Processor->>DB: Add pending webhook delivery
                    Processor->>Manager: Schedule webhook delivery job
                    Manager-->>Processor: Time ticker id
                    Processor->>DB: Save delivery with assigned job id
                else Monitor does not require delivery
                    Processor-->>TickerQ: Continue processing
                end
            end
        end
    end
```

## Delivery Dispatch And Retries

TickerQ invokes `WebhookMonitorDispatcher` when a scheduled `WebhookDelivery` job is due. The dispatcher validates that the delivery and monitor still exist, that the monitor is enabled, and that the delivery still matches the monitor owner. It then sends a JSON webhook event to the configured URL.

If the webhook call fails, the dispatcher throws so TickerQ can retry the job. When TickerQ invokes a retry attempt, the delivery retry count is registered before the webhook is sent again. On the final retry attempt, the delivery is marked as failed.

```mermaid
sequenceDiagram
    participant TickerQ
    participant Dispatcher as WebhookMonitorDispatcher
    participant DB as PostgreSQL
    participant Webhook as External Webhook

    rect rgb(235, 245, 255)
        TickerQ->>Dispatcher: Execute scheduled delivery job
        Dispatcher->>DB: Load delivery by DeliveryId

        alt Delivery already delivered or failed
            Dispatcher-->>TickerQ: Return without sending
        else Delivery is pending
            opt Retry invocation
                Dispatcher->>DB: Register retry attempt
            end

            Dispatcher->>DB: Load monitor by MonitorId

            alt Monitor missing
                Dispatcher->>DB: Mark failed on final retry
                Dispatcher-->>TickerQ: Throw to retry or fail
            else Monitor disabled or ownership mismatch
                Dispatcher->>DB: Mark failed
                Dispatcher-->>TickerQ: Return without retry
            else Monitor can receive delivery
                Dispatcher->>Webhook: POST weather event

                alt Webhook succeeds
                    Dispatcher->>DB: Mark delivery as delivered
                    Dispatcher-->>TickerQ: Complete job
                else Webhook fails and retries remain
                    Dispatcher-->>TickerQ: Throw to trigger retry
                else Webhook fails on final retry
                    Dispatcher->>DB: Mark delivery as failed
                    Dispatcher-->>TickerQ: Throw final failure
                end
            end
        end
    end
```

## Webhook Request Shape

The dispatcher sends a `POST` request to the monitor webhook URL. The request body is JSON and contains delivery, monitor, forecast, location, and weather-condition data. The dispatcher also adds delivery metadata headers:

- `X-WeatherMonitor-Delivery-Id`
- `X-WeatherMonitor-Monitor-Id`
- `X-WeatherMonitor-Event`
- `X-WeatherMonitor-Sent-At`
- `Idempotency-Key`

If the monitor has a stored access token, the dispatcher sends it as a bearer token in the `Authorization` header.
