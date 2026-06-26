# SSW.Rewards — Local dev with .NET Aspire

This replaces `docker-compose.yml` + `up.ps1` + hand-edited secrets/URLs for local development.
The MAUI mobile app still runs the normal way (emulator/device) — Aspire's job for mobile is
config/secret **materialization** and tunnel wiring, not running the app.

## Prerequisites (one-time)
- .NET 10 SDK (repo pins via `global.json`).
- Aspire CLI ≥ **13.4.6**: `dotnet tool install -g aspire` (or `dotnet tool update -g aspire`).
- Docker running.

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
- **Mobile: Switch identity / API target…** — shells out to the `rewards-dev` CLI (below)

## Mobile config switching — the `rewards-dev` CLI
Stop hand-editing `Constants.cs`. The DEBUG API/identity URLs come from a **git-ignored**
`src/MobileUI/Constants.LocalDev.cs`; when absent (fresh clone / CI) the committed
`Constants.LocalDev.Default.cs` supplies safe defaults.

```bash
dotnet run --project tools/RewardsDev -- api local        # https://localhost:5001
dotnet run --project tools/RewardsDev -- api staging
dotnet run --project tools/RewardsDev -- api tailscale     # stable phone URL (see below)
dotnet run --project tools/RewardsDev -- identity local    # https://localhost:14330
dotnet run --project tools/RewardsDev -- show              # print current
dotnet run --project tools/RewardsDev -- reset             # back to committed defaults
```
The same logic is callable head-less (AI / scripts) and from the Aspire dashboard commands.
Rebuild the mobile app to pick up a change.

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
