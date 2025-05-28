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
    }
}