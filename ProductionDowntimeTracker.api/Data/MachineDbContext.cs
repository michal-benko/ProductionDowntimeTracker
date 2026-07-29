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

        public DbSet<DowntimeRecord> DowntimeRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DowntimeRecord>()
                .HasOne(d => d.Machine)
                .WithMany()
                .HasForeignKey(d => d.MachineId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
