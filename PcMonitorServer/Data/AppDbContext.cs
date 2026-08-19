using Microsoft.EntityFrameworkCore;
using PcMonitorServer.Models;

namespace PcMonitorServer.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<Session> Sessions => Set<Session>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("pc_monitor");

        modelBuilder.Entity<Machine>(e =>
        {
            e.HasIndex(m => m.MachineId).IsUnique();
        });

        modelBuilder.Entity<Session>(e =>
        {
            e.HasIndex(s => new { s.MachineId, s.EndedAt });
        });
    }
}
