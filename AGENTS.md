# AI Agent Configuration for SSW.Rewards.Mobile

This file provides AI agents (GitHub Copilot, Cursor, Windsurf, etc.) with essential context about the SSW Rewards Mobile project to generate better, context-aware code suggestions.

## 📱 Project Overview

**SSW Rewards Mobile** is a gamified engagement platform that allows users to scan QR codes at SSW events, earn points, complete AI-driven quizzes, and redeem rewards. The app connects the outside world with SSW through an engaging rewards system.

- **Mobile App**: .NET MAUI (iOS & Android)
- **Admin Portal**: Blazor WebAssembly with MudBlazor
- **Backend API**: ASP.NET Core Web API (.NET 9)
- **Database**: SQL Server with EF Core
- **Authentication**: SSW.Identity (OIDC/OAuth2)
- **Architecture**: Clean Architecture with CQRS (MediatR)

## 🎯 Key Features

- QR code scanning for earning points at events
- AI-driven quiz system for additional points
- Points-based reward redemption
- Social networking with staff profiles (LinkedIn integration)
- Real-time leaderboards (daily, weekly, monthly, all-time)
- Activity feeds and notifications
- Offline support for redeem and profile pages
- Push notifications for prize draws

## 🏗️ Architecture & Structure

### Solution Organization

```
SSW.Rewards.Mobile/
├── src/
│   ├── Domain/               # Entities, value objects, domain events
│   ├── Application/          # CQRS handlers, services, DTOs
│   ├── Infrastructure/       # EF Core, external services
│   ├── WebAPI/               # REST API controllers
│   ├── AdminUI/              # Blazor WASM admin portal
│   ├── MobileUI/             # .NET MAUI mobile app
│   ├── ApiClient/            # Shared API client
│   ├── Common/               # Shared utilities
│   └── SSW.Rewards.Enums/    # Shared enumerations
├── tests/
│   ├── Domain.UnitTests/
│   ├── Application.UnitTests/
│   ├── Application.IntegrationTests/
│   └── WebAPI.IntegrationTests/
├── infra/                    # Bicep IaC templates
└── _docs/                    # Project documentation
```

### Clean Architecture Layers

1. **Domain** (`src/Domain/`)

   - Core entities: `User`, `Achievement`, `Reward`, `Quiz`, `UserAchievement`, `Leaderboard`
   - Base classes: `BaseEntity`, `BaseAuditableEntity`, `ValueObject`
   - Domain events for decoupled business logic
   - **No dependencies** on other layers

2. **Application** (`src/Application/`)

   - CQRS commands/queries with MediatR
   - Interfaces: `IApplicationDbContext`, `IDateTime`, `ICacheService`, etc.
   - DTOs and ViewModels in `Shared` project
   - Behaviors: validation (FluentValidation), logging, performance, authorization
   - Services: `LeaderboardService`, `UserService`, `QuizService`

3. **Infrastructure** (`src/Infrastructure/`)

   - `ApplicationDbContext` (EF Core)
   - Entity configurations in `Persistence/Configurations/`
   - External services: storage, email, notifications
   - Migrations in `Persistence/Migrations/`

4. **Presentation Layers**
   - **WebAPI**: REST controllers, minimal logic (delegates to MediatR)
   - **AdminUI**: Blazor WASM, MudBlazor components
   - **MobileUI**: .NET MAUI, MVVM with CommunityToolkit.Mvvm

### CQRS Pattern with MediatR

```
Application/Achievements/
├── Commands/
│   ├── CreateAchievement/
│   │   ├── CreateAchievementCommand.cs          # Record with properties
│   │   ├── CreateAchievementCommandValidator.cs # FluentValidation
│   │   └── CreateAchievementCommandHandler.cs   # IRequestHandler<,>
│   └── PostAchievement/
└── Queries/
    └── GetAchievements/
        ├── GetAchievementsQuery.cs
        └── GetAchievementsQueryHandler.cs
```

**Naming conventions**:

- Commands: `{Action}{Entity}Command` (e.g., `CreateUserCommand`)
- Queries: `Get{Entity}{Optional}Query` (e.g., `GetUserByIdQuery`)
- Handlers: `{Command/Query}Handler` (implements `IRequestHandler<TRequest, TResponse>`)
- DTOs: `{Entity}Dto` or `{Entity}ViewModel`

