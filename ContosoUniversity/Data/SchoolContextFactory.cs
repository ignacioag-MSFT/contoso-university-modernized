using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Data
{
    /// <summary>
    /// Factory for creating SchoolContext instances.
    /// Supports both local SQL Server (with Integrated Security) and Azure SQL Database (with Managed Identity).
    /// </summary>
    public static class SchoolContextFactory
    {
        /// <summary>
        /// Creates a SchoolContext with the connection string from environment variables.
        /// For Azure SQL Database, use "Authentication=Active Directory Default;" in the connection string.
        /// </summary>
        public static SchoolContext Create()
        {
            // For migrations or other scenarios where DI is not available
            // Use environment variable or fall back to default local connection string
            var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING_DEFAULTCONNECTION")
                ?? "Server=(localdb)\\mssqllocaldb;Database=ContosoUniversity;Trusted_Connection=true;";
            
            var optionsBuilder = new DbContextOptionsBuilder<SchoolContext>();
            
            // Configure SQL Server connection with retry policy
            // This supports both local development and Azure SQL Database connections
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                // Enable connection resiliency for transient failures
                // Important for cloud-based Azure SQL Database connections
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
                
                // Set appropriate command timeout for potentially slower cloud connections
                sqlOptions.CommandTimeout(30);
            });
            
            return new SchoolContext(optionsBuilder.Options);
        }
    }
}
