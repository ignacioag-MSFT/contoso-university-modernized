# Modernization Plan: ContosoUniversity Azure Migration

**Project**: ContosoUniversity

---

## Technical Framework

- **Language**: .NET Framework 4.8 (target: .NET 10)
- **Framework**: ASP.NET MVC 5 (target: ASP.NET Core MVC)
- **Build Tool**: NuGet/MSBuild
- **Database**: SQL Server (target: Azure SQL Database)
- **Key Dependencies**: Entity Framework 6, System.Messaging (MSMQ)

---

## Overview

This migration modernizes ContosoUniversity from .NET Framework 4.8 ASP.NET MVC 5 to .NET 10 ASP.NET Core running on Azure. The application currently uses local file system storage for teaching materials, System.Messaging (MSMQ) for notification queuing, and on-premises SQL Server for the database. The new architecture will:

- **Runtime Modernization**: Upgrade to .NET 10 with ASP.NET Core MVC for improved performance, security, and cloud-native features
- **Asynchronous Messaging**: Replace MSMQ with Azure Service Bus for reliable, cloud-native message queueing and notifications
- **Cloud Storage**: Replace local file system uploads with Azure Blob Storage for scalable, secure document management
- **Database Migration**: Migrate from on-premises SQL Server to Azure SQL Database with Entity Framework Core, enabling passwordless authentication and managed backups

The migration follows a phased approach: upgrade the runtime, transform to Azure services, verify security posture, and deploy to Azure Container Apps.

---

## Migration Impact Summary

| Application | Original Service | New Azure Service | Authentication | Comments |
|-------------|------------------|-------------------|---|---|
| ContosoUniversity | System.Messaging (MSMQ) | Azure Service Bus | Managed Identity | Async notifications for teaching material uploads and system events |
| ContosoUniversity | File System (Uploads/TeachingMaterials) | Azure Blob Storage | Managed Identity | Teaching materials and course documents storage |
| ContosoUniversity | SQL Server (On-Premises) | Azure SQL Database | Managed Identity | Student, course, and enrollment data |
| ContosoUniversity | ASP.NET MVC 5/.NET Framework 4.8 | ASP.NET Core MVC/.NET 10 | - | Modern, cloud-native application framework |

---

## Open Questions & Questionnaire

- [x] Q: Should infrastructure be provisioned? → A: Not specified, skipping infrastructure provisioning; focus on code migration
- [x] Q: Should integration testing be included? → A: Not specified, skipping integration testing
- [x] Q: Should security/CVE remediation be included? → A: Yes, included by default
- [x] Q: What is the deployment target? → A: Not specified, defaulting to Azure Container Apps
- [x] Q: Should containerization be included? → A: Yes, Azure Container Apps requires containerization

---

## Tasks Breakdown

The modernization plan includes the following task categories:

1. **Upgrade** (001-upgrade-dotnet-10): Upgrade .NET Framework 4.8 to .NET 10 and convert from web.config to .NET Core configuration
2. **Transform - Azure Service Bus** (002-transform-msmq-to-servicebus): Replace System.Messaging with Azure Service Bus for message queueing
3. **Transform - Azure Blob Storage** (003-transform-filestorage-to-blob): Replace file system storage with Azure Blob Storage for teaching materials
4. **Transform - Azure SQL Database** (004-transform-efcore-to-azure-sql): Migrate Entity Framework 6 to EF Core and Azure SQL Database
5. **Security** (005-security-cve-remediation): Scan dependencies and remediate CVEs
6. **Deployment** (006-deployment-container-apps): Containerize and deploy to Azure Container Apps

---

## Execution Strategy

- **Sequential Execution**: Upgrade task must run first; transform tasks depend on completed upgrade
- **Parallel Independence**: Each transform task (Service Bus, Blob Storage, EF Core/SQL) targets different concerns and can proceed independently after upgrade
- **Security First**: CVE scanning runs after all transform tasks to validate the migrated application
- **Cloud Deployment**: Containerization and deployment to Azure Container Apps complete the modernization

---

## Success Criteria

- ✅ Application compiles successfully targeting .NET 10
- ✅ All unit tests pass with migrated codebase
- ✅ MSMQ usage replaced with Azure Service Bus integration
- ✅ File system uploads replaced with Azure Blob Storage access
- ✅ Entity Framework 6 replaced with EF Core; SQL Server migrated to Azure SQL Database
- ✅ No critical or high-severity CVEs in project dependencies
- ✅ Docker image builds and runs successfully on Azure Container Apps
