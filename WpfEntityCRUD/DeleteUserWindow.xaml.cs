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
    /// Interaction logic for DeleteUserWindow.xaml
    /// </summary>
    public partial class DeleteUserWindow : Window
    {
        private wpfCrudEntities1 _db = new wpfCrudEntities1();

        public DeleteUserWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            var companyId = LoginWindow.LoggedInUser.company_id;

            // Populate user dropdown, excluding admins and filtering by company_id
            var users = _db.Users
                           .Where(u => u.Role != "Admin" && u.company_id == companyId)
                           .ToList();

            userComboBox.ItemsSource = users;
            userComboBox.DisplayMemberPath = "Username";
            userComboBox.SelectedValuePath = "UserId";
        }

        private void deleteUserBtn_Click(object sender, RoutedEventArgs e)
        {
            if (userComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a user to delete.");
                return;
            }

            int userIdToDelete = (int)userComboBox.SelectedValue;

            try
            {
                var userToDelete = _db.Users.Find(userIdToDelete);
                if (userToDelete != null)
                {
                    _db.Users.Remove(userToDelete);
                    _db.SaveChanges();

                    MessageBox.Show("User deleted successfully.");
                    LoadUsers(); // Refresh the user list
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting user: {ex.Message}");
            }
        }
    }
}
