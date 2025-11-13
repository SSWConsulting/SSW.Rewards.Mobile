# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

SSW Rewards Mobile is a .NET MAUI mobile application with a .NET 9 backend API and Blazor admin portal. The app allows users to scan QR codes at SSW events, earn points, complete quizzes, and redeem rewards. The project follows Clean Architecture principles and uses a CQRS pattern with MediatR.

## Key Technologies

- **Backend**: .NET 9.0, ASP.NET Core Web API, Entity Framework Core
- **Mobile App**: .NET MAUI (iOS & Android) targeting net9.0-ios and net9.0-android
- **Admin Portal**: Blazor Server
- **Database**: SQL Server (with Azurite for blob storage in development)
- **Authentication**: SSW.Identity (external service)
- **Architecture**: Clean Architecture with CQRS pattern
- **Testing**: NUnit, FluentAssertions, Moq
- **SDK Version**: 9.0.305

## Development Setup Commands

### Initial Setup
```bash
# Clone and setup development environment
git clone https://github.com/SSWConsulting/SSW.Rewards.Mobile.git
cd SSW.Rewards.Mobile

# Create dev certificates (required for HTTPS in Docker)
# Windows:
dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\WebAPI.pfx -p ThisPassword
# macOS/Linux:
dotnet dev-certs https -ep ${HOME}/.aspnet/https/WebAPI.pfx -p ThisPassword
dotnet dev-certs https --trust

# Start development environment
pwsh ./up.ps1
```

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

### Docker Commands
```bash
# Start all services (API, Admin UI, SQL Server, Azurite)
docker compose --profile all up -d

# Start only dependencies (SQL Server + Azurite for local API/Admin development)
docker compose --profile tools up -d

# Start only WebAPI (includes SQL Server + Azurite)
docker compose --profile webapi up -d

# Start only Admin UI (includes SQL Server, Azurite, and WebAPI)
docker compose --profile admin up -d

# View logs
docker compose logs -f rewards-webapi
docker compose logs -f rewards-adminui

# Stop all services
docker compose down

# Rebuild images
docker compose build
```

### Mobile Development
```bash
# For Android (requires dev tunnel for API access)
devtunnel host -p 5001
# Update Constants.cs ApiBaseUrl with tunnel address
dotnet build src/MobileUI/MobileUI.csproj -f net9.0-android

# Restore MAUI workloads (if needed)
dotnet workload update
dotnet workload restore
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

### Required User Secrets (WebAPI)
Add via: `dotnet user-secrets set "Key" "Value" --project src/WebAPI`

Get actual values from: **Client Secrets | SSW | SSW.Rewards | Developer Secrets** in Keeper

### Development Certificates
Required for HTTPS in Docker. Created via `up.ps1` script or manually:
- Location: `~/.aspnet/https/WebAPI.pfx`
- Password: `ThisPassword`

### HangFire Database
HangFire database (`ssw.rewards.hangfire`) is automatically created by the `up.ps1` script if it doesn't exist.

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
- **Certificate errors**: Recreate dev certificates with `dotnet dev-certs https --clean`
- **Mobile build failures**: Run `dotnet workload restore`
- **Database connection issues**: Ensure Docker containers are running
- **Mobile API access**: Verify dev tunnel is running and Constants.cs is updated
- **HangFire setup issues**: The `up.ps1` script automatically creates the HangFire database

### Useful Debugging
- API Swagger: https://localhost:5001/swagger/index.html
- Admin UI: https://localhost:7137
- Database: Connect to `localhost,1433` with SA user (password: `Rewards.Docker1!`)
- Container logs: `docker compose logs -f [service-name]`
- HangFire Dashboard: Available through the API when running
- Azurite Storage Explorer: Connect to `DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;`

## Related Documentation

- [F5 Experience Setup](_docs/Instructions-Compile.md)
- [Technologies & Architecture](_docs/Technologies-and-Architecture.md)
- [Developer Guidelines](_docs/Developer_Guidelines.MD)
- [Definition of Done](_docs/Definition-of-Done.md)
- [Definition of Ready](_docs/Definition-of-Ready.md)
- [Business Overview](_docs/Business.md)