## 🛠️ Technology Stack

### Backend (.NET 9)

- **Framework**: ASP.NET Core 9.0, Minimal APIs
- **ORM**: Entity Framework Core 9.0 (SQL Server provider)
- **CQRS**: MediatR 12.x
- **Validation**: FluentValidation 11.x
- **Mapping**: AutoMapper (limited use, prefer manual mapping)
- **Caching**: In-memory cache, distributed cache support
- **Background Jobs**: Hangfire
- **Authentication**: OpenIdConnect with SSW.Identity

### Frontend (Blazor WASM)

- **Framework**: Blazor WebAssembly (.NET 9)
- **UI Library**: MudBlazor 6.x
- **HTTP**: Custom API client with auth handler
- **State**: Scoped services per session

### Mobile (.NET MAUI)

- **Framework**: .NET MAUI (.NET 9), targets `net9.0-ios` and `net9.0-android`
- **Architecture**: MVVM with `CommunityToolkit.Mvvm`
- **Navigation**: Shell-based navigation
- **DI**: Built-in .NET DI container
- **UI**: XAML with compiled bindings (`x:DataType`)
- **Firebase**: Analytics, Crashlytics, Cloud Messaging (push notifications)
- **Packages**:
  - `CommunityToolkit.Maui` & `CommunityToolkit.Mvvm`
  - `FFImageLoading.Maui`
  - `BarcodeScanning.Native.Maui`
  - `Mopups` (modal popups)

### Database

- **Primary**: SQL Server (Azure SQL Database)
- **Blob Storage**: Azure Blob Storage (profile pics, assets)
- **Caching**: Redis (distributed cache for production)
- **Migrations**: EF Core migrations

## 📋 Coding Standards & Best Practices

### C# 13 & .NET 9 Conventions

```csharp
// File-scoped namespaces (always)
namespace SSW.Rewards.Application.Users;

// Primary constructors
public class UserService(IApplicationDbContext context, IDateTime dateTime) : IUserService
{
    // Use records for DTOs
    public record UserDto(int Id, string Name, string Email);

    // Collection expressions
    var userIds = [1, 2, 3, 4];

    // Pattern matching
    var status = user switch
    {
        { IsActive: true, Points: > 100 } => "Premium",
        { IsActive: true } => "Active",
        _ => "Inactive"
    };
}
```

**Naming**:

- PascalCase: classes, methods, properties, public fields
- camelCase: parameters, local variables
- `_camelCase`: private fields
- `I` prefix: interfaces
- `T` prefix: generic type parameters
- `Async` suffix: async methods

### Entity Framework Core Query Patterns

**ALWAYS follow this pattern**:

```csharp
// ✅ CORRECT: Read-only query with projection
var users = await _context.Users
    .AsNoTracking()                    // First: no tracking for read-only
    .TagWithContext("ActiveUsers")     // Second: tag for performance monitoring
    .Where(u => u.IsActive)
    .Select(u => new UserDto           // Third: project to DTO
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email
    })
    .ToListAsync(cancellationToken);

// ✅ CORRECT: Multiple queries in one handler
var user = await _context.Users
    .AsNoTracking()
    .TagWithContext("GetUser")         // Custom tag for first query
    .FirstOrDefaultAsync(u => u.Id == userId, ct);

var orders = await _context.Orders
    .AsNoTracking()
    .TagWithContext("GetUserOrders")   // Custom tag for second query
    .Where(o => o.UserId == userId)
    .ToListAsync(ct);
// SQL comments: "HandlerName-MethodName-GetUser", "HandlerName-MethodName-GetUserOrders"

// ❌ WRONG: Using Include for read-only queries
var orders = await _context.Orders
    .Include(o => o.Customer)
    .ThenInclude(c => c.Address)
    .ToListAsync();
// Problem: Loads entire entities, causes cartesian explosion

// ✅ CORRECT: Use Select for nested data
var orders = await _context.Orders
    .AsNoTracking()
    .TagWithContext()
    .Select(o => new OrderDto
    {
        Id = o.Id,
        CustomerName = o.Customer.Name,
        City = o.Customer.Address.City
    })
    .ToListAsync();
```

**Rules**:

