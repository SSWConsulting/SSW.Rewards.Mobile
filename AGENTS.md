# AI Agent Configuration for SSW.Rewards.Mobile

Gamified engagement platform: .NET MAUI mobile app + Blazor WASM admin + ASP.NET Core API.

## Essentials

- **Framework**: .NET 10 | **Architecture**: Clean Architecture with CQRS (MediatR)
- **Build**: `dotnet build SSW.Rewards.sln` | **Test**: `dotnet test`
- **Setup**: `pwsh ./up.ps1` (creates certs, starts Docker)

## Critical Rules

1. **EF Core queries**: Always `.AsNoTracking().TagWithContext()` then `.Select()` — never `Include` for read-only
2. **Async**: Never `.Result` or `.Wait()`
3. **Commands**: Validate with FluentValidation
4. **File-scoped namespaces**: Always

## Detailed Patterns

| Topic | File |
|-------|------|
| **EF Core Queries** ⚠️ | [ef-core-patterns.md](_docs/agents/ef-core-patterns.md) |
| CQRS & MediatR | [cqrs-patterns.md](_docs/agents/cqrs-patterns.md) |
| .NET MAUI / MVVM | [maui-patterns.md](_docs/agents/maui-patterns.md) |
| Blazor Components | [blazor-patterns.md](_docs/agents/blazor-patterns.md) |
| AdminUI Styling ⚠️ | [adminui-styling.md](_docs/agents/adminui-styling.md) |
| Dev Workflow | [dev-workflow.md](_docs/agents/dev-workflow.md) |
| Testing | [testing.md](_docs/agents/testing.md) |
| Troubleshooting | [troubleshooting.md](_docs/agents/troubleshooting.md) |

## Solution Structure

```
src/
├── Domain/          # Entities, value objects (no dependencies)
├── Application/     # CQRS handlers, services, DTOs
├── Infrastructure/  # EF Core, external services
├── WebAPI/          # REST controllers → MediatR
├── AdminUI/         # Blazor WASM + MudBlazor
└── MobileUI/        # .NET MAUI + CommunityToolkit.Mvvm
```

## Resources

- [Instructions-Compile.md](_docs/Instructions-Compile.md) — F5 experience
- [Instructions-Deployment.md](_docs/Instructions-Deployment.md) — Deployment guide
- [Technologies-and-Architecture.md](_docs/Technologies-and-Architecture.md) — Architecture diagrams

---

**Last Updated**: January 2026 | **.NET 10.0** | **MAUI 10.0.x**

## Landing the Plane (Session Completion)

**When ending a work session**, you MUST complete ALL steps below. Work is NOT complete until `git push` succeeds.

**MANDATORY WORKFLOW:**

1. **File issues for remaining work** - Create issues for anything that needs follow-up
2. **Run quality gates** (if code changed) - Tests, linters, builds
3. **Update issue status** - Close finished work, update in-progress items
4. **PUSH TO REMOTE** - This is MANDATORY:
   ```bash
   git pull --rebase
   bd sync
   git push
   git status  # MUST show "up to date with origin"
   ```
5. **Clean up** - Clear stashes, prune remote branches
6. **Verify** - All changes committed AND pushed
7. **Hand off** - Provide context for next session

**CRITICAL RULES:**
- Work is NOT complete until `git push` succeeds
- NEVER stop before pushing - that leaves work stranded locally
- NEVER say "ready to push when you are" - YOU must push
- If push fails, resolve and retry until it succeeds
