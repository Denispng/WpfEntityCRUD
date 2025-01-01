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
    /// Interaction logic for ViewTasksWindow.xaml
    /// </summary>
    public partial class ViewTasksWindow : Window
    {
        private wpfCrudEntities1 _db = new wpfCrudEntities1();

        public ViewTasksWindow()
        {
            InitializeComponent();
            LoadTasks();
        }

        private void LoadTasks()
        {
            int currentUserId = LoginWindow.LoggedInUser.UserId;

            // Manual join with the Users table to get the CreatedBy Username
            var tasks = (from t in _db.Tasks
                         join u in _db.Users on t.CreatedBy equals u.UserId
                         where t.AssignedTo == currentUserId
                         select new
                         {
                             t.Message,
                             CreatedBy = u.Username, // Get the Username directly
                             t.DateCreated
                         }).ToList();

            tasksDataGrid.ItemsSource = tasks;
        }
    }
}
