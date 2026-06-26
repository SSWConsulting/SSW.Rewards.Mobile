# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

SSW Rewards Mobile is a .NET MAUI mobile application with a .NET 10 backend API and Blazor admin portal. The app allows users to scan QR codes at SSW events, earn points, complete quizzes, and redeem rewards. The project follows Clean Architecture principles and uses a CQRS pattern with MediatR.

## Key Technologies

- **Backend**: .NET 10.0, ASP.NET Core Web API, Entity Framework Core
- **Mobile App**: .NET MAUI (iOS & Android) targeting net10.0-ios and net10.0-android
- **Admin Portal**: Blazor Server
- **Database**: SQL Server (with Azurite for blob storage in development)
- **Authentication**: SSW.Identity (external service)
- **Architecture**: Clean Architecture with CQRS pattern
- **Testing**: NUnit, FluentAssertions, Moq
- **SDK Version**: 10.0.301 (pinned in `global.json`, no `workloadVersion` pin)
- **Local orchestration**: .NET Aspire (`src/AppHost`) — runs SQL + Azurite + WebAPI + AdminUI

## Development Setup Commands

### Initial Setup

```bash
# Clone and setup development environment
git clone https://github.com/SSWConsulting/SSW.Rewards.Mobile.git
cd SSW.Rewards.Mobile

# Install the Aspire CLI (one-time)
dotnet tool install -g aspire   # or: dotnet tool update -g aspire

# Trust the dev HTTPS cert (one-time; also available as a dashboard command)
dotnet dev-certs https --trust

# Start the full local stack (SQL + Azurite + WebAPI + AdminUI) — Docker must be running.
# First run prompts once for the secret parameters (stored in the AppHost user-secrets).
cd src/AppHost && aspire run
```

> Full local-dev guide: [_docs/Aspire-Local-Dev.md](_docs/Aspire-Local-Dev.md). Replaces the
> old `up.ps1` + `docker compose` flow.

### Build Commands

```bash
# Build entire solution
dotnet build SSW.Rewards.sln

# Build specific projects
dotnet build src/WebAPI/WebAPI.csproj
dotnet build src/MobileUI/MobileUI.csproj
dotnet build src/AdminUI/AdminUI.csproj

# Clean and rebuild
dotnet clean SSW.Rewards.sln
dotnet build SSW.Rewards.sln
```

### Testing Commands

```bash
# Run all tests
dotnet test

# Run specific test projects
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj
dotnet test tests/Domain.UnitTests/Domain.UnitTests.csproj
dotnet test tests/Application.IntegrationTests/Application.IntegrationTests.csproj

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run a specific test
dotnet test --filter "TestName"
dotnet test --filter "ClassName"
```

### Running the Stack (Aspire)

```bash
# Start the full local stack: SQL + Azurite + WebAPI + AdminUI (Docker must be running)
cd src/AppHost && aspire run

# Stop: Ctrl+C in the aspire run terminal. SQL/Azurite are persistent containers and
# keep their data volumes; stop them from the Aspire dashboard or `docker stop <container>`.
# Per-resource logs, traces, and command buttons are in the auto-opened dashboard.
```

### Mobile Development

```bash
# Point the app at a backend (emulators can't reach localhost → use staging or tailscale):
dotnet run --project tools/RewardsDev -- env staging        # do NOT hand-edit Constants.cs

# Build + deploy to a running emulator (-t:Run pushes Fast-Deployment assemblies):
dotnet build src/MobileUI/MobileUI.csproj -t:Run -f net10.0-android -c Debug

# Install MAUI workloads (one-time; needs sudo on a system-wide .NET install)
dotnet workload install maui
```

### Database Commands

```bash
# Create EF migration
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/WebAPI

# Update database
dotnet ef database update --project src/Infrastructure --startup-project src/WebAPI

# Drop database
dotnet ef database drop --project src/Infrastructure --startup-project src/WebAPI
```

## Architecture Overview

### Clean Architecture Layers

The solution follows Clean Architecture with clear separation of concerns:

