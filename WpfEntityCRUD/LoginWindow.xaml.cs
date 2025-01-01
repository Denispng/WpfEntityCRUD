using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfEntityCRUD
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private wpfCrudEntities1 _db = new wpfCrudEntities1();
        public static User LoggedInUser; // Holds the logged-in user details

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordBox.Password;
            string idText = idBox.Text;

            // Validate that id is a valid integer
            if (!int.TryParse(idText, out int companyId))
            {
                MessageBox.Show("Please enter a valid Company ID!", "Error");
                return;
            }

            // Validate user credentials including company_id
            var user = _db.Users.FirstOrDefault(u => u.Username == username
                                                    && u.Password == password
                                                    && u.company_id == companyId);
            if (user != null)
            {
                LoggedInUser = user; // Store the logged-in user
                MessageBox.Show($"Welcome, {user.Username} from Company ID: {companyId}!", "Success");

                // Navigate to MainWindow, pass the company_id if needed
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username, password, or company ID!", "Error");
            }
        }
    }
}
