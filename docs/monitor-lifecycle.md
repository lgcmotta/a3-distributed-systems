# Monitor Lifecycle

Monitors are owned by authenticated clients. The API always derives `ClientId` from the authenticated principal and never accepts it from route, query string, or request body. Cross-client monitor access is intentionally indistinguishable from a missing resource and returns `404`.

The canonical monitor response never includes the stored webhook access token.

## Create Monitor

Creating a monitor resolves the requested city through BrasilAPI, verifies that the resolved city belongs to the requested state, checks the duplicate rule, and persists the monitor.

A duplicate monitor has the same client, resolved city, scheduled local time, and time zone.

```mermaid
flowchart TD
    Start["POST /api/v1/monitors"] --> Auth["Authenticate client"]
    Auth --> ResolveCity["Resolve city with BrasilAPI"]
    ResolveCity --> CityFound{"City found in requested state?"}
    CityFound -->|No| CityNotFound["Return 400 Monitor City Not Found"]
    CityFound -->|Yes| DuplicateCheck["Check duplicate monitor in PostgreSQL"]
    DuplicateCheck --> Duplicate{"Duplicate exists?"}
    Duplicate -->|Yes| Conflict["Return 409 Duplicate Weather Monitor"]
    Duplicate -->|No| Persist["Persist monitor"]
    Persist --> Created["Return 201 Created"]
```

```mermaid
sequenceDiagram
    actor Client
    participant API as WeatherMonitor.Api
    participant Auth as Keycloak middleware
    participant Handler as CreateMonitorCommandHandler
    participant BrasilAPI
    participant DB as PostgreSQL

    Client->>API: POST /api/v1/monitors
    API->>Auth: Validate bearer token
    Auth-->>API: Client identity
    API->>Handler: Dispatch create command
    Handler->>BrasilAPI: Search city by name

    alt City lookup unavailable
        Handler-->>API: CityLookupUnavailableException
        API-->>Client: 503 ProblemDetails
    else City not resolved for requested state
        Handler-->>API: MonitorCityNotFoundException
        API-->>Client: 400 ProblemDetails
    else City resolved
        Handler->>DB: Check duplicate monitor
        alt Duplicate exists
            Handler-->>API: DuplicateWeatherMonitorException
            API-->>Client: 409 ProblemDetails
        else No duplicate
            Handler->>DB: Add monitor and save changes
            Handler-->>API: MonitorResponse
            API-->>Client: 201 Created
        end
    end
```

## Patch Monitor

Patch requests may change only the webhook URL, access token, scheduled time, time zone, and enabled state. `null` means "do not change this property"; `AccessToken = ""` clears the stored token. City and weather condition are immutable after creation.

The duplicate check only runs when the schedule identity changes. In this project, schedule identity is the scheduled local time plus time zone for the same client and resolved city.

```mermaid
flowchart TD
    Start["PATCH /api/v1/monitors/{monitorId}"] --> Auth["Authenticate client"]
    Auth --> Load["Load monitor by MonitorId and ClientId"]
    Load --> Found{"Monitor found?"}
    Found -->|No| NotFound["Return 404 Monitor Not Found"]
    Found -->|Yes| ScheduleChanged{"Time or time zone changed?"}
    ScheduleChanged -->|No| ApplyPatch["Apply allowed patch fields"]
    ScheduleChanged -->|Yes| DuplicateCheck["Check duplicate schedule identity"]
    DuplicateCheck --> Duplicate{"Duplicate exists?"}
    Duplicate -->|Yes| Conflict["Return 409 Duplicate Weather Monitor"]
    Duplicate -->|No| ApplyPatch
    ApplyPatch --> Save["Save changes"]
    Save --> Ok["Return 200 OK"]
```

```mermaid
sequenceDiagram
    actor Client
    participant API as WeatherMonitor.Api
    participant Auth as Keycloak middleware
    participant Handler as PatchMonitorCommandHandler
    participant DB as PostgreSQL

    Client->>API: PATCH /api/v1/monitors/{monitorId}
    API->>Auth: Validate bearer token
    Auth-->>API: Client identity
    API->>Handler: Dispatch patch command
    Handler->>DB: Load monitor by MonitorId and ClientId

    alt Monitor missing or belongs to another client
        Handler-->>API: MonitorNotFoundException
        API-->>Client: 404 ProblemDetails
    else Monitor belongs to client
        Handler->>Handler: Compute target time and time zone
        alt Schedule identity changed
            Handler->>DB: Check duplicate monitor
        end

        alt Duplicate exists
            Handler-->>API: DuplicateWeatherMonitorException
            API-->>Client: 409 ProblemDetails
        else Patch accepted
            Handler->>Handler: Reconfigure webhook, token, schedule, time zone, and enabled state
            Handler->>DB: Save changes
            Handler-->>API: MonitorResponse
            API-->>Client: 200 OK
        end
    end
```

## Read And List Monitors

Monitor reads always filter by `ClientId`. A monitor owned by another client is treated as not found and returns `404`. List endpoints return only the authenticated client's monitors.

```mermaid
flowchart TD
    Request["GET monitor or monitors"] --> Auth["Authenticate client"]
    Auth --> Query["Query PostgreSQL with ClientId filter"]
    Query --> Single{"Single monitor endpoint?"}
    Single -->|No| Page["Return paged client-owned monitors"]
    Single -->|Yes| Found{"Monitor found for client?"}
    Found -->|No| NotFound["Return 404"]
    Found -->|Yes| Response["Return monitor response"]
```
