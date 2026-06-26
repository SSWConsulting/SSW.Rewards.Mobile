---
name: setup-admin-api
description: Set up and run the SSW.Rewards backend locally — the WebAPI + AdminUI (Blazor) with SQL Server + Azurite, orchestrated by .NET Aspire. Use when someone wants to get the API / Admin UI running, do a first-time backend setup, or troubleshoot `aspire run`. This is the prerequisite for running the mobile app (see setup-mobile).
---

# Run the backend (WebAPI + AdminUI) with .NET Aspire

One command brings up SQL Server + Azurite + WebAPI + AdminUI. Full guide: `_docs/Aspire-Local-Dev.md`.

## Prerequisites (one-time)

- **.NET 10 SDK** — `global.json` pins `10.0.301` (no `workloadVersion` pin).
- **Aspire CLI ≥ 13.4.6**: `dotnet tool install -g aspire` (or `dotnet tool update -g aspire`).
- **Docker** running (Aspire starts the SQL + Azurite containers).

## Run it

```bash
aspire run   # from the repo root — .aspire/settings.json targets src/AppHost
```

The dashboard opens automatically. SQL Server + Azurite come up as **persistent** containers with
data volumes; WebAPI + AdminUI start once SQL is healthy; EF migrations apply on WebAPI startup.

- **AdminUI** → https://localhost:7137 (redirects to SSW Identity to sign in)
- **WebAPI** → https://localhost:5001 — health `https://localhost:5001/health`, Swagger `/swagger`

## Secrets

On the first `aspire run`, Aspire **prompts once** for the unresolved secret parameters and stores
them in the **AppHost** user-secrets (id `F76E3E10-FABB-4543-B949-549EEC500823`). Get the values from
Keeper (**Client Secrets | SSW | SSW.Rewards | Developer Secrets**). To seed non-interactively
(scripts / CI / `--non-interactive`):

```bash
dotnet user-secrets set --id F76E3E10-FABB-4543-B949-549EEC500823 "Parameters:sql-sa-password" "<pick-a-strong-pw>"
# names: sql-sa-password, firebase-credentials, sendgrid-api-key, email-user, email-password,
#        signing-authority, mobile-google-services-json, mobile-google-service-info-plist
```

Secrets flow **only** from the AppHost — `WebAPI`/`AdminUI` no longer carry their own `UserSecretsId`.
Aspire injects the connection strings (`ConnectionStrings:DefaultConnection`/`:HangfireConnection`),
the Azurite blob string, Firebase/SendGrid/SMTP, and `SigningAuthority` as env vars.

## Dashboard commands (Actions ▸ Commands on `rewards-sql`)

DB: Apply migrations / Add migration… · Install dotnet-ef · MAUI workload restore · Trust dev HTTPS
cert · Materialize Firebase secrets · Switch identity / API target (shells out to `rewards-dev`).

## Switch which identity / API the apps use

`dotnet run --project tools/RewardsDev -- env <local|staging|prod>` (or `identity` / `api`,
`api tailscale`). Writes git-ignored overrides for Mobile + AdminUI + WebAPI. `… -- show` prints current.

## Troubleshooting

- **`sql-sa-password → ValueMissing`, SQL never starts** — secret parameters only resolve in the
  **Development** environment. The committed `src/AppHost/Properties/launchSettings.json` sets it;
  otherwise export `DOTNET_ENVIRONMENT=Development`.
- **`MSB4242` / workload version not found** — make sure you're on the updated `global.json` (SDK
  `10.0.301`, no `workloadVersion` pin).
- **Nothing starts** — Docker isn't running.
- **`Aspire.AppHost.Sdk` rejected** — must be ≥ 13.4.6 (already pinned in the AppHost csproj).
