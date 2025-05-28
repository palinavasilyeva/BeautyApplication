using BeautyApplication.Data;
using BeautyApplication.Views;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using System.Diagnostics;

namespace BeautyApplication
{
    public partial class MainWindow : Window
    {
        private readonly SalonContext _context;

        public MainWindow()
        {
            InitializeComponent();
            var options = new DbContextOptionsBuilder<SalonContext>()
                .UseSqlite("Data Source=salon.db")
                .Options;
            _context = new SalonContext(options);
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            _context.Database.EnsureCreated();
            if (!_context.Users.Any())
            {
                _context.Users.Add(new Models.User { Name = "Test Client", Email = "client@example.com", Role = "Client", PasswordHash = "test" });
                var masterUser = new Models.User { Name = "Test Master", Email = "master@example.com", Role = "Master", PasswordHash = "test" };
                _context.Users.Add(masterUser);
                _context.SaveChanges();

                _context.Masters.Add(new Models.Master { UserId = masterUser.UserId, Specialization = "Hairdresser" });
                _context.Services.Add(new Models.Service { Name = "Haircut", Duration = 60, Price = 50.00m });
                _context.Services.Add(new Models.Service { Name = "Manicure", Duration = 45, Price = 30.00m });
                _context.SaveChanges();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Email == EmailTextBox.Text && u.PasswordHash == PasswordBox.Password);

            if (user != null)
            {
                Window dashboard;
                if (user.Role == "Client")
                {
                    dashboard = new ClientDashboard(user);
                }
                else
                {
                    dashboard = new MasterDashboard(user);
                }
                dashboard.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Invalid email or password.");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Password recovery is not implemented yet.");
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