1. ✅ **Always** `.AsNoTracking()` for read-only queries
2. ✅ **Always** `.TagWithContext()` after (single query) or `.TagWithContext("CustomName")` (multiple queries)
3. ✅ **Always** `.Select()` to project, **never** `Include`/`ThenInclude` for read-only
4. ✅ Only use `Include` when updating entities or required by domain logic
5. 📁 See: `LeaderboardService.cs`, `GetNotificationHistoryListQuery.cs`

### Pagination Patterns

**Extension**: `src/Application/Common/Extensions/PaginationResultExtensions.cs`

```csharp
// ✅ For EF Core queries (IQueryable) - use async
var result = await _context.Users
    .AsNoTracking()
    .TagWithContext("UserList")
    .Where(u => u.Active)
    .ToPaginatedResultAsync<UserListViewModel, UserDto>(request, ct);
// Executes 2 queries: Count + Paged (both auto-tagged)

// ✅ For in-memory data (IEnumerable/List) - use sync
var users = await leaderboardService.GetFullLeaderboard(ct); // Cached
var sorted = users.OrderByDescending(u => u.Points);
var result = sorted.ToPaginatedResult<LeaderboardViewModel, UserDto>(request);
// No DB calls - paginates already-loaded data
```

**When to use**:

- `ToPaginatedResultAsync`: EF Core `IQueryable<T>` (queries DB efficiently)
- `ToPaginatedResult`: In-memory `IEnumerable<T>` or `List<T>` (cached data)

📁 **Examples**: `GetMobileLeaderboard.cs` (in-memory), `GetNotificationHistoryListQuery.cs` (DB)

### MAUI MVVM Pattern

```csharp
// ViewModel with CommunityToolkit.Mvvm
public partial class HomeViewModel : ObservableObject
{
    private readonly IUserService _userService;

    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    private bool _isLoading;

    public HomeViewModel(IUserService userService)
    {
        _userService = userService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            Username = await _userService.GetCurrentUserAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

**XAML with compiled bindings**:

```xml
<ContentPage xmlns:vm="clr-namespace:SSW.Rewards.Mobile.ViewModels"
             x:DataType="vm:HomeViewModel">
    <Label Text="{Binding Username}" />
    <Button Text="Load" Command="{Binding LoadDataCommand}" />
</ContentPage>
```

### Blazor Component Patterns

```csharp
@inject IUserService UserService

<MudDataGrid Items="@_users" Loading="@_loading">
    <Columns>
        <PropertyColumn Property="x => x.Name" />
        <PropertyColumn Property="x => x.Email" />
    </Columns>
</MudDataGrid>

@code {
    private List<UserDto> _users = [];
    private bool _loading;

    protected override async Task OnInitializedAsync()
    {
        _loading = true;
        _users = await UserService.GetUsersAsync();
        _loading = false;
    }
}
```

## 🚀 Development Workflow

### Getting Started

```bash
# Clone repository
git clone https://github.com/SSWConsulting/SSW.Rewards.Mobile.git
cd SSW.Rewards.Mobile

# Setup environment (creates certificates, starts Docker services)
pwsh ./up.ps1

# Build solution
dotnet build SSW.Rewards.sln

# Run tests
dotnet test
```

### Common Commands

```bash
# API development
dotnet run --project src/WebAPI
# API: https://localhost:5001/swagger

# Admin Portal
dotnet run --project src/AdminUI
# Admin: https://localhost:7137

# Create EF migration
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/WebAPI

# Update database
dotnet ef database update --project src/Infrastructure --startup-project src/WebAPI

# Mobile (Android)
dotnet build src/MobileUI/MobileUI.csproj -f net9.0-android

# Mobile (iOS) - requires macOS
dotnet build src/MobileUI/MobileUI.csproj -f net9.0-ios
```

### Docker Services

```bash
# All services
docker compose --profile all up -d

# Dependencies only (for local dev)
docker compose --profile tools up -d

