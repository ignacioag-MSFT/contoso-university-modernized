using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Services;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add DbContext with support for Azure SQL Database with Managed Identity
// MIGRATION NOTE: Following Azure SQL Database best practices,
// we configure connection strings that work with both local development (using Integrated Security)
// and Azure SQL Database (using Managed Identity via Azure.Identity).
builder.Services.AddDbContext<SchoolContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Configure SQL Server connection options
    // For Azure SQL Database, the connection string should use "Authentication=Active Directory Default;"
    // For local development, the connection string can use "Integrated Security=True;"
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Enable connection resiliency for transient failures (especially important for Azure SQL)
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
        
        // Configure for Entity Framework Core best practices
        sqlOptions.CommandTimeout(30);
    });
});

// Add application services
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddScoped<NotificationService>();

var serviceBusNamespace = builder.Configuration["AzureServiceBus:FullyQualifiedNamespace"];
if (!string.IsNullOrWhiteSpace(serviceBusNamespace) && !serviceBusNamespace.Contains("${", StringComparison.Ordinal))
{
    builder.Services.AddSingleton(
        new ServiceBusClient(serviceBusNamespace, new DefaultAzureCredential()));
}

// Add Azure Blob Storage service
// MIGRATION NOTE: Following Rule 26 (Azure SDK Client Lifetime),
// register BlobServiceClient as Singleton using Microsoft.Extensions.Azure.
// This reuses the HTTP pipeline, connection pool, and credential token cache.
builder.Services.AddAzureClients(clientBuilder =>
{
    var storageUri = builder.Configuration["AzureStorage:ServiceUri"];
    if (string.IsNullOrEmpty(storageUri))
    {
        throw new InvalidOperationException(
            "AzureStorage:ServiceUri must be configured in appsettings.json. " +
            "Format: https://{STORAGE_ACCOUNT_NAME}.blob.core.windows.net");
    }

    clientBuilder.AddBlobServiceClient(new Uri(storageUri));
    clientBuilder.UseCredential(new DefaultAzureCredential());
});

// Register AzureBlobStorageService as Singleton since it holds a BlobServiceClient
builder.Services.AddSingleton<AzureBlobStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Map controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    dbInitializer.Initialize(scope.ServiceProvider.GetRequiredService<SchoolContext>());
}

app.Run();
