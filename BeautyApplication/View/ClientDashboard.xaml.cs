using BeautyApplication.Data;
using BeautyApplication.Models;
using BeautyApplication.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace BeautyApplication.Views
{
    public partial class ClientDashboard : Window
    {
        private readonly SalonContext _context;
        private readonly AppointmentService _appointmentService;
        private readonly User _user;

        public ClientDashboard(User user)
        {
            InitializeComponent();
            _context = new SalonContext();
            _appointmentService = new AppointmentService(_context);
            _user = user;
            LoadData();
        }

        private void LoadData()
        {
            ServiceComboBox.ItemsSource = _context.Services.ToList();
            MasterComboBox.ItemsSource = _context.Masters.Include(m => m.User).ToList();
            AppointmentsListView.ItemsSource = _appointmentService.GetUserAppointments(_user.UserId);
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


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
                    MessageBox.Show("Appointment booked successfully!");
                }
                else
                {
                    MessageBox.Show("Error: Time slot is unavailable or invalid.");
                }
            }
        }
    }
}