# View logs
docker compose logs -f rewards-webapi
```

### Testing Strategy

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Testing tools**:

- **Unit**: NUnit, FluentAssertions, Moq
- **Integration**: `WebApplicationFactory<Program>`, TestContainers
- **Data**: Bogus (fake data generation), Respawn (DB cleanup)

## 🔒 Authentication & Security

- **OAuth 2.0 / OIDC** with SSW.Identity (external service)
- **JWT Bearer tokens** for API authentication
- **Secure Storage** for tokens on mobile (MAUI `SecureStorage`)
- **HTTPS only** in production
- **Connection strings** in Azure Key Vault
- **Never log PII** (emails, tokens, passwords)

## 📦 Key Dependencies

### Backend

- `MediatR` (CQRS pattern)
- `FluentValidation.AspNetCore` (validation)
- `Microsoft.EntityFrameworkCore.Tools` (migrations)
- `Microsoft.EntityFrameworkCore.SqlServer` (SQL Server)
- `Swashbuckle.AspNetCore` (Swagger/OpenAPI)
- `Hangfire` (background jobs)

### Mobile

- `CommunityToolkit.Mvvm` (MVVM helpers)
- `CommunityToolkit.Maui` (additional controls)
- `Plugin.Firebase.*` (Analytics, Crashlytics, Messaging)
- `FFImageLoading.Maui` (image caching)
- `BarcodeScanning.Native.Maui` (QR scanning)

### Admin

- `MudBlazor` (Material Design components)
- `Microsoft.AspNetCore.Components.WebAssembly.Authentication` (auth)

## 🎨 Chat Modes & Custom Instructions

This project includes specialized AI chat modes in `.github/chatmodes/`:

1. **`gh-actions.chatmode.md`**: GitHub Actions expert for CI/CD workflows, deployments, debugging
2. **`dotnet-maui-blazor.chatmode.md`**: .NET MAUI, Blazor WASM, EF Core expert with project-specific patterns

Use these chat modes for context-aware assistance with specific technologies.

## 📚 Additional Resources

- **F5 Experience**: [Instructions-Compile.md](_docs/Instructions-Compile.md)
- **Architecture**: [Technologies-and-Architecture.md](_docs/Technologies-and-Architecture.md)
- **Guidelines**: [Developer_Guidelines.MD](_docs/Developer_Guidelines.MD)
- **DoD**: [Definition-of-Done.md](_docs/Definition-of-Done.md)
- **DoR**: [Definition-of-Ready.md](_docs/Definition-of-Ready.md)
- **Business**: [Business.md](_docs/Business.md)
- **Deployment**: [Instructions-Deployment.md](_docs/Instructions-Deployment.md)

## 🐛 Common Issues & Solutions

### Certificate Errors

```bash
# Recreate dev certificates
dotnet dev-certs https --clean
dotnet dev-certs https -ep ${HOME}/.aspnet/https/WebAPI.pfx -p ThisPassword
dotnet dev-certs https --trust
```

### Mobile Build Failures

```bash
# Restore MAUI workloads
dotnet workload restore
dotnet workload update
```

### Database Connection Issues

```bash
# Verify Docker containers are running
docker ps

# Check database connection
docker compose logs -f sqlserver
```

### Mobile API Access

- Use **dev tunnel** for mobile testing: `devtunnel host -p 5001`
- Update `Constants.cs` → `ApiBaseUrl` with tunnel URL
- Ensure HTTPS certificates are trusted

## 🎯 AI Agent Guidelines

When generating code for this project:

1. **Follow Clean Architecture** - respect layer boundaries
2. **Use CQRS** - commands for mutations, queries for reads
3. **Tag all EF queries** - `.AsNoTracking().TagWithContext()`
4. **Project don't Include** - use `Select()` instead of `Include()`
5. **Paginate correctly** - async for DB, sync for in-memory
6. **MVVM for MAUI** - `ObservableObject`, `RelayCommand`, compiled bindings
7. **File-scoped namespaces** - always use modern C# syntax
8. **Async all the way** - never `.Result` or `.Wait()`
9. **Validate commands** - use FluentValidation
10. **Test comprehensively** - unit + integration tests

## 🚢 Deployment

- **Environments**: Dev/Staging, Production (Azure)
- **IaC**: Bicep templates in `infra/`
- **CI/CD**: GitHub Actions (`.github/workflows/`)
- **Mobile**: App Store (iOS), Play Store (Android)
- **Web**: Azure App Service (API + Admin)
- **Database**: Azure SQL Database
- **Storage**: Azure Blob Storage

---

**Last Updated**: October 2025 | **Target Framework**: .NET 9.0 | **MAUI Version**: 9.0.21
