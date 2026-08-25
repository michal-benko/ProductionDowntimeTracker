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

        public DbSet<DowntimeCategory> DowntimeCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DowntimeRecord>()
                .HasOne(d => d.Machine)
                .WithMany()
                .HasForeignKey(d => d.MachineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DowntimeRecord>()
                .HasOne(d => d.Category)
                .WithMany()
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DowntimeCategory>().HasData(
                new DowntimeCategory { Id = 1, Name = "Porucha senzoru" },
                new DowntimeCategory { Id = 2, Name = "Problém s PLC logikou" },
                new DowntimeCategory { Id = 3, Name = "Zaseknutý robot" },
                new DowntimeCategory { Id = 4, Name = "Ostatní" }
                );

            modelBuilder.Entity<DowntimeCategory>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<DowntimeRecord>()
                .Property(x => x.Detail)
                .HasMaxLength(500);
        }

    }
}
