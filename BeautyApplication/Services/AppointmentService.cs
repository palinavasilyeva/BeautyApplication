using BeautyApplication.Data;
using BeautyApplication.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeautyApplication.Services
{
    public class AppointmentService
    {
        private readonly SalonContext _context;

        public AppointmentService(SalonContext context)
        {
            _context = context;
        }

        public bool CreateAppointment(int userId, int masterId, int serviceId, DateTime appointmentTime)
        {
            if (appointmentTime < DateTime.Now)
                return false;

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

        public List<Appointment> GetUserAppointments(int userId)
        {
            return _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Master)
                .Where(a => a.UserId == userId)
                .ToList();
        }

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