1. **Domain** (`src/Domain/`): Core business entities, value objects, and domain events
   - Entities (User, Achievement, Reward, Quiz, etc.)
   - Common base classes (BaseEntity, BaseAuditableEntity, ValueObject)
   - Domain events and business rules

2. **Application** (`src/Application/`): Business logic and use cases
   - CQRS commands and queries using MediatR
   - Application services and interfaces
   - DTOs and view models
   - Cross-cutting behaviors (validation, logging, performance, authorization)

3. **Infrastructure** (`src/Infrastructure/`): External concerns implementation
   - Entity Framework data access
   - External service integrations
   - File storage, email services, etc.

4. **Presentation Layer**:
   - **WebAPI** (`src/WebAPI/`): REST API controllers
   - **AdminUI** (`src/AdminUI/`): Blazor Server admin portal
   - **MobileUI** (`src/MobileUI/`): .NET MAUI mobile app
   - **ApiClient** (`src/ApiClient/`): Shared API client library

### CQRS Pattern

The application uses CQRS (Command Query Responsibility Segregation) with MediatR:

- **Commands**: Modify state (Create, Update, Delete operations)
- **Queries**: Read data without side effects
- **Handlers**: Process commands and queries
- **Behaviors**: Cross-cutting concerns (validation, logging, authorization)

Example structure:

```
Application/
├── Users/
│   ├── Commands/
│   │   ├── CreateUser/
│   │   │   ├── CreateUserCommand.cs
│   │   │   └── CreateUserCommandHandler.cs
│   └── Queries/
│       ├── GetUser/
│       │   ├── GetUserQuery.cs
│       │   └── GetUserQueryHandler.cs
```

### Key Architectural Patterns

- **Repository Pattern**: Through EF Core DbContext
- **Unit of Work**: EF Core handles transactions
- **Dependency Injection**: Extensive use throughout all layers
- **Domain Events**: For decoupled domain logic
- **Specification Pattern**: For complex query logic
- **Result Pattern**: For handling success/failure states

## Development Guidelines

### Code Organization

- Follow Clean Architecture layering
- Use CQRS for all business operations
- Place DTOs in `Shared` project for cross-layer communication
- Keep controllers thin - delegate to MediatR handlers
- Use soft delete patterns for data retention and audit trails

### Naming Conventions

- Commands: `CreateUserCommand`, `UpdateRewardCommand`
- Queries: `GetUserQuery`, `GetRewardsListQuery`
- Handlers: `CreateUserCommandHandler`, `GetUserQueryHandler`
- DTOs: `UserDto`, `RewardListDto`

### Database Context Usage

- Use `IApplicationDbContext` interface in Application layer
- Tag queries with `.TagWithContext("MethodName")` for debugging
- Use `AsNoTracking()` for read-only queries
- Include related entities explicitly with `.Include()`

### Mobile Development Specifics

- Update `Constants.cs` with dev tunnel URL for local API testing
- Use dependency injection pattern consistently
- Follow MVVM pattern with CommunityToolkit.Mvvm
- Handle platform-specific code in Platforms folders

### Testing Guidelines

- Unit tests for domain logic and application handlers
- Integration tests for API endpoints
- Use FluentAssertions for readable test assertions
- Mock external dependencies with Moq
- Test both success and failure scenarios

### Performance Considerations

- Use async/await consistently
- Leverage EF Core query optimization
- Implement caching where appropriate (CacheKeys.cs)
- Use pagination for list queries
- Monitor query performance with EF Core logging
- Consider offline capabilities for mobile features
- Implement proper race condition handling for concurrent operations

## Common Development Patterns

### Adding a New Feature

1. Create domain entity if needed (`src/Domain/Entities/`)
2. Add EF configuration (`src/Infrastructure/Persistence/Configurations/`)
3. Create application commands/queries (`src/Application/`)
4. Add API controller endpoint (`src/WebAPI/Controllers/`)
5. Update mobile UI if needed (`src/MobileUI/`)
6. Add comprehensive tests

