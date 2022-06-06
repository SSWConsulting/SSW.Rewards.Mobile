## Getting Started

### Required Tools

- Visual Studio 2019 v16.3+
- Xamarin forms Visual Studio packages installed
- Android SDK setup/ installed w/ Xamarin (https://docs.microsoft.com/en-us/xamarin/android/get-started/installation/android-sdk)
- iOS SDK setup/installed w/ Xamarin (https://docs.microsoft.com/en-us/xamarin/ios/get-started/installation/)
- XCode v11+ ( Mac / iOS only)
- [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) (Web Api)
- [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/) (Easy way to upload and download files (see Local Emulator Database)
- [ngrok](https://ngrok.com/) (Api and Mobile communication)

## The F5 Experience

### Web API

1. Clone the repository `git clone https://github.com/SSWConsulting/SSW.Rewards.git`
1. Open the `API` Project within this repo
1. Grab connection strings from an existing dev to create local `secrets.json` file (https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.0&tabs=windows)
1. Install / Run `Azure Storage Emulator` (Run the .exe or use the "start" command)
1. Install / Run [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/#overview)
1. Start Explorer (**On First Install** - See steps below)
   1. Sign in using the "Subscription" option
   1. Select the "Azure" option
   1. Next (Sign in with your account that has access to ssw1 DevOps)
1. Inside the Explorer - **Local & Attached** | **Storage Accounts** | **Emulator** | **Blob Containers**
1. Create a **new container** called `profile`
1. Copy over all files in `SeedData/profiles` (images + .xlsx) into the newly created `profile` container
1. Press f5
1. Test that you can access the swagger docs @ `https://localhost:5001/swagger/`
1. **(Optional)** - If you need to run the api and mobile app locally. Set up an [ngrok](https://ngrok.com/) account (This is free and signing up allows 24hour access instead of the 2hours without an account)
   1. Once you have an account / installed ngrok
      Run `ngrok http https://localhost:5001`

### Mobile App Android

1. Get the source code `git clone https://github.com/SSWConsulting/SSW.Consulting.git`
1. Open the Xamarin project (/Xamarin/SSW.Consulting.sln)
1. Connect Android Device or Emulator (https://docs.microsoft.com/en-us/xamarin/android/get-started/installation/android-emulator/)
1. Set build target as desired device.
1. **(Optional)** - Using ngrok - If you need to connect to the Api locally
   1. Under SSW.Rewards | **Constants.cs**
   1. Update the `ApiBaseUrl` in The **DEBUG** region to use the custom ngrok **https** address (See Image below)
      ![ngrok Https Address](imgs/ngrok-https-example.png)
      **Figure: ngrok https address**
1. Run / press f5

### Mobile App iOS (Mac Only)

1. Get the source code
   `git clone https://github.com/SSWConsulting/SSW.Consulting.git`
1. Open the Xamarin project (/Xamarin/SSW.Consulting.sln)
1. Set build target (either connected iOS device or emulator)
1. Run / Press F5

**NOTE: if you cannot build and see an error relating to the provisioning profile/ app signing identity**

1. Open up the iOS project settings by right clicking on SSW.Consulting.iOS and selecting Options.
1. go to 'iOS Bundle Signing' and select your signing identity and provisioning profile.

- these should be automatic by default but if you get an error you can manually set them.
- if you don't have these, talk to another Team Member or Sys Admin and get them to add your appleID to the Superior Software for Windows Pty Ltd
  Apple Developer Program Team)

[Now you are setup, lets get started on a PBI](Definition-of-Ready.md)

Be sure to read the [Definition of done](Definition-of-Done.md) and the [Definition of ready](Definition-of-Ready.md)