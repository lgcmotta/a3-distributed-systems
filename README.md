# A3 – Distributed Systems & Mobile

Academic final assessment for Distributed Systems and Mobile class.

## Members

- [Luiz Motta](https://github.com/lgcmotta)
- [Igor Loureiro](https://github.com/IgorLoureiro)
- [Idelson Mendes](https://github.com/IdelsonMendes)

## Environment

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [.NET Aspire Templates](https://aspire.dev/get-started/aspire-sdk-templates/)
- [Aspire CLI](https://aspire.dev/get-started/install-cli/)
- [Docker](https://docs.docker.com/get-started/introduction/get-docker-desktop/)

### Setup

After cloning the project, you need to set up your development environment. To do so, follow the steps described below:

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) for your OS distribution and architecture (x86 or ARM). 
It's recommended to install the latest available SDK.
2. Install the [Aspire CLI](https://aspire.dev/get-started/install-cli/) and the [.NET Aspire Templates](https://aspire.dev/get-started/aspire-sdk-templates/).
3. Open your terminal and navigate to the cloned repository directory, and run:
    
   ```shell
    aspire certs trust
    ```
   
4. Finally, if you are on **Linux** or **macOS** run the `init.sh` script or if you're in **Windows** run the `init.ps1` script.
These scripts will generate the secrets and the `realm.json` file for the Keycloak container.
    
    > **Do not commit your `realm.json`, this file is already ignored, but please be sure not to intentionally track or commit this file.**

5. Open the IDE of your choice and select the `AppHost` as the entry point project for debugging. 
Aspire will pull the images and start all required Docker containers.

### Authentication

For the complete authentication documentation for [Scalar UI](https://github.com/scalar/scalar) and the available authentication flows, click [here](./docs/auth.md). 

## Documentation

> TBD
