# Authentication

This project uses Keycloak for authentication and authorization using either `Client Credentials` or `Authorization Code (with PKCE)` flows.
Both authentication flows are pre-configured in the Keycloak container when importing the realm and are supported in Scalar UI.
This tutorial explains how to use those in [Scalar UI](https://github.com/scalar/scalar) when developing.

## Common Ground

When you launch the application using Aspire, the Aspire Dashboard will be opened in your browser. 

Locate the **WeatherMonitor** and click on the **Scalar** link in the **URLs** column.

Right on top of the page, you will see a form where you can input the required fields for each authentication flow.

## Client Credentials

## Authorization Code