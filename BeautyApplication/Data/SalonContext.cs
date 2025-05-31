using Microsoft.EntityFrameworkCore;
using BeautyApplication.Models;

namespace BeautyApplication.Data
{
    public class SalonContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Master> Masters { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        public SalonContext() { }

        public SalonContext(DbContextOptions<SalonContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=salon.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Master>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Master)
                .WithMany()
                .HasForeignKey(a => a.MasterId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId);
        }
    }
}