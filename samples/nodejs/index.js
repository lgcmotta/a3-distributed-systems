import express from "express";

const app = express();
const port = process.env.PORT ?? 3000;

app.use(express.json());

app.post("/api/external/weather", (req, res) => {
    const receivedAt = new Date().toISOString();

    console.log("WeatherMonitor webhook received", {
        receivedAt,
        headers: {
            deliveryId: req.header("x-weathermonitor-delivery-id"),
            monitorId: req.header("x-weathermonitor-monitor-id"),
            event: req.header("x-weathermonitor-event"),
            sentAt: req.header("x-weathermonitor-sent-at"),
            idempotencyKey: req.header("idempotency-key"),
            authorization: req.header("authorization") ? "<present>" : "<missing>",
        },
        body: req.body,
    });

    res.status(202).json({
        accepted: true,
        receivedAt,
        deliveryId: req.header("x-weathermonitor-delivery-id"),
    });
});

app.get("/healthz", (_req, res) => {
    res.status(200).json({ status: "healthy" });
});

app.listen(port, () => {
    console.log(`Webhook sample API listening on port ${port}`);
});