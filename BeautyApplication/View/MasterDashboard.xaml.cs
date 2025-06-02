using BeautyApplication.Data;
using BeautyApplication.Models;
using BeautyApplication.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace BeautyApplication.Views
{
    /// <summary>
    /// Logika interakcji dla okna pulpitu mistrza.
    /// </summary>
    public partial class MasterDashboard : Window
    {
        private readonly SalonContext _context;
        private readonly AppointmentService _appointmentService;
        private readonly User _user;

        /// <summary>
        /// Inicjalizuje nową instancję klasy <see cref="MasterDashboard"/> dla określonego użytkownika.
        /// </summary>
        /// <param name="user">Użytkownik powiązany z tym pulpitem.</param>
        public MasterDashboard(User user)
        {
            InitializeComponent();
            _context = new SalonContext();
            _appointmentService = new AppointmentService(_context);
            _user = user;
            LoadData();
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
        /// Ładuje dane harmonogramu mistrza na podstawie jego wizyt.
        /// </summary>
        private void LoadData()
        {
            var master = _context.Masters.FirstOrDefault(m => m.UserId == _user.UserId);
            if (master != null)
            {
                AppointmentsListView.ItemsSource = _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Service)
                    .Where(a => a.MasterId == master.MasterId)
                    .ToList();
            }
        }
    }
}
