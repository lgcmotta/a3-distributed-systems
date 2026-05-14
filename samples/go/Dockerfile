FROM mirror.gcr.io/golang:1.26-alpine AS build

WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY main.go ./

RUN CGO_ENABLED=0 GOOS=linux go build -o main ./main.go

FROM scratch AS final

WORKDIR /app

COPY --from=build /app/main /main

ENV PORT=8080

EXPOSE 8080

ENTRYPOINT ["/main"]