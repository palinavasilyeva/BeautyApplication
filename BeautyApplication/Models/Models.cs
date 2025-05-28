using System;
using System.ComponentModel.DataAnnotations;

namespace BeautyApplication.Models
{
    /// <summary>
    /// Пользователь системы (клиент или мастер).
    /// </summary>
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Role { get; set; } // "Client" или "Master"
        [Required]
        public string PasswordHash { get; set; }
    }

    /// <summary>
    /// Услуга, предоставляемая салоном.
    /// </summary>
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Duration { get; set; } // В минутах
        [Required]
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Мастер салона.
    /// </summary>
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

    /// <summary>
    /// Запись на прием.
    /// </summary>
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
        public string Status { get; set; } // "Pending", "Confirmed", "Cancelled"
    }
}