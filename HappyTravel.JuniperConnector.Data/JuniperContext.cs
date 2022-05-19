using Microsoft.EntityFrameworkCore;

namespace HappyTravel.JuniperConnector.Data;

public class JuniperContext : DbContext
{
    public JuniperContext()
    { }

    public JuniperContext(DbContextOptions<JuniperContext> options) : base(options)
    { }
}
