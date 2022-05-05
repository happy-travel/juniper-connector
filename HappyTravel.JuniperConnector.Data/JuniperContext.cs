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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Zone>(r =>
        {
            r.HasKey(a => a.Code);
        });
    }
}
