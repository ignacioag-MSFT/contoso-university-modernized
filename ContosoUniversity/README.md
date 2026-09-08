# Contoso University - .NET 10

This project is an ASP.NET Core MVC application targeting .NET 10.

## Project Overview

### Framework
- ASP.NET Core MVC (.NET 10)

### Database Access: Entity Framework
- Entity Framework Core 10.0.0

### Project Structure
```
ContosoUniversity/
├── Controllers/            # MVC Controllers
├── Data/                   # Entity Framework context and initializer
├── Models/                 # Data models and view models
├── Services/               # Azure Service Bus and Blob Storage integrations
├── Views/                  # Razor views
├── Content/                # CSS and other content
├── Scripts/                # JavaScript files
├── Properties/             # Assembly properties
├── Program.cs              # ASP.NET Core application startup
├── appsettings.json        # Application configuration
└── ContosoUniversity.csproj # SDK-style project with PackageReference
```

## Database Configuration

The application uses SQL Server LocalDB with the following connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=ContosoUniversityNoAuthEFCore;Integrated Security=True;MultipleActiveResultSets=True"
  }
}
```

## Running the Application

1. **Prerequisites**:
   - Visual Studio 2019 or later
   - IIS Express
   - SQL Server LocalDB
   - Access to Azure Service Bus with managed identity permissions for the notification queue

2. **Setup**:
   - Open the project in Visual Studio
   - Restore NuGet packages
   - Build the solution
   - Run the ASP.NET Core application

## Features

- **Student Management**: CRUD operations for students with pagination and search
- **Course Management**: Manage courses and their assignments to departments
- **Instructor Management**: Handle instructor assignments and office locations
- **Department Management**: Manage departments and their administrators
- **Statistics**: View enrollment statistics by date

## Database Initialization

The application uses Entity Framework Core Code First with a database initializer that:
- Creates the database if it doesn't exist
- Seeds sample data including students, instructors, courses, and departments
- Handles model changes by recreating the database
