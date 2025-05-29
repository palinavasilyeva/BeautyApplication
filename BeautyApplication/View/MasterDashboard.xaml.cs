using BeautyApplication.Data;
using BeautyApplication.Models;
using BeautyApplication.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace BeautyApplication.Views
{
    public partial class MasterDashboard : Window
    {
        private readonly SalonContext _context;
        private readonly AppointmentService _appointmentService;
        private readonly User _user;

        public MasterDashboard(User user)
        {
            InitializeComponent();
            _context = new SalonContext();
            _appointmentService = new AppointmentService(_context);
            _user = user;
            LoadData();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
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
