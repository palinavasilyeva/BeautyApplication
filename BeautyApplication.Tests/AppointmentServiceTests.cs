using BeautyApplication.Data;
using BeautyApplication.Models;
using BeautyApplication.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;

namespace BeautyApplication.Tests
{
    [TestFixture]
    public class AppointmentServiceTests
    {
        private SalonContext _context;
        private AppointmentService _appointmentService;
        private DateTime _currentTime;

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

            // Inicjalizacja serwisu
            _appointmentService = new AppointmentService(_context);

            // Ustaw bieżący czas na datę testową (31 maja 2025, 14:22 CEST)
            _currentTime = new DateTime(2025, 5, 31, 14, 22, 0);

            // Dodanie danych testowych
            var client = new User { Name = "Test Client", Email = "client@example.com", Role = "Client", PasswordHash = "test" };
            var masterUser = new User { Name = "Test Master", Email = "master@example.com", Role = "Master", PasswordHash = "test" };
            _context.Users.AddRange(client, masterUser);
            _context.SaveChanges();

            var master = new Master { UserId = masterUser.UserId, Specialization = "Hairdresser" };
            _context.Masters.Add(master);

            var service = new Service { Name = "Haircut", Duration = 60, Price = 50.00m };
            _context.Services.Add(service);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.CloseConnection();
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void CreateAppointment_ValidData_ReturnsTrue()
        {
            // Przygotowanie
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1); // Przyszła data

            // Działanie
            var result = _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);

            // Asercja
            Assert.IsTrue(result);
            var appointment = _context.Appointments.FirstOrDefault();
            Assert.IsNotNull(appointment);
            Assert.AreEqual(client.UserId, appointment.UserId);
            Assert.AreEqual(master.MasterId, appointment.MasterId);
            Assert.AreEqual(service.ServiceId, appointment.ServiceId);
            Assert.AreEqual(appointmentTime, appointment.AppointmentTime);
            Assert.AreEqual("Pending", appointment.Status);
        }

        [Test]
        public void CreateAppointment_PastDate_ReturnsFalse()
        {
            // Przygotowanie
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(-1); // Przeszła data

            // Działanie
            var result = _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);

            // Asercja
            Assert.IsFalse(result);
            Assert.IsEmpty(_context.Appointments);
        }

        [Test]
        public void CreateAppointment_ConflictingTime_ReturnsFalse()
        {
            // Przygotowanie
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);

            // Dodaj pierwszą wizytę
            _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);

            // Działanie: Próba utworzenia drugiej wizyty na ten sam czas
            var result = _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);

            // Asercja
            Assert.IsFalse(result);
            Assert.AreEqual(1, _context.Appointments.Count());
        }

        [Test]
        public void CreateAppointment_NonExistentUserId_ThrowsException()
        {
            // Przygotowanie
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);
            int nonExistentUserId = 999; // ID, którego nie ma w bazie

            // Działanie i Asercja
            Assert.Throws<InvalidOperationException>(() =>
                _appointmentService.CreateAppointment(nonExistentUserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime));
        }

        [Test]
        public void GetUserAppointments_ReturnsCorrectAppointments()
        {
            // Przygotowanie
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);

            _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);

            // Działanie
            var appointments = _appointmentService.GetUserAppointments(client.UserId);

            // Asercja
            Assert.AreEqual(1, appointments.Count);
            var appointment = appointments.First();
            Assert.AreEqual(client.UserId, appointment.UserId);
            Assert.IsNotNull(appointment.Service);
            Assert.AreEqual("Haircut", appointment.Service.Name);
            Assert.IsNotNull(appointment.Master);
            Assert.AreEqual("Hairdresser", appointment.Master.Specialization);
        }

        [Test]
        public void CancelAppointment_ValidId_UpdatesStatus()
        {
            // Przygotowanie
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);

            _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);
            var appointment = _context.Appointments.First();

            // Działanie
            _appointmentService.CancelAppointment(appointment.AppointmentId);

            // Asercja
            var updatedAppointment = _context.Appointments.First();
            Assert.AreEqual("Cancelled", updatedAppointment.Status);
        }

        [Test]
        public void CancelAppointment_NonExistentId_DoesNotThrow()
        {
            // Przygotowanie
            int nonExistentAppointmentId = 999; // ID, którego nie ma w bazie

            // Działanie i Asercja: Sprawdź, że metoda nie rzuca wyjątku
            Assert.DoesNotThrow(() => _appointmentService.CancelAppointment(nonExistentAppointmentId));
        }
    }
}