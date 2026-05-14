package main

import (
	"log"
	"net/http"
	"os"
	"time"
	"fmt"

	"github.com/gin-gonic/gin"
)

func main() {
	port := os.Getenv("PORT")

	if port == "" {
		port = "8080"
	}

	router := gin.Default()

	router.POST("/api/external/weather", func(ctx *gin.Context) {
		var body map[string]any

		if err := ctx.ShouldBindJSON(&body); err != nil {
			ctx.JSON(http.StatusBadRequest, gin.H{
				"error": "invalid JSON body",
			})
			return
		}

		receivedAt := time.Now().UTC().Format(time.RFC3339)

		log.Printf("WeatherMonitor webhook received: deliveryId=%s monitorId=%s event=%s sentAt=%s idempotencyKey=%s authorizationPresent=%t body=%v",
			ctx.GetHeader("X-WeatherMonitor-Delivery-Id"),
			ctx.GetHeader("X-WeatherMonitor-Monitor-Id"),
			ctx.GetHeader("X-WeatherMonitor-Event"),
			ctx.GetHeader("X-WeatherMonitor-Sent-At"),
			ctx.GetHeader("Idempotency-Key"),
			ctx.GetHeader("Authorization") != "",
			body,
		)

		ctx.JSON(http.StatusAccepted, gin.H{
			"accepted":   true,
			"receivedAt": receivedAt,
			"deliveryId": ctx.GetHeader("X-WeatherMonitor-Delivery-Id"),
		})
	})

	router.GET("/healthz", func(ctx *gin.Context) {
		ctx.JSON(http.StatusOK, gin.H{
			"status": "healthy",
		})
	})

	log.Printf("Webhook sample API listening on port %s", port)

	address := fmt.Sprintf(":%s", port)

	if err := router.Run(address); err != nil {
		log.Fatal(err)
	}
}