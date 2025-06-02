using BeautyApplication.Data;
using BeautyApplication.Models;
using BeautyApplication.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace BeautyApplication.Views
{
    /// <summary>
    /// Logika interakcji dla okna pulpitu klienta.
    /// </summary>
    public partial class ClientDashboard : Window
    {
        private readonly SalonContext _context;
        private readonly AppointmentService _appointmentService;
        private readonly User _user;

        /// <summary>
        /// Inicjalizuje nową instancję klasy <see cref="ClientDashboard"/> dla określonego użytkownika.
        /// </summary>
        /// <param name="user">Użytkownik powiązany z tym pulpitem.</param>
        public ClientDashboard(User user)
        {
            InitializeComponent();
            _context = new SalonContext();
            _appointmentService = new AppointmentService(_context);
            _user = user;
            LoadData();
        }

        /// <summary>
        /// Ładuje początkowe dane dla pulpitu, w tym usługi, mistrzów i wizyty.
        /// </summary>
        private void LoadData()
        {
            ServiceComboBox.ItemsSource = _context.Services.ToList();
            MasterComboBox.ItemsSource = _context.Masters.Include(m => m.User).ToList();
            AppointmentsListView.ItemsSource = _appointmentService.GetUserAppointments(_user.UserId);
        }

        /// <summary>
        /// Zamyka okno pulpitu po kliknięciu przycisku zamknięcia.
        /// </summary>
        /// <param name="sender">Obiekt, który wywołał zdarzenie.</param>
        /// <param name="e">Argumenty zdarzenia.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Rezerwuje nową wizytę na podstawie wybranej usługi, mistrza i daty.
        /// </summary>
        /// <param name="sender">Obiekt, który wywołał zdarzenie.</param>
        /// <param name="e">Argumenty zdarzenia.</param>
        private void BookAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceComboBox.SelectedItem is Service service &&
                MasterComboBox.SelectedItem is Master master &&
                AppointmentDatePicker.SelectedDate.HasValue)
            {
                var appointmentTime = AppointmentDatePicker.SelectedDate.Value;
                var success = _appointmentService.CreateAppointment(_user.UserId, master.MasterId, service.ServiceId, appointmentTime);

                if (success)
                {
                    AppointmentsListView.ItemsSource = _appointmentService.GetUserAppointments(_user.UserId);
                    MessageBox.Show("Wizyta została pomyślnie zarezerwowana!");
                }
                else
                {
                    MessageBox.Show("Błąd: Wybrany termin jest niedostępny lub nieprawidłowy.");
                }
            }
        }
    }
}
