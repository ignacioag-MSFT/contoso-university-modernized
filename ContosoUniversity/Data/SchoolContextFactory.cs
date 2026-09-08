using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Data
{
    public static class SchoolContextFactory
    {
        public static SchoolContext Create()
        {
            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ContosoUniversity;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
            var optionsBuilder = new DbContextOptionsBuilder<SchoolContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new SchoolContext(optionsBuilder.Options);
        }
    }
}
