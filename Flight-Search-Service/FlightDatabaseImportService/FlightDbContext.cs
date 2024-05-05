using FlightSearchService.Models.Internal;
using Microsoft.EntityFrameworkCore;

namespace FlightDatabaseImportService;

public class FlightDbContext : DbContext
{
    public FlightDbContext() { }

    public FlightDbContext(DbContextOptions opts)
    : base(opts)
    {
    }

    public DbSet<AircraftStaticInfo> AircraftStaticInfos => Set<AircraftStaticInfo>();

    public DbSet<AircraftDynamicInfo> AircraftDynamicInfos => Set<AircraftDynamicInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<AircraftStaticInfo>()
            .HasKey(e => e.CallSign);

        modelBuilder
            .Entity<AircraftDynamicInfo>()
            .HasKey(e => e.CallSign);
    }
}
