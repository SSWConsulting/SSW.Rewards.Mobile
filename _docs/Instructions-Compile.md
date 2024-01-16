# The F5 Experience

## Requirements 
**TODO** Find Azurite seed data for the API (Tylah might be blind)

- .NET 8 https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- IDE - Visual Studio Enterprise Latest // Jetbrains Rider // VS Code 
- Android SDK setup/ installed w/ Xamarin (https://docs.microsoft.com/en-us/xamarin/android/get-started/installation/android-sdk)
- iOS SDK setup/installed w/ Xamarin (https://docs.microsoft.com/en-us/xamarin/ios/get-started/installation/)
- [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/) (Easy way to upload and download files (see Local Emulator Database)
- [Azure Data Studio](https://azure.microsoft.com/en-us/products/data-studio/) (Not required, can use IDE Tools for DB Querying)

- Install Dev Tunnels or Ngrok see the rule https://ssw.com.au/rules/port-forwarding/
  - [dev tunnels](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=macos) (Recommended)
  - [ngrok](https://ngrok.com/)

### Required Tools (for Mac)

- XCode v15.0.1+
  - Check or set `Command Line Tools` location
  - **Preferences** | **Locations** | **Command Line Tools**

## Setting up the Repo for Development
### To work on the API + Admin UI
1. Pull this Repo
2. Grab the Secrets from Keeper 
   1. **Client Secrets | SSW | SSW.Rewards | Developer Secrets**
   2. Add them as .NET User Secrets for `WebAPI.csproj`
   3. Update `appsettings.*.json` files accordingly
3. Create a Developer Certificate https://learn.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-8.0#certificates
   1. Create `WebAPI.pfx` with a password of `ThisPassword` (You can change change this, but the `docker-compose.yml` should be updated appropriately)

**Windows**
```bash
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\WebAPI.pfx -p ThisPassword
dotnet dev-certs https --trust
```

**Mac**
```bash
   dotnet dev-certs https -ep ${HOME}/.aspnet/https/WebAPI.pfx -p ThisPassword
   dotnet dev-certs https --trust
```


4. Cd into the Repo
5. Run the Docker Containers
 ```bash
 docker compose build
 docker compose --profile all up -d
 ```   

You should now be able to access the AdminUI hosted locally at https://localhost:7137  


You should now be able to access the WebAPI Swagger docs at https://localhost:5001/swagger/index.html

  
**Note:** You can run only the WebAPI or AdminUI by running:
```bash
docker compose --profile webapi up -d
OR
docker compose --profile admin up -d
```

## Mobile UI

Microsoft Learn has a great step-by-step process to get your first .NET MAUI project up and running, this will should allow you to run the Mobile UI with out much issue! https://learn.microsoft.com/en-us/dotnet/maui/get-started/first-app?view=net-maui-8.0&tabs=vsmac&pivots=devices-android 

### To work on the Mobile UI (Android SDK)
1. Run the Docker containers (Only WebApi required)
2. Start a Dev Tunnel for the API
```bash
devtunnel host -p 5001
```
3. Update the `Constants.cs` `ApiBaseUrl` in the **#if DEBUG** block to use your DevTunnel address
4. Run the MobileUI, targeting your Android Emulator

### To work on the Mobile UI (iOS, MacOS Only)
2. Complete steps 1-3 above
3. Run the MobileUI, targeting your Android Emulator

**NOTE: if you cannot build and see an error relating to the provisioning profile/ app signing identity**

1. Open up the iOS project settings by right clicking on SSW.Consulting.iOS and selecting Options.
1. go to 'iOS Bundle Signing' and select your signing identity and provisioning profile.

- These should be automatic by default but if you get an error you can manually set them.
- If you don't have these, talk to another Team Member or Sys Admin and get them to add your AppleID to the Superior Software for Windows Pty Ltd
  Apple Developer Program Team

### Setting up for Apple Development (on your own iPhone)
If you want to set up to deploy to your own iPhone, talk to an App Manager (it's hard :))!

[Now you are setup, lets get started on a PBI](Definition-of-Ready.md)

Be sure to read the [Definition of done](Definition-of-Done.md) and the [Definition of ready](Definition-of-Ready.md)