# The F5 Experience

> **Local dev now runs on .NET Aspire.** `aspire run` (from the repo root) brings up SQL Server +
> Azurite + WebAPI + AdminUI with one command (replacing `up.ps1` + `docker compose`). See the full
> guide: **[Aspire-Local-Dev.md](Aspire-Local-Dev.md)**. This page covers the end-to-end setup,
> including mobile and iOS signing.

**NOTE:** ⚠️ From 07/07/2025 we moved from Azure SQL Edge to SQL Server 2022! If you set up SSW.Rewards before that date, back up your DB and restore it afterwards (see [manage-database skill](../.agents/skills/manage-database/SKILL.md)).

## Local architecture

```mermaid
%%──────────────────────────────────────────────────────────
%%  SSW-red on charcoal  – same layout as your first diagram
%%──────────────────────────────────────────────────────────
%%{ init: {
      "theme":"base",
      "darkMode":true,                  /* custom colours win   */
      "themeVariables":{
        "fontSize":"15px",
        "fontFamily":"Inter, Segoe UI, sans-serif",

        /* red accents */
        "lineColor":"#d50000",
        "arrowheadColor":"#d50000",
        "clusterBorder":"#d50000",

        /* link label colour (edge text) */
        "tertiaryTextColor":"#cccccc"
      },
      "themeCSS":".node rect, .node polygon, .cluster rect { filter:none !important; }",
      "flowchart":{ "curve":"basis" }
}%%

flowchart LR

%%──────────────────── Local ────────────────────
subgraph Local["🧑‍💻 Local"]
  direction TB
  MobileApp["📱 Mobile App<br/>(.NET&nbsp;MAUI)"]
  DevTunnel[/"🌐 Tailscale<br/>(stable URL, trusted cert)"/]
  AdminUI["🖥️ Admin UI<br/>(Blazor)"]
  WebAPI{{"⚡ WebAPI<br/>(ASP.NET Core)"}}

  MobileApp -- HTTPS --> DevTunnel
  DevTunnel -- HTTPS --> WebAPI
  AdminUI  -- HTTPS --> WebAPI
end

%%──────────────────── Docker ───────────────────
subgraph Docker["🐳 Docker (via .NET Aspire)"]
  direction TB
  SQLServer[("🗄️ SQL Server")]
  Azurite[("🪣 Azurite Blob Storage")]

  WebAPI -- SQL --> SQLServer
  WebAPI -- "Blob API" --> Azurite
end

%%────────────────── External ───────────────────
subgraph External["🌐 External"]
  direction TB
  SSWIdentity["🔐 SSW.Identity<br/>(can run local)"]
  SSWQuizGPT["🧠 SSW.QuizGPT<br/>(can run local)"]
  NotificationHub["🔔 AWS Notification Hub"]

  WebAPI    -- HTTPS         --> SSWQuizGPT
  WebAPI    -- HTTPS         --> SSWIdentity
  AdminUI   -- HTTPS         --> SSWIdentity
  MobileApp -- HTTPS         --> WebAPI
  MobileApp -- HTTPS         --> SSWIdentity

  WebAPI    -- Notifications --> NotificationHub
  NotificationHub -- Push    --> MobileApp
end

%%────────────────── Styling ────────────────────
classDef clusterStyle stroke-width:2,rx:6,ry:6;
classDef service fill:#1e1e1e,stroke:#d50000,stroke-width:1,color:#ffffff;

class Local,Docker,External clusterStyle;
class MobileApp,DevTunnel,AdminUI,WebAPI,SQLServer,Azurite,SSWIdentity,SSWQuizGPT,NotificationHub service;
```

## Requirements

**TODO** Find Azurite seed data for the API (Tylah might be blind)

