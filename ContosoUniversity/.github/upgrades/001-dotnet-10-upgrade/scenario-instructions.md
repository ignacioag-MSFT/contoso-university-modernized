# Scenario: .NET Framework 4.8 → .NET 10 Upgrade

## Scenario Overview

**Task ID**: 001-dotnet-10-upgrade  
**Project**: ContosoUniversity  
**Source Framework**: .NET Framework 4.8 (ASP.NET MVC 5)  
**Target Framework**: .NET 10 (ASP.NET Core MVC)  
**Project Type**: ASP.NET Web Application (MVC)

### Scope

- Convert ContosoUniversity.csproj from .NET Framework 4.8 to .NET 10 SDK-style format
- Migrate from ASP.NET MVC 5 to ASP.NET Core MVC
- Update Web.config settings to appsettings.json
- Migrate Entity Framework Core 3.1.x to Entity Framework Core 10.x
- Update NuGet package references for .NET 10 compatibility
- Maintain backward compatibility with existing business logic
- Ensure project compiles warning-free
- Pass all unit tests

### Key Artifacts

This upgrade produces:
- New SDK-style `ContosoUniversity.csproj` 
- `appsettings.json` (replacing Web.config)
- Updated `Startup.cs` or `Program.cs`
- Updated controller and view files for ASP.NET Core
- Migration of services and dependency injection

---

## User Preferences

### Flow Mode
**Automatic** - Execute end-to-end without pausing for confirmation between tasks.

### Technical Preferences
- Target Framework: .NET 10 (net10.0)
- Build Configuration: Always build warning-free
- Test Strategy: Run all unit tests after each major task
- Package Strategy: Use latest compatible versions

### Custom Instructions
- Skip any optional migrations (if applicable)
- Always validate compilation with `dotnet build`
- Always run tests with `dotnet test` after code changes
- Fix all warnings (never suppress)

---

## Decisions

### Architecture Decisions
1. **SDK-style project format**: Use modern SDK-style .csproj (required for .NET 10)
2. **Configuration**: Migrate from Web.config to appsettings.json
3. **Dependency Injection**: Use ASP.NET Core built-in DI instead of manual setup
4. **Entity Framework**: Target EF Core 10.x

### Package Decisions
- Newtonsoft.Json: Keep latest (already 13.0.3, compatible)
- Entity Framework Core: Update to 10.x
- Microsoft.Extensions.*: Use 10.x versions for .NET 10
- ASP.NET Core: Use 10.x versions

---

## Execution Status

| Phase | Status | Notes |
|-------|--------|-------|
| **Initialization** | ✅ Complete | Workflow directory created |
| **Planning** | 🔄 In Progress | Creating task breakdown |
| **Execution** | ⏳ Pending | Tasks will execute sequentially |
| **Validation** | ⏳ Pending | Build and test validation |

---

## Related Resources

- **Project Path**: C:\Users\ignacioag\repos\RVAP\AppModernization\frontier-agentic-modernization-rvas\Student\Resources\dotnet\dotnet-migration-copilot-samples\ContosoUniversity
- **Original Project File**: ContosoUniversity.csproj (Framework 4.8)
- **Original Config**: Web.config
- **Packages File**: packages.config (to be migrated)

