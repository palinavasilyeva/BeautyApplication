using System;
using System.ComponentModel.DataAnnotations;

namespace BeautyApplication.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Role { get; set; } 
        [Required]
        public string PasswordHash { get; set; }
    }

    public class Service
    {
        [Key]
        public int ServiceId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Duration { get; set; } 
        [Required]
        public decimal Price { get; set; }
    }

    public class Master
    {
        [Key]
        public int MasterId { get; set; }
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        public string Specialization { get; set; }
    }

    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        public int MasterId { get; set; }
        public Master Master { get; set; }
        [Required]
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        [Required]
        public DateTime AppointmentTime { get; set; }
        [Required]
        public string Status { get; set; } 
    }
}