using BeautyApplication.Data;
using BeautyApplication.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;

namespace BeautyApplication.Tests
{
    [TestFixture]
    public class MainWindowTests
    {
        private SalonContext _context;

        [SetUp]
        public void Setup()
        {
            // Skonfiguruj bazę danych SQLite w pamięci dla testów
            var options = new DbContextOptionsBuilder<SalonContext>()
                .UseSqlite("Data Source=:memory:")
                .Options;

            _context = new SalonContext(options);

            // Otwórz połączenie z bazą danych, aby in-memory SQLite działała poprawnie
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            // Wyczyść bazę danych po każdym teście
            _context.Database.CloseConnection();
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void InitializeDatabase(SalonContext context)
        {
            // Sprawdź, czy istnieją użytkownicy z określonymi emailami
            bool clientExists = context.Users.Any(u => u.Email == "client@example.com");
            bool masterExists = context.Users.Any(u => u.Email == "master@example.com");

            // Dodaj domyślnych użytkowników tylko jeśli nie istnieją
            if (!clientExists)
            {
                context.Users.Add(new Models.User { Name = "Test Client", Email = "client@example.com", Role = "Client", PasswordHash = "test" });
            }

            if (!masterExists)
            {
                var masterUser = new Models.User { Name = "Test Master", Email = "master@example.com", Role = "Master", PasswordHash = "test" };
                context.Users.Add(masterUser);
                context.SaveChanges(); // Zapisz, aby masterUser otrzymał UserId

                // Dodaj powiązanego Mastera
                context.Masters.Add(new Models.Master { UserId = masterUser.UserId, Specialization = "Hairdresser" });
            }
            else
            {
                context.SaveChanges(); // Zapisz zmiany, jeśli dodano tylko klienta
            }

            // Dodaj usługi, jeśli jeszcze nie istnieją
            if (!context.Services.Any(s => s.Name == "Haircut"))
            {
                context.Services.Add(new Models.Service { Name = "Haircut", Duration = 60, Price = 50.00m });
            }
            if (!context.Services.Any(s => s.Name == "Manicure"))
            {
                context.Services.Add(new Models.Service { Name = "Manicure", Duration = 45, Price = 30.00m });
            }

            context.SaveChanges();
        }

        [Test]
        public void InitializeDatabase_NoUsers_CreatesDefaultData()
        {
            // Działanie: Wywołaj metodę do zainicjowania bazy danych
            InitializeDatabase(_context);

            // Asercja: Zweryfikuj, że domyślne dane zostały utworzone
            Assert.AreEqual(2, _context.Users.Count());
            Assert.IsTrue(_context.Users.Any(u => u.Email == "client@example.com" && u.Role == "Client"));
            Assert.IsTrue(_context.Users.Any(u => u.Email == "master@example.com" && u.Role == "Master"));

            Assert.AreEqual(1, _context.Masters.Count());
            var master = _context.Masters.First();
            Assert.AreEqual("Hairdresser", master.Specialization);
            Assert.AreEqual("Test Master", _context.Users.First(u => u.UserId == master.UserId).Name); // Sprawdź powiązanie z użytkownikiem

            Assert.AreEqual(2, _context.Services.Count());
            Assert.IsTrue(_context.Services.Any(s => s.Name == "Haircut" && s.Price == 50.00m && s.Duration == 60));
            Assert.IsTrue(_context.Services.Any(s => s.Name == "Manicure" && s.Price == 30.00m && s.Duration == 45));
        }

        [Test]
        public void InitializeDatabase_ExistingUsers_DoesNotDuplicate()
        {
            // Przygotowanie: Dodaj istniejącego użytkownika testowego
            _context.Users.Add(new User { Name = "Existing User", Email = "existing@example.com", Role = "Client", PasswordHash = "test" });
            _context.SaveChanges();

            // Działanie: Wywołaj metodę do zainicjowania bazy danych
            InitializeDatabase(_context);

            // Asercja: Zweryfikuj, że dodano tylko domyślne dane testowe, bez duplikowania istniejących użytkowników
            Assert.AreEqual(3, _context.Users.Count()); // 1 istniejący + 2 nowych użytkowników
            Assert.IsTrue(_context.Users.Any(u => u.Email == "client@example.com"));
            Assert.IsTrue(_context.Users.Any(u => u.Email == "master@example.com"));
            Assert.IsTrue(_context.Users.Any(u => u.Email == "existing@example.com"));
        }
    }
}