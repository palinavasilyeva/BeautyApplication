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
            var options = new DbContextOptionsBuilder<SalonContext>()
                .UseSqlite("Data Source=:memory:")
                .Options;

            _context = new SalonContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            _appointmentService = new AppointmentService(_context);
            _currentTime = new DateTime(2025, 5, 31, 14, 22, 0);

            var client = new User { Name = "Test Client", Email = "client@example.com", Role = "Client", PasswordHash = "test" };
            var masterUser1 = new User { Name = "Test Master 1", Email = "master1@example.com", Role = "Master", PasswordHash = "test" };
            var masterUser2 = new User { Name = "Test Master 2", Email = "master2@example.com", Role = "Master", PasswordHash = "test" };
            _context.Users.AddRange(client, masterUser1, masterUser2);
            _context.SaveChanges();

            var master1 = new Master { UserId = masterUser1.UserId, Specialization = "Hairdresser" };
            var master2 = new Master { UserId = masterUser2.UserId, Specialization = "Manicurist" };
            _context.Masters.AddRange(master1, master2);

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
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);

            var result = _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);

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
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(-1);

            var result = _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);

            Assert.IsFalse(result);
            Assert.IsEmpty(_context.Appointments);
        }

        [Test]
        public void CreateAppointment_ConflictingTime_ReturnsFalse()
        {
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);

            _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);
            var result = _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);

            Assert.IsFalse(result);
            Assert.AreEqual(1, _context.Appointments.Count());
        }

        [Test]
        public void CreateAppointment_DifferentMastersSameTime_ReturnsTrue()
        {
            var client = _context.Users.First(u => u.Role == "Client");
            var master1 = _context.Masters.First(m => m.Specialization == "Hairdresser");
            var master2 = _context.Masters.First(m => m.Specialization == "Manicurist");
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);

            var result1 = _appointmentService.CreateAppointment(client.UserId, master1.MasterId, service.ServiceId, appointmentTime, _currentTime);
            var result2 = _appointmentService.CreateAppointment(client.UserId, master2.MasterId, service.ServiceId, appointmentTime, _currentTime);

            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
            Assert.AreEqual(2, _context.Appointments.Count());
        }

        [Test]
        public void CreateAppointment_NonExistentUserId_ThrowsException()
        {
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);
            int nonExistentUserId = 999;

            Assert.Throws<InvalidOperationException>(() =>
                _appointmentService.CreateAppointment(nonExistentUserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime));
        }

        [Test]
        public void GetUserAppointments_ReturnsCorrectAppointments()
        {
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);

            _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);

            var appointments = _appointmentService.GetUserAppointments(client.UserId);

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
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);

            _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);
            var appointment = _context.Appointments.First();

            _appointmentService.CancelAppointment(appointment.AppointmentId);

            var updatedAppointment = _context.Appointments.First();
            Assert.AreEqual("Cancelled", updatedAppointment.Status);
        }

        [Test]
        public void CancelAppointment_NonExistentId_DoesNotThrow()
        {
            int nonExistentAppointmentId = 999;
            Assert.DoesNotThrow(() => _appointmentService.CancelAppointment(nonExistentAppointmentId));
        }

        [Test]
        public void UpdateAppointment_ValidId_UpdatesTimeAndStatus()
        {
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);

            _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);
            var appointment = _context.Appointments.First();

            var newTime = _currentTime.AddDays(2);
            var result = _appointmentService.UpdateAppointment(appointment.AppointmentId, newTime, "Completed", _currentTime);

            Assert.IsTrue(result);
            var updatedAppointment = _context.Appointments.First();
            Assert.AreEqual(newTime, updatedAppointment.AppointmentTime);
            Assert.AreEqual("Completed", updatedAppointment.Status);
        }

        [Test]
        public void UpdateAppointment_PastTime_ReturnsFalse()
        {
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);

            _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);
            var appointment = _context.Appointments.First();

            var pastTime = _currentTime.AddDays(-1);
            var result = _appointmentService.UpdateAppointment(appointment.AppointmentId, pastTime, "Completed", _currentTime);

            Assert.IsFalse(result);
            var unchangedAppointment = _context.Appointments.First();
            Assert.AreEqual(appointmentTime, unchangedAppointment.AppointmentTime);
        }

        [Test]
        public void UpdateAppointment_ConflictingTime_ReturnsFalse()
        {
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime1 = _currentTime.AddDays(1);
            var appointmentTime2 = _currentTime.AddDays(2);

            _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime1, _currentTime);
            _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime2, _currentTime);

            var appointment = _context.Appointments.First(a => a.AppointmentTime == appointmentTime2);
            var result = _appointmentService.UpdateAppointment(appointment.AppointmentId, appointmentTime1, "Pending", _currentTime);

            Assert.IsFalse(result);
            var unchangedAppointment = _context.Appointments.First(a => a.AppointmentId == appointment.AppointmentId);
            Assert.AreEqual(appointmentTime2, unchangedAppointment.AppointmentTime);
        }

        [Test]
        public void DeleteAppointment_ValidId_RemovesAppointment()
        {
            var client = _context.Users.First(u => u.Role == "Client");
            var master = _context.Masters.First();
            var service = _context.Services.First();
            var appointmentTime = _currentTime.AddDays(1);

            _appointmentService.CreateAppointment(client.UserId, master.MasterId, service.ServiceId, appointmentTime, _currentTime);
            var appointment = _context.Appointments.First();

            var result = _appointmentService.DeleteAppointment(appointment.AppointmentId);

            Assert.IsTrue(result);
            Assert.IsEmpty(_context.Appointments);
        }

        [Test]
        public void DeleteAppointment_NonExistentId_ReturnsFalse()
        {
            int nonExistentAppointmentId = 999;
            var result = _appointmentService.DeleteAppointment(nonExistentAppointmentId);
            Assert.IsFalse(result);
        }
    }
}