- .NET 10 SDK https://dotnet.microsoft.com/en-us/download/dotnet/10.0 (`global.json` pins `10.0.301`)
- Aspire CLI ≥ **13.4.6**: `dotnet tool install -g aspire` (or `dotnet tool update -g aspire`)
- [Docker Desktop](https://docs.docker.com/desktop/) — Aspire runs SQL + Azurite as containers
- IDE - Visual Studio Enterprise Latest // Jetbrains Rider // VS Code
- Android SDK (for the mobile app) + MAUI workloads: `dotnet workload install maui` (or `maui-android`; needs `sudo` on a system-wide .NET install)
- Optional: [Azure Data Studio](https://azure.microsoft.com/en-us/products/data-studio/) (Not required, can use IDE Tools for DB Querying)
- Optional: [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/) (Easy way to upload and download files)

- For testing the mobile app on a phone, use **Tailscale** for a stable, iOS-trusted URL (see the
  Mobile UI section below) — it replaces Dev Tunnels / ngrok.

### Required Tools (for Mac)

- XCode v15.0.1+
  - Check or set `Command Line Tools` location
  - **Preferences** | **Locations** | **Command Line Tools**

## Setting up the Repo for Development

### To work on the API + Admin UI

1. Clone this Repo https://github.com/SSWConsulting/SSW.Rewards.Mobile.git
2. Download and install latest .NET 10 SDK
3. Trust the local dev HTTPS certificate (one-time):

   ```bash
   dotnet dev-certs https --trust
   ```

   (Also available later as the **Tools: Trust dev HTTPS cert** dashboard command. If you get
   "A valid HTTPS certificate is already present", run `dotnet dev-certs https --clean` first.)

   Before the first run, put the secrets in place — **Keeper is the only external resource**. Every
   stack secret lives in one store (the AppHost user-secrets); copy the single
   **`SSW.Rewards ▸ SSW.Rewards — Aspire Dev Secrets`** Keeper record, then:

   ```bash
   ./rewards-dev secrets edit    # opens secrets.json — paste the Keeper record, save
   ./rewards-dev secrets check   # ✓/✗ per key; tells you what's missing + where in Keeper
   ```

4. Start the local stack with Aspire (Docker must be running):

   ```bash
   aspire run   # from the repo root (.aspire/settings.json targets src/AppHost)
   ```

   `WebAPI`/`AdminUI` no longer carry their own secrets — everything is injected from the AppHost.
   If a secret is missing, Aspire reports *ValueMissing* and SQL won't start; run
   `./rewards-dev secrets edit`, paste, and re-run.

The dashboard opens automatically. SQL Server + Azurite come up as **persistent** containers, then
WebAPI + AdminUI start once SQL is healthy (EF migrations apply on WebAPI startup):

- AdminUI → https://localhost:7137
- WebAPI Swagger → https://localhost:5001/swagger/index.html

> Full guide, dashboard commands, and the `rewards-dev` target switcher: **[Aspire-Local-Dev.md](Aspire-Local-Dev.md)**.

5. Check [manage-database skill](../.agents/skills/manage-database/SKILL.md) if you want to restore an existing DB.

**TODO: Current seeding might not fully work for development. [Tech-Debt - #540](https://github.com/SSWConsulting/SSW.Rewards.Mobile/issues/540)**

Currently, we need to restore the DB like for instance from staging or another developer. Follow the `manage-database` skill (`.agents/skills/manage-database/SKILL.md`) for backup and restore instructions.

## Mobile UI

Follow Microsoft Learn’s step-by-step guide to get your first .NET MAUI project up and running. It lets you run Mobile UI without issues. https://learn.microsoft.com/en-us/dotnet/maui/get-started/first-app?view=net-maui-8.0&tabs=vsmac&pivots=devices-android

> **Don't hand-edit `Constants.cs`.** The DEBUG API/identity URLs come from a git-ignored override
> written by the `rewards-dev` CLI. See [Aspire-Local-Dev.md](Aspire-Local-Dev.md) for the full flow.

### To work on the Mobile UI (Android)

1. Install the MAUI workload (one-time): `dotnet workload install maui` (or `maui-android`).
2. Make sure the Firebase config files exist (git-ignored): copy `google-services.json` /
   `GoogleService-Info.plist` from Keeper into `src/MobileUI/Platforms/{Android,iOS}/`, or use the
   **Mobile: Sync mobile secrets (isolated)** dashboard command. Only `*.template` placeholders are committed.
3. Point the app at a backend (emulators **can't** reach `localhost`, so use `staging` or `tailscale`):

   ```bash
   dotnet run --project tools/RewardsDev -- env staging      # or: api tailscale + identity staging
   ```

4. Build and deploy to a running emulator:

   ```bash
   dotnet build src/MobileUI/MobileUI.csproj -t:Run -f net10.0-android -c Debug
   ```

   (`-t:Run` is required for Debug builds — a plain `adb install` of the APK won't start due to Fast Deployment.)

#### Phone testing with Tailscale (stable URL + trusted cert)

```bash
tailscale serve --bg https / https+insecure://localhost:5001     # iOS-trusted HTTPS to the local API
dotnet run --project tools/RewardsDev -- api tailscale            # auto-detects your *.ts.net URL
```

### To work on the Mobile UI (iOS, macOS only)

1. Complete the steps above, then target an iOS simulator/device (requires the `maui-ios` workload).

**NOTE: if you cannot build and see an error relating to the provisioning profile/ app signing identity**

**NOTE: if the solution fails to load, open a terminal in the SSW.Rewards.Mobile folder and run:**

```bash
dotnet workload update
dotnet workload restore
```

1. Open up the iOS project settings by right clicking on SSW.Consulting.iOS and selecting Options.
1. go to 'iOS Bundle Signing' and select your signing identity and provisioning profile.

- These should be automatic by default but if you get an error you can manually set them.
- If you don't have these, talk to another Team Member or Sys Admin and get them to add your AppleID to the Superior Software for Windows Pty Ltd
  Apple Developer Program Team

### Setting up your own iPhone for testing

If you want to set up to deploy to your own iPhone, talk to an App Manager, it's hard! :)
Or you can give it a go yourself. For that you will be using 2 portals - App Store Connect and Apple Developer Portal:

1. Talk to a team member who has either Admin or App Manager role in App Store Connect. Give them your Apple ID (you can provide your personal one) and they will send you an invite to the Apple Developer team. If you've never used Apple products before, you will need to [create an Apple ID](https://support.apple.com/en-au/108647). You need to be granted **at least the App Manager role** to be able to perform some of the steps below; otherwise ask someone with the App Manager Role or higher to assist you.
2. On your mac create a Certificate Signing Request (CSR) using Keychain. Go to Keychain Access -> Certificate Assistant -> Request a certificate from a certificate authority. Fill the fields and save generated CSR somewhere.
3. Go to https://developer.apple.com, scroll down and click "Certificates, IDs, & Profiles".
4. Select the Certificates tab. Add a new Certificate of type `Apple Development`. For that you will need to upload the CSR. Once the certificate is created, the CSR is no longer needed and can be deleted.
5. Download the certificate on your mac and double click it to install.
6. Go back to the developer portal and select the Devices tab.
7. Add a device you are gonna be using for testing. To get your device's UDID connect it to your mac and open the Music app. Select your device on the left and then click a couple of times on the subtitle which displays your iPhone model, storage and battery level.
8. Once your device is added to the developer portal, select the Profiles tab. Update the provisioning profile for iOS App Development `SSWRewards_Dev`. Select the certificate and device added on the previous steps. **Tip:** Any changes to the provisioning profile won't disrupt the work of your team.
9. Download the provisioning profile on your mac and double click to install it. **Tip:** provisioning profiles are installed to _~/Library/MobileDevice/Provisioning Profiles/_

After that you should be able to deploy and debug the application on your iPhone.

### Creating a production build locally

Currently we have GitHub Actions which build and push the application to InternalTesting and TestFlight. But sometimes it's required to build and sign the application locally.
Below is a detailed description how to do that.

#### iOS

Apple expects developers to have individual development certificates (up to 2 per developer) and share distribution certificates (up to 3 per team).
To be able to build and sign an ipa file you need to have a distribution certificate and provisioning profile associated with it installed on your machine.
All the secrets can be found in our password manager (PM).

1. Get distribution certificate as a base-64 string from PM and run: `echo <cert-base64-string> | base64 --decode > DistributionCertificate.p12`
2. Double click the certificate to install it; use the password from PM
3. Download distribution provisioning profile form the Apple Developer Portal
4. Double click the provisioning profile to install it
5. Cd into the MobileUI folder
6. Run `dotnet publish -f net10.0-ios -p:ArchiveOnBuild=true -p:CodesignKey="XXX" -p:CodesignProvision="YYY"`; XXX is the CertificateName (in PM) and YYY is ProfileName (same as in Apple Developer Portal)
7. The ipa file in the _../MobileUI/bin/Release/net10.0-ios/ios-arm64/publish/_ folder is ready to be uploaded to TestFlight

#### Android

1. Find keystore info in PM
2. Run `echo <keystore-base64-string> | base64 --decode > rewards.keystore`
3. To see the info about keystore run `keytool -list -v -keystore rewards.keystore`
4. Run `dotnet publish -f net10.0-android -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=rewards.keystore -p:AndroidSigningKeyAlias=XXX -p:AndroidSigningKeyPass=YYY -p:AndroidSigningStorePass=YYY`; XXX and YYY can be found in PM
5. The signed aab file in the _../MobileUI/bin/Release/net10.0-android/publish/_ folder is ready to be uploaded to InternalTesting

[Now you are setup, lets get started on a PBI](Definition-of-Ready.md)

Be sure to read the [Definition of done](Definition-of-Done.md) and the [Definition of ready](Definition-of-Ready.md)
