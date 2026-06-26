using SSW.Rewards.AppHost.Hosts;

// =====================================================================
// SSW.Rewards — .NET Aspire local-dev AppHost
//
// Replaces docker-compose + up.ps1 + hand-edited config for local dev.
// This file stays thin: it wires the hosts together. Each host file under
// Hosts/ owns the resource(s) and secrets only it consumes.
// =====================================================================

var builder = DistributedApplication.CreateBuilder(args);

// Shared infra: SQL Server (persistent) + the two databases + Azurite blob storage.
var common = builder.AddCommonInfra();

// WebAPI owns its own secrets (Firebase / SendGrid / SMTP / identity authority),
// injected alongside the connection strings from the common infra.
var api = builder.AddWebApiHost(common);

// AdminUI (Blazor WASM dev server) points at the API.
builder.AddAdminUiHost(api);

// Dashboard command buttons for the common dev chores (EF migrate, materialize
// mobile secrets, switch identity/API, …) — grouped on the SQL resource.
common.SqlServer.AddDevCommands();

builder.Build().Run();
