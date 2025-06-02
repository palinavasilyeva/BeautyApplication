using Microsoft.EntityFrameworkCore;
using BeautyApplication.Models;

namespace BeautyApplication.Data
{
    /// <summary>
    /// Reprezentuje kontekst bazy danych dla aplikacji salonu, zarządzający encjami takimi jak użytkownicy, usługi, mistrzowie i wizyty.
    /// </summary>
    public class SalonContext : DbContext
    {
        /// <summary>
        /// Pobiera lub ustawia kolekcję użytkowników w salonie.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Pobiera lub ustawia kolekcję usług oferowanych przez salon.
        /// </summary>
        public DbSet<Service> Services { get; set; }

        /// <summary>
        /// Pobiera lub ustawia kolekcję mistrzów pracujących w salonie.
        /// </summary>
        public DbSet<Master> Masters { get; set; }

        /// <summary>
        /// Pobiera lub ustawia kolekcję zaplanowanych wizyt w salonie.
        /// </summary>
        public DbSet<Appointment> Appointments { get; set; }

        /// <summary>
        /// Inicjalizuje nową instancję klasy <see cref="SalonContext"/> za pomocą domyślnego konstruktora bez parametrów.
        /// </summary>
        public SalonContext() { }

        /// <summary>
        /// Inicjalizuje nową instancję klasy <see cref="SalonContext"/> z określonymi opcjami.
        /// </summary>
        /// <param name="options">Opcje konfiguracji kontekstu.</param>
        public SalonContext(DbContextOptions<SalonContext> options) : base(options) { }

        /// <summary>
        /// Konfiguruje połączenie z bazą danych, jeśli nie zostało jeszcze skonfigurowane.
        /// </summary>
        /// <param name="optionsBuilder">Budowniczy używany do konfiguracji opcji kontekstu.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=salon.db");
            }
        }

        /// <summary>
        /// Konfiguruje relacje i ograniczenia modeli dla encji.
        /// </summary>
        /// <param name="modelBuilder">Budowniczy używany do tworzenia modelu.</param>
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
