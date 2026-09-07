# Modernization Plan: ContosoUniversity .NET 10 Upgrade

**Scenario**: 001-dotnet-10-upgrade  
**Project**: ContosoUniversity  
**Target**: .NET Framework 4.8 → .NET 10  
**Status**: Active

---

## Assessment Summary

### Current State
- **Framework**: .NET Framework 4.8
- **Web Framework**: ASP.NET MVC 5
- **Project Format**: Legacy .csproj (Framework style)
- **Configuration**: Web.config
- **Package Management**: packages.config
- **Entity Framework**: EF Core 3.1.x (mixed with legacy references)
- **Key Controllers**: HomeController, StudentsController, CoursesController, InstructorsController, DepartmentsController, NotificationsController, BaseController

### Target State
- **Framework**: .NET 10 (net10.0)
- **Web Framework**: ASP.NET Core MVC
- **Project Format**: SDK-style .csproj
- **Configuration**: appsettings.json
- **Package Management**: PackageReference in .csproj
- **Entity Framework**: EF Core 10.x
- **Architecture**: Modern ASP.NET Core with dependency injection

### Key Challenges & Considerations

1. **Global.asax removal**: ASP.NET Core uses Startup.cs/Program.cs instead
2. **Web.config migration**: Settings move to appsettings.json
3. **Controller inheritance**: BaseController pattern needs update
4. **View engine changes**: Razor syntax updates
5. **Authentication/Authorization**: Needs migration to Core identity patterns
6. **Services layer**: NotificationService and others need DI updates
7. **Route configuration**: RouteConfig needs Startup.cs migration
8. **Bundle/Optimization**: No longer needed (npm/webpack used instead)

### Risk Assessment
- **High Impact**: Global.asax removal, authentication migration
- **Medium Impact**: Web.config conversion, package updates
- **Low Impact**: CSS/JS files (can remain mostly unchanged)

---

## Execution Plan

### Phase 1: Project Structure Preparation
1. **Task 01**: Create new SDK-style .csproj with .NET 10 target framework
2. **Task 02**: Migrate packages.config to PackageReferences in new .csproj
3. **Task 03**: Create appsettings.json and migrate Web.config settings

### Phase 2: Core Application Conversion
4. **Task 04**: Create Program.cs and Startup.cs for ASP.NET Core
5. **Task 05**: Migrate controllers from MVC 5 to ASP.NET Core MVC
6. **Task 06**: Update models and data layer for .NET 10 compatibility
7. **Task 07**: Migrate services and business logic

### Phase 3: Views and Frontend
8. **Task 08**: Update Razor views for ASP.NET Core (_ViewStart, _Layout, etc.)
9. **Task 09**: Update view imports and tag helpers

### Phase 4: Configuration and Startup
10. **Task 10**: Update RouteConfig to new routing model
11. **Task 11**: Update Global.asax.cs logic to Startup.cs

### Phase 5: Validation & Fixes
12. **Task 12**: Fix build errors and warnings
13. **Task 13**: Run and pass all unit tests
14. **Task 14**: Validate application functionality

---

## Success Criteria

✅ **Build Criteria**
- Project builds successfully: `dotnet build`
- No compiler errors
- No compiler warnings
- `dotnet publish` succeeds

✅ **Testing Criteria**
- All unit tests pass: `dotnet test`
- No test failures or warnings
- Code passes compilation checks

✅ **Functional Criteria**
- Application starts without errors
- Core features work (student CRUD, course management, etc.)
- Backward compatibility maintained
- Data access layer functional

---

## Rollback Strategy

If migration encounters critical issues:
1. Revert to backup of original project
2. Identify specific failing component
3. Address component-level issue
4. Re-apply changes incrementally

Current backup point: Original .NET Framework 4.8 project preserved in version control.

---

## Execution Timeline

- **Phase 1**: ~30 minutes (project structure)
- **Phase 2**: ~60 minutes (core conversion)
- **Phase 3**: ~20 minutes (views)
- **Phase 4**: ~15 minutes (configuration)
- **Phase 5**: ~30 minutes (validation and fixes)

**Total Estimated Time**: ~2.5 hours

---

## Next Steps

1. Create new SDK-style .csproj file
2. Migrate all package references
3. Set up ASP.NET Core entry points
4. Execute tasks sequentially
5. Validate with build and tests
