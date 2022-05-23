using HappyTravel.JuniperConnector.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.JuniperConnector.Data;

public class JuniperContext : DbContext
{
    public JuniperContext()
    { }

    public JuniperContext(DbContextOptions<JuniperContext> options) : base(options)
    { }


    public DbSet<Zone> Zones { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<StaticDataUpdateHistoryEntry> StaticDataUpdateHistory { get; set; }
    public DbSet<Accommodation> Accommodations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Zone>(r =>
        {
            r.HasKey(a => a.Code);
            r.Property(p => p.IATA).IsRequired(false);            
            r.Property(p => p.ParentCode).IsRequired(false);
        });

        builder.Entity<Hotel>(c => 
        {
            c.HasKey(c => c.Code);
            c.Property(r => r.Data).HasColumnType("jsonb");
            c.Property(p => p.IsActive).HasDefaultValue(true);
        });

        builder.Entity<Accommodation>(r =>
        {
            r.HasKey(a => a.Code);
        });
    }
}
