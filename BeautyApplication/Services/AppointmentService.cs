using BeautyApplication.Data;
using BeautyApplication.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeautyApplication.Services
{
    /// <summary>
    /// Zapewnia usługi do zarządzania wizytami w salonie.
    /// </summary>
    public class AppointmentService
    {
        private readonly SalonContext _context;

        /// <summary>
        /// Inicjalizuje nową instancję klasy <see cref="AppointmentService"/> z określonym kontekstem.
        /// </summary>
        /// <param name="context">Kontekst bazy danych.</param>
        public AppointmentService(SalonContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tworzy nową wizytę dla użytkownika, używając aktualnego czasu.
        /// </summary>
        /// <param name="userId">ID użytkownika rezerwującego wizytę.</param>
        /// <param name="masterId">ID mistrza.</param>
        /// <param name="serviceId">ID usługi.</param>
        /// <param name="appointmentTime">Czas wizyty.</param>
        /// <returns>True jeśli wizyta została pomyślnie utworzona, w przeciwnym razie false.</returns>
        public bool CreateAppointment(int userId, int masterId, int serviceId, DateTime appointmentTime)
        {
            return CreateAppointment(userId, masterId, serviceId, appointmentTime, DateTime.Now);
        }

        /// <summary>
        /// Tworzy nową wizytę dla użytkownika, z określonym czasem bieżącym (np. do testów).
        /// </summary>
        /// <param name="userId">ID użytkownika rezerwującego wizytę.</param>
        /// <param name="masterId">ID mistrza.</param>
        /// <param name="serviceId">ID usługi.</param>
        /// <param name="appointmentTime">Czas wizyty.</param>
        /// <param name="currentTime">Bieżący czas do walidacji.</param>
        /// <returns>True jeśli wizyta została pomyślnie utworzona, w przeciwnym razie false.</returns>
        public bool CreateAppointment(int userId, int masterId, int serviceId, DateTime appointmentTime, DateTime currentTime)
        {
            if (appointmentTime < currentTime)
                return false;

            if (!_context.Users.Any(u => u.UserId == userId))
                throw new InvalidOperationException("Użytkownik nie istnieje.");
            if (!_context.Masters.Any(m => m.MasterId == masterId))
                throw new InvalidOperationException("Mistrz nie istnieje.");
            if (!_context.Services.Any(s => s.ServiceId == serviceId))
                throw new InvalidOperationException("Usługa nie istnieje.");

            var conflict = _context.Appointments
                .Any(a => a.MasterId == masterId && a.AppointmentTime == appointmentTime && a.Status != "Cancelled");

            if (conflict)
                return false;

            var appointment = new Appointment
            {
                UserId = userId,
                MasterId = masterId,
                ServiceId = serviceId,
                AppointmentTime = appointmentTime,
                Status = "Pending"
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Aktualizuje czas i status istniejącej wizyty.
        /// </summary>
        /// <param name="appointmentId">ID wizyty do zaktualizowania.</param>
        /// <param name="newAppointmentTime">Nowy czas wizyty.</param>
        /// <param name="newStatus">Nowy status wizyty.</param>
        /// <param name="currentTime">Bieżący czas do walidacji.</param>
        /// <returns>True jeśli wizyta została pomyślnie zaktualizowana, w przeciwnym razie false.</returns>
        public bool UpdateAppointment(int appointmentId, DateTime newAppointmentTime, string newStatus, DateTime currentTime)
        {
            var appointment = _context.Appointments.Find(appointmentId);
            if (appointment == null || newAppointmentTime < currentTime)
                return false;

            var conflict = _context.Appointments
                .Any(a => a.AppointmentId != appointmentId &&
                          a.MasterId == appointment.MasterId &&
                          a.AppointmentTime == newAppointmentTime &&
                          a.Status != "Cancelled");

            if (conflict)
                return false;

            appointment.AppointmentTime = newAppointmentTime;
            appointment.Status = newStatus;
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Usuwa istniejącą wizytę z bazy danych.
        /// </summary>
        /// <param name="appointmentId">ID wizyty do usunięcia.</param>
        /// <returns>True jeśli wizyta została pomyślnie usunięta, false jeśli nie została znaleziona.</returns>
        public bool DeleteAppointment(int appointmentId)
        {
            var appointment = _context.Appointments.Find(appointmentId);
            if (appointment == null)
                return false;

            _context.Appointments.Remove(appointment);
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Pobiera wszystkie wizyty dla określonego użytkownika.
        /// </summary>
        /// <param name="userId">ID użytkownika, którego wizyty mają zostać pobrane.</param>
        /// <returns>Lista wizyt użytkownika.</returns>
        public List<Appointment> GetUserAppointments(int userId)
        {
            return _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Master)
                .Where(a => a.UserId == userId)
                .ToList();
        }

        /// <summary>
        /// Anuluje wizytę, ustawiając jej status na "Cancelled".
        /// </summary>
        /// <param name="appointmentId">ID wizyty do anulowania.</param>
        public void CancelAppointment(int appointmentId)
        {
            var appointment = _context.Appointments.Find(appointmentId);
            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                _context.SaveChanges();
            }
        }
    }
}
