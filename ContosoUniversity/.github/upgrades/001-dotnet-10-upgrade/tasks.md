# Tasks: ContosoUniversity .NET 10 Upgrade

**Scenario**: 001-dotnet-10-upgrade  
**Flow Mode**: Automatic  
**Status**: Ready to Execute

---

## Phase 1: Project Structure Preparation

- ⏳ **01-create-sdk-csproj**: Create new SDK-style .csproj with .NET 10 target
- ⏳ **02-migrate-packages**: Migrate packages.config to PackageReferences
- ⏳ **03-create-appsettings**: Create appsettings.json from Web.config

## Phase 2: Core Application Conversion

- ⏳ **04-create-startup**: Create Program.cs and Startup.cs
- ⏳ **05-migrate-controllers**: Migrate controllers to ASP.NET Core MVC
- ⏳ **06-migrate-models**: Update models and data context for .NET 10
- ⏳ **07-migrate-services**: Update services and business logic

## Phase 3: Views and Frontend

- ⏳ **08-update-views**: Update Razor views for ASP.NET Core
- ⏳ **09-update-view-imports**: Update _ViewStart, view imports, tag helpers

## Phase 4: Configuration and Startup

- ⏳ **10-update-routing**: Migrate RouteConfig to Startup routing
- ⏳ **11-migrate-globalasax**: Migrate Global.asax.cs logic to Startup

## Phase 5: Validation & Fixes

- ⏳ **12-fix-build**: Fix build errors and warnings
- ⏳ **13-run-tests**: Run and fix unit tests
- ⏳ **14-validate-app**: Validate application functionality

---

## Execution Notes

- All tasks execute sequentially in automatic mode
- No user interaction required between tasks
- Build validation after each major phase
- Full compilation check before finalizing

---

## Completion Target

✅ Build passes without errors/warnings  
✅ All unit tests pass  
✅ Application functional and ready for deployment
