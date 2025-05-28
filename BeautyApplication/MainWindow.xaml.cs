using BeautyApplication.Data;
using BeautyApplication.Views;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace BeautyApplication
{
    public partial class MainWindow : Window
    {
        private readonly SalonContext _context;

        public MainWindow()
        {
            InitializeComponent();
            _context = new SalonContext();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            _context.Database.EnsureCreated();
            if (!_context.Users.Any())
            {
                _context.Users.Add(new Models.User { Name = "Test Client", Email = "client@example.com", Role = "Client", PasswordHash = "test" });
                _context.Users.Add(new Models.User { Name = "Test Master", Email = "master@example.com", Role = "Master", PasswordHash = "test" });
                _context.SaveChanges();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Email == EmailTextBox.Text && u.PasswordHash == PasswordBox.Password);

            if (user != null)
            {
                var dashboard = user.Role == "Client" ? (Window)new ClientDashboard(user) : new MasterDashboard(user);
                dashboard.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Incorrect email or password.");
            }
        }
    }
}
