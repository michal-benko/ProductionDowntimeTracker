using Microsoft.EntityFrameworkCore;
using ProductionDowntimeTracker.api.Models;


namespace ProductionDowntimeTracker.api.Data
{
    public class MachineDbContext : DbContext
    {
        public MachineDbContext(DbContextOptions<MachineDbContext> options) : base(options)
        {

        }

        public DbSet<Machine> Machines { get; set; }

    }
}
