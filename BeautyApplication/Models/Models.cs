using System;
using System.ComponentModel.DataAnnotations;

namespace BeautyApplication.Models
{
    /// <summary>
    /// Reprezentuje użytkownika aplikacji salonu, z rolami takimi jak Klient lub Mistrz.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Pobiera lub ustawia unikalny identyfikator użytkownika.
        /// </summary>
        [Key]
        public int UserId { get; set; }

        /// <summary>
        /// Pobiera lub ustawia imię użytkownika.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Pobiera lub ustawia adres e-mail użytkownika.
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// Pobiera lub ustawia rolę użytkownika (np. Klient lub Mistrz).
        /// </summary>
        [Required]
        public string Role { get; set; }

        /// <summary>
        /// Pobiera lub ustawia haszowane hasło użytkownika.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }
    }

    /// <summary>
    /// Reprezentuje usługę oferowaną przez salon.
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Pobiera lub ustawia unikalny identyfikator usługi.
        /// </summary>
        [Key]
        public int ServiceId { get; set; }

        /// <summary>
        /// Pobiera lub ustawia nazwę usługi.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Pobiera lub ustawia czas trwania usługi w minutach.
        /// </summary>
        [Required]
        public int Duration { get; set; }

        /// <summary>
        /// Pobiera lub ustawia cenę usługi.
        /// </summary>
        [Required]
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Reprezentuje mistrza (specjalistę) pracującego w salonie, powiązanego z użytkownikiem.
    /// </summary>
    public class Master
    {
        /// <summary>
        /// Pobiera lub ustawia unikalny identyfikator mistrza.
        /// </summary>
        [Key]
        public int MasterId { get; set; }

        /// <summary>
        /// Pobiera lub ustawia identyfikator użytkownika powiązanego z tym mistrzem.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Pobiera lub ustawia użytkownika powiązanego z tym mistrzem.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Pobiera lub ustawia specjalizację mistrza.
        /// </summary>
        [Required]
        public string Specialization { get; set; }
    }

    /// <summary>
    /// Reprezentuje umówioną wizytę w salonie.
    /// </summary>
    public class Appointment
    {
        /// <summary>
        /// Pobiera lub ustawia unikalny identyfikator wizyty.
        /// </summary>
        [Key]
        public int AppointmentId { get; set; }

        /// <summary>
        /// Pobiera lub ustawia identyfikator użytkownika powiązanego z wizytą.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Pobiera lub ustawia użytkownika powiązanego z wizytą.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Pobiera lub ustawia identyfikator mistrza powiązanego z wizytą.
        /// </summary>
        [Required]
        public int MasterId { get; set; }

        /// <summary>
        /// Pobiera lub ustawia mistrza powiązanego z wizytą.
        /// </summary>
        public Master Master { get; set; }

        /// <summary>
        /// Pobiera lub ustawia identyfikator usługi powiązanej z wizytą.
        /// </summary>
        [Required]
        public int ServiceId { get; set; }

        /// <summary>
        /// Pobiera lub ustawia usługę powiązaną z wizytą.
        /// </summary>
        public Service Service { get; set; }

        /// <summary>
        /// Pobiera lub ustawia zaplanowaną godzinę wizyty.
        /// </summary>
        [Required]
        public DateTime AppointmentTime { get; set; }

        /// <summary>
        /// Pobiera lub ustawia status wizyty (np. Oczekująca, Anulowana).
        /// </summary>
        [Required]
        public string Status { get; set; }
    }
}
