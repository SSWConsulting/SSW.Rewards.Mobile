## Technologies & Architecture

![image.png](imgs/ssw-rewards-architecture-diagram.png)
**Figure: Architecture diagram of SSW.Rewards project**


| Technology                              | Purpose                                                                  |
| --------------------------------------- | ------------------------------------------------------------------------ |
| Frontend - ~~React~~ Blazor WASM        | Admin Portal to interact with the mobile app                             |
| Auth - ~~Azure AD B2C~~ Identity Server | Auth for logging into the mobile app, accessing the api and Admin portal |
| Database - Azure SQL Server (EF Core)   | Stores User data                                                         |
| Database - Blob storage                 | Stores employee profile pictures                                         |
| Api - .NET CORE 5                       | Backend for the Admin portal and mobile app                              |
| Mobile App - Xamarin Forms              | Cross platform Mobile Application                                        |

### Infrastructure
[Staging/Dev](https://portal.azure.com/#@sswcom.onmicrosoft.com/resource/subscriptions/b8b18dcf-d83b-47e2-9886-00c2e983629e/resourceGroups/SSW.Consulting.Dev/overview)
[Production](https://portal.azure.com/#@sswcom.onmicrosoft.com/resource/subscriptions/b8b18dcf-d83b-47e2-9886-00c2e983629e/resourceGroups/SSW.Consulting.Prod/overview)

### Telemetry
[Staging/Dev](https://portal.azure.com/#@sswcom.onmicrosoft.com/resource/subscriptions/b8b18dcf-d83b-47e2-9886-00c2e983629e/resourceGroups/SSW.Consulting.Dev/providers/microsoft.insights/components/sswconsulting-dev/overview)
[Production](https://portal.azure.com/#@sswcom.onmicrosoft.com/resource/subscriptions/b8b18dcf-d83b-47e2-9886-00c2e983629e/resourceGroups/SSW.Consulting.Prod/providers/Microsoft.Insights/components/sswconsulting-prod/overview)

### Resource names
The resource names are in the process of being changed to align with the diagram.

### Auth
Auth is excluded from the architecture diagram. If you are working on auth, refer to the [SSW.IdentityServer](https://github.com/SSWConsulting/SSW.IdentityServer) project.

