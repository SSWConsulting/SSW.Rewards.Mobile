# SSW.Rewards — Local dev with .NET Aspire

This replaces `docker-compose.yml` + `up.ps1` + hand-edited secrets/URLs for local development.
The MAUI mobile app still runs the normal way (emulator/device) — Aspire's job for mobile is
config/secret **materialization** and tunnel wiring, not running the app.

## Prerequisites (one-time)
- .NET 10 SDK — `global.json` pins `10.0.301` (no `workloadVersion` pin, so the resolver doesn't fail).
- Aspire CLI ≥ **13.4.6**: `dotnet tool install -g aspire` (or `dotnet tool update -g aspire`).
- Docker running.
- For the **mobile app** only: MAUI workloads — `dotnet workload install maui` (or `maui-android` for Android-only). On a system-wide .NET install this needs `sudo`.

## First run
```bash
cd src/AppHost
aspire run
```
On first launch Aspire prompts (once) for the secret parameters and stores them in **this
AppHost's** user-secrets (id `F76E3E10-…`). To seed them non-interactively instead:
```bash
dotnet user-secrets set --id F76E3E10-FABB-4543-B949-549EEC500823 "Parameters:sql-sa-password" "<pick-a-strong-pw>"
# …firebase-credentials, sendgrid-api-key, email-user, email-password, signing-authority,
#   mobile-google-services-json, mobile-google-service-info-plist (from Keeper)
```
The dashboard opens automatically. SQL Server + Azurite come up as **persistent** containers
with data volumes (`ssw-rewards-sql-data`, `ssw-rewards-azurite-data`), so your data survives
restarts. WebAPI + AdminUI start once SQL is healthy; migrations apply on WebAPI startup.

> Secrets now flow **only** from the AppHost. WebAPI/AdminUI no longer carry their own
> `UserSecretsId`. Aspire injects `ConnectionStrings:DefaultConnection` / `:HangfireConnection`,
> `CloudBlobProviderOptions:ContentStorageConnectionString` (→ Azurite), `Firebase:FirebaseCredentials`,
> `SendGridAPIKey`, `EmailUser`, `EmailPassword`, `SigningAuthority` as env vars.

## Dashboard commands (Actions ▸ Commands on the `rewards-sql` resource)
- **DB: Apply migrations** / **Add migration…** — EF update / add (prompts for the name)
- **Tools: Install/upgrade dotnet-ef**, **MAUI workload restore**, **Trust dev HTTPS cert**
- **Mobile: Materialize Firebase secrets** — writes `google-services.json` + `GoogleService-Info.plist`
  from the secret parameters (these files are git-ignored; only `*.template` is committed)
- **Switch identity / API target…** — shells out to the `rewards-dev` CLI (below), which
  switches **all** apps, not just mobile

## Switching dev targets — the `rewards-dev` CLI
Stop hand-editing `Constants.cs` and the AdminUI `appsettings`. One command switches the
identity authority and/or API URL across **all apps**, in sync, via **git-ignored** overrides:

| App | Override file (git-ignored) | Falls back to |
|---|---|---|
| Mobile | `src/MobileUI/Constants.LocalDev.cs` | committed `Constants.LocalDev.Default.cs` |
| AdminUI | `src/AdminUI/wwwroot/appsettings.Local.json` | committed `appsettings(.Development).json` |
| WebAPI | AppHost user-secret `Parameters:signing-authority` | prompted at `aspire run` |

```bash
# identity → Mobile + AdminUI + WebAPI ;  api → Mobile + AdminUI ;  env → both
dotnet run --project tools/RewardsDev -- env staging        # everything → staging (safe default)
dotnet run --project tools/RewardsDev -- api local          # https://localhost:5001 (AdminUI + Mobile)
dotnet run --project tools/RewardsDev -- api tailscale      # stable phone URL (see below)
dotnet run --project tools/RewardsDev -- identity local     # https://localhost:14330 (all apps)
dotnet run --project tools/RewardsDev -- show --json        # current effective targets (machine-readable)
dotnet run --project tools/RewardsDev -- reset              # remove overrides → committed defaults
```
`api`/`identity` change only their own dimension and leave the other untouched. The same logic
runs head-less (AI / scripts) and from the Aspire dashboard commands. Rebuild the mobile app and
restart `aspire run` (AdminUI refresh + WebAPI) to pick up a change.

> Code lives in `tools/RewardsDev/`: `Core/` (presets, repo paths, state, process helpers) and
> `Apps/` (one config writer per app — `MobileConfig`, `AdminUiConfig`, `WebApiConfig`).

## Build & run the mobile app
Aspire does **not** run the MAUI app — it runs on an emulator/device the usual way.
1. Install the MAUI workload once (above).
2. Pick a backend: `dotnet run --project tools/RewardsDev -- env staging` (emulators can't reach
   `localhost`, so use `staging` or `api tailscale` — not `local` — when running on a device/emulator).
3. Ensure Firebase config exists (git-ignored): grab `google-services.json` /
   `GoogleService-Info.plist` from Keeper, or use the **Materialize Firebase secrets** dashboard command.
4. Build + deploy to a running emulator:
   ```bash
   dotnet build src/MobileUI/MobileUI.csproj -t:Run -f net10.0-android -c Debug -p:AdbTarget="-s <emulator-id>"
   ```
   (`-t:Run` pushes the Fast-Deployment assemblies; a plain `adb install` of the Debug APK won't run.)

## Phone dev with Tailscale (stable URL, real HTTPS cert)
Dev tunnels / ngrok give a new URL every session and an untrusted cert on iOS. Tailscale fixes both:
1. Install Tailscale on the Mac **and** the phone; sign both into the same tailnet (`tailscale up`).
2. Give the phone a trusted HTTPS endpoint to the API:
   ```bash
   tailscale serve --bg https / https+insecure://localhost:5001
   ```
   This terminates TLS with a real Let's Encrypt cert for your `*.ts.net` name — iOS trusts it, no
   dev-cert sideloading.
3. Point the mobile app at the stable hostname **once**:
   ```bash
   dotnet run --project tools/RewardsDev -- api tailscale
   ```
   (auto-detects your MagicDNS name, e.g. `https://<machine>.<tailnet>.ts.net:5001`).
No more per-session URL churn in `Constants.cs`.

## Notes / gotchas
- `aspire run` must run with the **Development** environment so user-secrets load (otherwise secret
  parameters report *ValueMissing* and SQL never starts). The committed
  `src/AppHost/Properties/launchSettings.json` sets this; if you launch differently, export
  `DOTNET_ENVIRONMENT=Development`.
- `Aspire.AppHost.Sdk` is pinned to **13.4.6** — older versions are rejected by the DCP runtime.
- `IInteractionService` (the command-time masked prompts) is experimental → `ASPIREINTERACTION001`
  is suppressed in the AppHost csproj.
- `docker-compose.yml` / `up.ps1` can be retired for local dev (kept for now if any pure-container
  CI path still relies on them).