### Adding a New API Endpoint

1. Create command/query in Application layer
2. Add handler with proper validation
3. Create controller action in WebAPI
4. Update API client if used by mobile app
5. Add integration tests

### Database Schema Changes

1. Modify entities in Domain layer
2. Add/update EF configurations
3. Create migration: `dotnet ef migrations add MigrationName`
4. Update database: `dotnet ef database update`
5. Update seed data if necessary

## Environment Variables & Secrets

### Secret Parameters (AppHost)

Secrets now live in the **AppHost** user-secrets (not `src/WebAPI`). WebAPI/AdminUI no longer
carry their own `UserSecretsId`; Aspire injects config into them at run time.

`aspire run` prompts once for any missing parameter. To seed non-interactively:

```bash
dotnet user-secrets set --id F76E3E10-FABB-4543-B949-549EEC500823 "Parameters:<name>" "<value>"
# names: sql-sa-password, firebase-credentials, sendgrid-api-key, email-user, email-password,
#        signing-authority, mobile-google-services-json, mobile-google-service-info-plist
```

Get actual values from: **Client Secrets | SSW | SSW.Rewards | Developer Secrets** in Keeper.

### Development Certificates

Trust the local HTTPS cert with `dotnet dev-certs https --trust` (also available as the
**Tools: Trust dev HTTPS cert** dashboard command). Aspire manages the per-resource certs; the
old `~/.aspnet/https/WebAPI.pfx` from `up.ps1` is no longer required.

### HangFire Database

The HangFire database (`ssw.rewards.hangfire`) is provisioned by the AppHost alongside the main
`ssw.rewards` database when you run `aspire run`.

## Recent Features & Improvements

The project has undergone several major updates recently:

- **Notification System**: Enhanced notification management with soft-delete functionality
- **Mobile Offline Support**: Added offline capabilities for Redeem and Profile pages
- **UI/UX Improvements**: Enhanced mobile notification UI and various layout improvements
- **Performance Optimizations**: Improved performance for claiming rewards and reduced API noise
- **Security**: Added security.txt and PGP public key for enhanced security compliance
- **Mobile Enhancements**: Added search functionality to Redeem page, improved quiz layouts, and consolidated profile activities

## Troubleshooting

### Common Issues

- **Certificate errors**: Recreate dev certificates with `dotnet dev-certs https --clean` then `--trust`
- **Mobile build failures**: Install the MAUI workload — `dotnet workload install maui` (or `maui-android`)
- **`MSB4242` / workload-version errors**: `global.json` must pin SDK `10.0.301` with no `workloadVersion`
- **Database connection issues**: Ensure Docker is running and `aspire run` shows SQL healthy in the dashboard
- **Mobile API access**: Set the target with `dotnet run --project tools/RewardsDev -- api <staging|tailscale>` (don't hand-edit `Constants.cs`); emulators can't reach `localhost`
- **HangFire setup issues**: The HangFire database is provisioned by the AppHost on `aspire run`

### Useful Debugging

- Aspire dashboard: opens automatically on `aspire run` (resource graph, logs, traces, commands)
- API Swagger: https://localhost:5001/swagger/index.html
- Admin UI: https://localhost:7137
- Database: SA user, mapped port shown on the `rewards-sql` resource in the dashboard; password is the AppHost `sql-sa-password` parameter (`docker ps` to find the container + host port)
- Container logs: view per-resource in the Aspire dashboard, or `docker logs <container>`
- HangFire Dashboard: Available through the API when running
- Azurite Storage Explorer: Connect to `DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;`

## Related Documentation

- [F5 Experience Setup](_docs/Instructions-Compile.md)
- [Technologies & Architecture](_docs/Technologies-and-Architecture.md)
- [Developer Guidelines](_docs/Developer_Guidelines.MD)
- [Definition of Done](_docs/Definition-of-Done.md)
- [Definition of Ready](_docs/Definition-of-Ready.md)
- [Business Overview](_docs/Business.md)
