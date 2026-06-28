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

## Aspire CLI — agent guardrails (distilled)

`aspire run` is the **human** flow (foreground, opens the dashboard, blocks the terminal). When an
**AI agent** drives the AppHost, use the background lifecycle instead so you don't block and so locks
release cleanly. CLI is **≥ 13.4.6**; all verbs below exist there.

```bash
aspire start --non-interactive          # background; --isolated in a git worktree (random ports + isolated secrets)
aspire wait <resource> --non-interactive # block until ready — NEVER curl-poll a health endpoint
aspire ps                               # list running AppHosts/resources (--include-hidden for proxies/migrations)
aspire describe [<resource>]            # inspect state/endpoints  ·  aspire logs <resource>  ·  aspire otel
aspire resource <resource> restart      # one resource changed → restart/rebuild IT, don't bounce the whole AppHost
aspire stop                             # release file locks + ports when done
```

**Rules that prevent agent self-harm:**

- **Never** `dotnet run` an AppHost — it bypasses orchestration (no dashboard, no `wait`/`logs`, orphaned procs). Use `aspire start`/`aspire run`.
- **File-lock build errors (`MSB3491` / `CS2012`, "file in use") = Aspire is running**, not a broken project. Fix: `aspire stop` first, then rebuild. **Don't** delete `bin/`/`obj/`, `kill` dotnet, or "reboot".
- **"Port already in use"** → `aspire stop` then `aspire start`.
- This is a **worktree** — prefer `aspire start --isolated` so it won't collide with another checkout's instance.
- `aspire doctor` diagnoses a broken environment (missing SDK/Docker/cert).

> We deliberately **don't** vendor Microsoft's `aspire-skills` plugin (init/aspireify/deployment are
> irrelevant — our AppHost is wired and we deploy via Azure pipelines, not `aspire publish`). The
> guardrails above are the only genuinely useful distillation. Devs who want full Aspire CLI fluency
> can install it per-machine: `copilot plugin install aspire@aspire-skills`.

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

## Dashboard commands

**On `rewards-sql`** (DB + tooling): DB: Apply migrations / Add migration… · Install dotnet-ef ·
Trust dev HTTPS cert.

**On `mobile-app`** (virtual lifetime-less resource for the MAUI app): Show current target ·
Switch API / identity target… · API → Tailscale (one-click) · Tailscale: Status · Materialize
Firebase secrets · MAUI workload restore. The switch commands shell out to `rewards-dev`.

## Switch which identity / API the apps use

`./rewards-dev env <local|staging|prod>` (or `identity` / `api`, `api tailscale`) from the repo root.
Writes git-ignored overrides for Mobile + AdminUI + WebAPI. `./rewards-dev show --json` prints the
current effective targets (machine-readable); `./rewards-dev help` is fully self-teaching.
`./rewards-dev` is a wrapper over `dotnet run --project tools/RewardsDev`.

## Troubleshooting

- **`sql-sa-password → ValueMissing`, SQL never starts** — secret parameters only resolve in the
  **Development** environment. The committed `src/AppHost/Properties/launchSettings.json` sets it;
  otherwise export `DOTNET_ENVIRONMENT=Development`.
- **`MSB4242` / workload version not found** — make sure you're on the updated `global.json` (SDK
  `10.0.301`, no `workloadVersion` pin).
- **Nothing starts** — Docker isn't running.
- **`Aspire.AppHost.Sdk` rejected** — must be ≥ 13.4.6 (already pinned in the AppHost csproj).
