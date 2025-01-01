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
    /// Interaction logic for AddUserWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window
    {
        private wpfCrudEntities1 _db = new wpfCrudEntities1();

        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void addUserBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordBox.Password;
            string role = roleComboBox.Text;
            int companyId = LoginWindow.LoggedInUser.company_id ?? -1;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                MessageBox.Show("All fields are required.");
                return;
            }

            var newUser = new User
            {
                Username = username,
                Password = password, // For production, hash this password!
                Role = role,
                company_id = companyId
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();
            MessageBox.Show("User added successfully.");
            this.Close();
        }
    }
}
