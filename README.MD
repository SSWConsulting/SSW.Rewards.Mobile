# SSW Rewards Mobile App!

This is a Xamarin.Forms app with a .NET Core 3 Api backend.

Use this app to scan SSW QR codes, earn SSW Points ⭐, claim rewards and win prizes!

## Contributing to this repo

To contribute to this project follow the steps below. Please ensure you are following the [Developer Guidelines](https://github.com/SSWConsulting/SSW.Consulting/Docs/Developer_Guidelines.MD) when submitting a pull request.

1. Clone the repository
2. Create a feature branch
3. Open a Pull Request


# Starting Development

## Required Tools
- Visual Studio 2019 v16.3+
- Xamarin forms Visual Studio packages installed
- Android SDK setup/ installed w/ Xamarin (https://docs.microsoft.com/en-us/xamarin/android/get-started/installation/android-sdk)
- iOS SDK setup/installed w/ Xamarin (https://docs.microsoft.com/en-us/xamarin/ios/get-started/installation/)
- XCode v11+ ( Mac / iOS only)
- Azure Storage Emulator ( Web API )


# The F5 Experience

## Web API
1. Get the source code
`git clone https://github.com/SSWConsulting/SSW.Consulting.git`
1. Grab connection strings from an existing dev to create local `secrets.json` file (https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.0&tabs=windows)
1. Run Azure Storage Emulator
1. Press f5

## Mobile App Android
1. Get the source code
`git clone https://github.com/SSWConsulting/SSW.Consulting.git`
1. Open the Xamarin project (/Xamarin/SSW.Consulting.sln)
1. Connect Android Device or Emulator (https://docs.microsoft.com/en-us/xamarin/android/get-started/installation/android-emulator/)
1. Set build target as desired device.
1. Run / press f5

## Mobile App iOS (Mac Only)
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


## <a name="analytics"></a>Trends and Analytics

* App Analytics here: https://analytics.itunes.apple.com/#/overview?app=1482994853&interval=r&datesel=d7&pmeasure=units&smeasure=units&tmeasure=units
(You need a login for this. See Adam Cogan for access)

* App sales and trends here: https://reportingitc2.apple.com/insights?pageid=8
(You need a login for this. See Adam Cogan for access)
