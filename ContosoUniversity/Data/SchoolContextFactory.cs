using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Data
{
    public static class SchoolContextFactory
    {
        public static SchoolContext Create()
        {
            // For migrations or other scenarios where DI is not available
            // Use a default connection string or environment variable
            var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING_DEFAULTCONNECTION")
                ?? "Server=(localdb)\\mssqllocaldb;Database=ContosoUniversity;Trusted_Connection=true;";
            
            var optionsBuilder = new DbContextOptionsBuilder<SchoolContext>();
            optionsBuilder.UseSqlServer(connectionString);
            
            return new SchoolContext(optionsBuilder.Options);
        }
    }
}
