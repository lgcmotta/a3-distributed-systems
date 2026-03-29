# Authentication

This project uses Keycloak for authentication and authorization using either **Client Credentials** or **Authorization Code (with PKCE)** flows.
Both authentication flows are preconfigured in the Keycloak container during realm import and are supported in Scalar UI.
This tutorial explains how to use those in [Scalar UI](https://github.com/scalar/scalar) when developing.

## Common Ground

When you launch the application with Aspire, the Aspire Dashboard opens in your browser. 

Locate the **WeatherMonitor** and click on the **Scalar** link in the **URLs** column.

<img width="1795" height="505" alt="image" src="https://github.com/user-attachments/assets/bfe0b125-7f10-4a0c-b2e1-9b83a46173be" />

Right at the top of the page, you will see a form where you can enter the required fields for each authentication flow.

<img width="1795" height="718" alt="image" src="https://github.com/user-attachments/assets/6a588e94-6a40-4985-95f8-0a9ab1f1cd27" />

## Client Credentials

1. To use the **Client Credentials** authentication flow, select the `clientCredentials` tab.
2. Open your `src/AppHost/Realms/realm.json` file and locate the chosen client ID, either `weather_consumer_a` or `weather_consumer_b`.
3. Copy the `clientId` and the `secret` and paste them into the `Client ID` and `Client Secret` fields on Scalar UI.

  <img width="1795" height="718" alt="image" src="https://github.com/user-attachments/assets/576f425e-de46-4456-a114-14a217832772" />

4. Click on the `Authorize` button.
5. You should see that the **Authentication** form has changed, and it only has a single field with the **Access Token**.

  <img width="1795" height="588" alt="image" src="https://github.com/user-attachments/assets/d3808d40-0fa0-4bac-bd06-ed06d65722bf" />

6. After following those steps, Scalar will automatically add the `Authorization` header to all requests.

## Authorization Code

1. To use the **Authorization Code** authentication flow, select the `authorizationCode` tab.
2. Write **postman** into the **Client ID** field.
  
  > The Authorization Code flow **DOES NOT** require the **Client Secret** field; only the **Client ID** is required.

  <img width="1795" height="780" alt="image" src="https://github.com/user-attachments/assets/1c2d90c6-bf39-42c7-a9d4-afa4d83eb873" />

3. Click on the `Authorize` button. You should see the **Keycloak** login page:

   <img width="805" height="670" alt="image" src="https://github.com/user-attachments/assets/7b567329-2bec-43c6-8a7f-513a3bf61195" />

4. Open your `src/AppHost/Realms/realm.json` file and locate the username `dev-admin`.
5. The credentials for the `dev-admin` user are right below its definition on the `realm.json` file. Copy the generated password for the `dev-admin` user.
6. Fill out the form and click the **Sign In** button.

  <img width="805" height="670" alt="image" src="https://github.com/user-attachments/assets/c00e0a1e-c994-423d-8538-5a28e3b4f6c2" />

7. If it's the first time you're logging in as `dev-admin`, **Keycloak** will prompt to provide the user's first and last name.
   Fill out this form with whatever values you find suitable. Click on the **Submit** button.

  <img width="805" height="670" alt="image" src="https://github.com/user-attachments/assets/1af7c3cb-f4e8-4cae-9947-07bca1682bee" />

8. You should see that the **Authentication** form has changed, and it only has a single field with the **Access Token**.

  <img width="1789" height="610" alt="image" src="https://github.com/user-attachments/assets/309d9775-d4aa-4a0e-8e76-97797ca20079" />

9. After following those steps, Scalar will automatically add the `Authorization` header to all requests.
