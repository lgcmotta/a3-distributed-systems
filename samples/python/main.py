from datetime import datetime, timezone

from flask import Flask, jsonify, request

app = Flask(__name__)


@app.post("/api/external/weather")
def weather_monitor():
    received_at = datetime.now(timezone.utc).isoformat()

    print(
        "WeatherMonitor webhook received",
        {
            "receivedAt": received_at,
            "headers": {
                "deliveryId": request.headers.get("X-WeatherMonitor-Delivery-Id"),
                "monitorId": request.headers.get("X-WeatherMonitor-Monitor-Id"),
                "event": request.headers.get("X-WeatherMonitor-Event"),
                "sentAt": request.headers.get("X-WeatherMonitor-Sent-At"),
                "idempotencyKey": request.headers.get("Idempotency-Key"),
                "authorization": "<present>"
                if request.headers.get("Authorization")
                else "<missing>",
            },
            "body": request.get_json(silent=True),
        },
    )

    return (
        jsonify(
            {
                "accepted": True,
                "receivedAt": received_at,
                "deliveryId": request.headers.get("X-WeatherMonitor-Delivery-Id"),
            }
        ),
        202,
    )


@app.get("/healthz")
def healthz():
    return jsonify({"status": "healthy"})


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)