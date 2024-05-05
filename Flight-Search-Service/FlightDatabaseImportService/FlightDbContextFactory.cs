using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlightDatabaseImportService
{
    public class FlightDbContextFactory : IDesignTimeDbContextFactory<FlightDbContext>
    {
        public FlightDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FlightDbContext>();
            optionsBuilder.UseNpgsql("User ID=postgres;Host=localhost;Port=5432;Database=flights;Pooling=true");

            return new FlightDbContext(optionsBuilder.Options);
        }
    }
}
