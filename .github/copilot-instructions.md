# GitHub Copilot Instructions for SSW.Rewards.Mobile

> 💡 **For comprehensive AI agent context and detailed guidelines, see [`AGENTS.md`](../AGENTS.md) in the repository root.**

This file provides GitHub Copilot with essential context about the SSW.Rewards.Mobile project to generate better, context-aware code suggestions.

## Quick Reference

**Project Type**: .NET 10 MAUI Mobile App + Blazor WASM Admin + ASP.NET Core API  
**Architecture**: Clean Architecture with CQRS (MediatR)  
**Database**: SQL Server with EF Core  
**Key Technologies**: .NET MAUI, Blazor, MediatR, FluentValidation, Entity Framework Core

## Core Principles

1. **Clean Architecture**: Respect layer boundaries (Domain → Application → Infrastructure → Presentation)
2. **CQRS Pattern**: Use MediatR commands for mutations, queries for reads
3. **Modern C#**: File-scoped namespaces, primary constructors, collection expressions, pattern matching
4. **Minimal Changes**: Make surgical, targeted changes only to what's necessary

## Critical EF Core Patterns

**ALWAYS** follow this pattern for EF Core queries:

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

// ❌ WRONG: Using Include for read-only queries
var orders = await _context.Orders
    .Include(o => o.Customer)          // Avoid this!
    .ThenInclude(c => c.Address)       // Causes cartesian explosion
    .ToListAsync();
```

**Rules**:

1. ✅ **Always** `.AsNoTracking()` for read-only queries
2. ✅ **Always** `.TagWithContext()` after `.AsNoTracking()`
3. ✅ **Always** `.Select()` to project, **never** `Include`/`ThenInclude` for read-only
4. ✅ Only use `Include` when updating entities

## Pagination

Use the correct pagination method:

```csharp
// ✅ For EF Core queries (IQueryable) - use async
var result = await _context.Users
    .AsNoTracking()
    .TagWithContext("UserList")
    .Where(u => u.Active)
    .ToPaginatedResultAsync<UserListViewModel, UserDto>(request, ct);

// ✅ For in-memory data (IEnumerable/List) - use sync
var sorted = cachedUsers.OrderByDescending(u => u.Points);
var result = sorted.ToPaginatedResult<LeaderboardViewModel, UserDto>(request);
```

## MAUI MVVM Pattern

```csharp
// ViewModel with CommunityToolkit.Mvvm
public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _username;

    [RelayCommand]
    private async Task LoadDataAsync() { /* ... */ }
}

// XAML with compiled bindings
<ContentPage x:DataType="vm:HomeViewModel">
    <Label Text="{Binding Username}" />
    <Button Command="{Binding LoadDataCommand}" />
</ContentPage>
```

## Naming Conventions

- **Commands**: `{Action}{Entity}Command` (e.g., `CreateUserCommand`)
- **Queries**: `Get{Entity}{Optional}Query` (e.g., `GetUserByIdQuery`)
- **Handlers**: `{Command/Query}Handler` implementing `IRequestHandler<TRequest, TResponse>`
- **DTOs**: `{Entity}Dto` or `{Entity}ViewModel`
- **PascalCase**: Classes, methods, properties, public fields
- **camelCase**: Parameters, local variables
- **\_camelCase**: Private fields
- **Async suffix**: All async methods

## Common Commands

```bash
# Build solution
dotnet build SSW.Rewards.sln

# Run tests
dotnet test

# Create EF migration
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/WebAPI

# Run API locally
dotnet run --project src/WebAPI

# Run Admin Portal locally
dotnet run --project src/AdminUI
```

## Testing

- **Unit Tests**: NUnit, FluentAssertions, Moq
- **Integration Tests**: WebApplicationFactory, TestContainers
- **Always** validate that changes don't break existing tests
- **Always** run linters, builds, and tests before finalizing

## Important Guidelines

1. **Never** delete or modify working code unnecessarily
2. **Never** use `.Result` or `.Wait()` - async all the way
3. **Always** validate commands with FluentValidation
4. **Always** use file-scoped namespaces
5. **Always** tag EF queries with `.TagWithContext()`
6. **Ignore** unrelated bugs or broken tests - not your responsibility

## Documentation

See [`AGENTS.md`](../AGENTS.md) for:

- Complete technology stack details
- Full architecture documentation
- Detailed coding standards
- Development workflow
- Troubleshooting guides
- Deployment information

## Resources

- **F5 Experience**: [`_docs/Instructions-Compile.md`](../_docs/Instructions-Compile.md)
- **Architecture**: [`_docs/Technologies-and-Architecture.md`](../_docs/Technologies-and-Architecture.md)
- **Guidelines**: [`_docs/Developer_Guidelines.MD`](../_docs/Developer_Guidelines.MD)
- **DoD**: [`_docs/Definition-of-Done.md`](../_docs/Definition-of-Done.md)

---

**Last Updated**: December 2025 | **Target Framework**: .NET 10.0 | **MAUI Version**: 10.0.x
