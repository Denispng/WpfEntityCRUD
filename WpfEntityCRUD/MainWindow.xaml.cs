using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfEntityCRUD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        wpfCrudEntities1 _db = new wpfCrudEntities1();
        public static DataGrid datagrid;


        public MainWindow()
        {
            InitializeComponent();
      
            SetUserAccess();
            LoadCompanySpecificData();


        }
        
        private void SetUserAccess()
        {
            if (LoginWindow.LoggedInUser.Role == "Admin")
            {
                addUserBtn.IsEnabled = true;
                deleteUserBtn.IsEnabled = true;
                assignTaskBtn.IsEnabled = true;
            }
            else
            {
                addUserBtn.IsEnabled = false;
                deleteUserBtn.IsEnabled = false;
                assignTaskBtn.IsEnabled = false;
                addUserBtn.Visibility = Visibility.Collapsed;
                deleteUserBtn.Visibility = Visibility.Collapsed;
                assignTaskBtn.Visibility = Visibility.Collapsed;
            }
        
        }
        private void LoadCompanySpecificData()
        {
            var companyId = LoginWindow.LoggedInUser.company_id; // Retrieve company_id from logged-in user

            using (var _db = new wpfCrudEntities1())
            {
                // Filter TaskHistories based on company_id
                var companyData = _db.TaskHistories
                                     .Where(th => th.company_id == companyId)
                                     .ToList();

                // Filter members based on company_id and exclude Id = 1
                var filteredMembers = _db.members
                                          .Where(m => m.company_id == companyId && m.Id != 1)
                                          .ToList();

                // Bind the filtered members to the DataGrid
                myDataGrid.ItemsSource = filteredMembers;
                datagrid = myDataGrid;
            }
        }
        private void List_Click(object sender, RoutedEventArgs e)
        {
            var listWindow = new PackingListWindow();
            this.Content = listWindow;
           
        }
        private void addUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddUserWindow();
            addUserWindow.ShowDialog();
        }
        private void warehousesBtn_Click(object sender, RoutedEventArgs e)
        {
            var warehouse = new Warehouses();
            this.Content = warehouse;
        }

        private void assignTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            var assignTaskWindow = new AssignTaskWindow();
            assignTaskWindow.ShowDialog();
        }
        private void History_Click(object sender, RoutedEventArgs e)
        {
            var loadHistory = new LoadHistory();
            this.Content = loadHistory;
        }
        private void Tax_Click(object sender, RoutedEventArgs e)
        {
            var tax = new Tax();
            this.Content = tax;
        }


        private void viewTasksBtn_Click(object sender, RoutedEventArgs e)
        {
            var taskWindow = new ViewTasksWindow();
            taskWindow.ShowDialog();
        }
        private void deleteUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var deleteUserWindow = new DeleteUserWindow();
            deleteUserWindow.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SideMenu.Visibility = Visibility.Visible;

            // Start the SlideOut animation
            Storyboard slideOut = (Storyboard)FindResource("SlideOut");
            slideOut.Begin();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            // Automatically open the side menu when the window is activated
            SideMenu.Visibility = Visibility.Visible;

            // Start the SlideOut animation
            Storyboard slideOut = (Storyboard)FindResource("SlideOut");
            slideOut.Begin();
        }
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Make the SideMenu visible before playing the animation
            SideMenu.Visibility = Visibility.Visible;

            // Start the SlideOut animation
            Storyboard slideOut = (Storyboard)FindResource("SlideOut");
            slideOut.Begin();
        }

        private void CloseMenu_Click(object sender, RoutedEventArgs e)
        {
            // Start the SlideIn animation
            Storyboard slideIn = (Storyboard)FindResource("SlideIn");
            slideIn.Completed += (s, ev) =>
            {
                // Hide the SideMenu after the animation completes
                SideMenu.Visibility = Visibility.Collapsed;
            };
            slideIn.Begin();
        }
     
        private void insertBtn_Click(object sender, RoutedEventArgs e)
        {
            InsertPage Ipage = new InsertPage();
            Ipage.ShowDialog();
        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedMember = myDataGrid.SelectedItem as member;
            if (selectedMember != null)
            {
              
                UpdatePage Upage = new UpdatePage(selectedMember);
                Upage.ShowDialog();
            }
            else
            {
                MessageBox.Show("Nothing to Update", "Notification", MessageBoxButton.OK);
            }
        }






        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            // Check if a row is selected
            var selectedMember = myDataGrid.SelectedItem as member;
            var companyId = LoginWindow.LoggedInUser.company_id;

            if (selectedMember != null)
            {
                int memberId = selectedMember.Id;

                // Check if there are any related cells in the Cells table for the selected member
                var relatedCells = _db.Cells.Where(c => c.Id == memberId).ToList();

                if (relatedCells.Any())
                {
                    // Notify user to unbind cells before deletion
                    MessageBox.Show("You need to unbind the cells(Map) from this Item before deleting.", "Deletion Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Prevent deletion if there are related cells
                }

                // Find the member to delete
                var deleteMember = _db.members.Where(m => m.Id == memberId).SingleOrDefault();

                if (deleteMember != null)
                {
                    // Log the delete action BEFORE removing the record
                    LogHistory(deleteMember.Id, "DELETE", $"Deleted Member: {deleteMember.Description}");

                    // Remove the record from the database
                    _db.members.Remove(deleteMember);
                    _db.SaveChanges();

                    // Refresh the DataGrid and apply filtering to exclude the empty row
                    myDataGrid.ItemsSource = _db.members.Where(m => m.company_id == companyId && m.Id != 1).ToList();

                    MessageBox.Show("Member deleted successfully.", "Notification", MessageBoxButton.OK);
                }
            }
            else
            {
                // Notify user if no row is selected
                MessageBoxResult result;
                result = MessageBox.Show("Nothing to Delete", "Notification", MessageBoxButton.OK);
            }
        }
        private void LogHistory(int memberId, string action, string details)
        {
            var user = LoginWindow.LoggedInUser; // Access the currently logged-in user
            var username = user.Username;
            var companyId = user.company_id;  // Assuming company_id is available in LoggedInUser

            var historyRecord = new TaskHistory
            {
                TaskId = memberId, // Replace with your relevant ID field
                Action = action,
                ChangedBy = username, // The user performing the action
                ChangeDate = DateTime.Now,
                Details = details,
                company_id = companyId  // Store the company_id with the history record
            };

            // Save the history record to the database
            using (var _db = new wpfCrudEntities1())
            {
                _db.TaskHistories.Add(historyRecord);
                _db.SaveChanges();
            }
        }
      
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text;
            int searchId;
            bool isIdSearch = int.TryParse(searchText, out searchId);

            var companyId = LoginWindow.LoggedInUser.company_id;

            using (var _db = new wpfCrudEntities1())
            {
                // If the search box is empty, reload the original data
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // Fetch original data for the company
                    var originalData = _db.members
                        .Where(m => m.company_id == companyId && m.Id != 1) // Exclude member with Id = 1
                        .ToList(); // Retrieve the original members list

                    // Update the DataGrid with the full data
                    myDataGrid.ItemsSource = originalData;
                }
                else
                {
                    var filteredData = _db.members
     .Where(m =>
         m.company_id == companyId && // Ensure company_id is filtered
         (isIdSearch && m.Id == searchId) || // Check if the search text is an integer and match the Id
         (m.Description.ToLower().Contains(searchText.ToLower()) || // Search in Description
          m.warehousesId.HasValue && m.warehousesId.Value.ToString().ToLower().Contains(searchText.ToLower())) // Search in Location
     )
     .Where(m => m.Id != 1) // Exclude the member with Id = 1
     .ToList(); // ToList() returns a List<Member>

                    // Update the DataGrid with filtered data
                    myDataGrid.ItemsSource = filteredData;
                }
            }
        }
        private void Shipping_Click(object sender, RoutedEventArgs e)
        {
            Shipping shipping = new Shipping();
            this.Content = shipping;
        }
        private void statsBtn_Click(Object sender, RoutedEventArgs e)
        {
            Stats stats = new Stats();
            this.Content = stats;
        }
        private void InvoicesBtn_Click(object sender, RoutedEventArgs e)
        {
             Invoices invoices = new Invoices(); 
            this.Content = invoices;
        }
        private void forecastingBtn_Click(Object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=tcp:stocksolve.database.windows.net,1433;Initial Catalog=wpfCrud;Persist Security Info=False;User ID=Denis;Password=Denvlad1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"; // Replace with your connection string.

            string query = "UPDATE members SET Quantity_r = ROUND((CAST(Sold AS FLOAT) / 30.0) * 7.0, 0);";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            Forecasting forecasting = new Forecasting();
            this.Content = forecasting;
        }
        private void supportBtn_Click(Object sender, RoutedEventArgs e)
        {
            Support support = new Support();
            this.Content = support;
        }
        private void MapBtn_Click(Object sender, RoutedEventArgs e)
        {
            Map map = new Map();
                this.Content = map;
        }
        private void marketBtn_Click( Object sender, RoutedEventArgs e)
        {
            Market market = new Market();
        this.Content = market;
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
