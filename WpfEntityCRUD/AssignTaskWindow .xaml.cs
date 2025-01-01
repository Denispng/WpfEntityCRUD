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
    /// Interaction logic for AssignTaskWindow.xaml
    /// </summary>
    public partial class AssignTaskWindow : Window
    {
        private wpfCrudEntities1 _db = new wpfCrudEntities1();

        public AssignTaskWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
           
            var companyId = LoginWindow.LoggedInUser.company_id;

          
            var users = _db.Users
                           .Where(u => u.Role != "Admin" && u.company_id == companyId)
                           .ToList();

            assignedToComboBox.ItemsSource = users;
            assignedToComboBox.DisplayMemberPath = "Username";
            assignedToComboBox.SelectedValuePath = "UserId";
      
        }

        private void assignTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            int assignedTo = (int)assignedToComboBox.SelectedValue;
            string message = taskTextBox.Text;
            DateTime dateCreated = dateCreatedPicker.SelectedDate.Value;

            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Task message cannot be empty.");
                return;
            }

            var task = new Task
            {
                AssignedTo = assignedTo,
                Message = message,
                CreatedBy = LoginWindow.LoggedInUser.UserId,
                  DateCreated = dateCreated
            };

            _db.Tasks.Add(task);
            _db.SaveChanges();
            MessageBox.Show("Task assigned successfully.");
            this.Close();
        }
    }
}
