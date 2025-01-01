using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
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
using System.Xml;
using Newtonsoft.Json;

namespace WpfEntityCRUD
{
    /// <summary>
    /// Interaction logic for PackingListWindow.xaml
    /// </summary>
    public partial class PackingListWindow : Page
    {
        public PackingListWindow()
        {
            InitializeComponent();
            LoadAvailableItems();
            _dbContext = new wpfCrudEntities1();
            LoadPackingLists();
            AvailableItemsListBox.PreviewMouseLeftButtonDown += AvailableItemsListBox_PreviewMouseLeftButtonDown;
        }
        private wpfCrudEntities1 _db;
        private wpfCrudEntities1 _dbContext;
        private void AvailableItemsListBox_Drop(object sender, DragEventArgs e)
        {
            var droppedItem = e.Data.GetData(typeof(Member)) as Member;
            if (droppedItem != null)
            {
                // Ensure that the item being dropped is not already in the PackingListBox
                bool alreadyExists = PackingListBox.Items.Cast<Member>().Any(item => item.Id == droppedItem.Id);
                if (alreadyExists)
                {
                    MessageBox.Show("This item is already in the packing list.", "Duplicate Item", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    // You can now add the item to your packing list
                    PackingListBox.Items.Add(droppedItem);
                }
            }
        }
        private void SearchTextBox_TextChanged1(object sender, TextChangedEventArgs e)
        {
            // Retrieve search text from the TextBox
            string search = (sender as TextBox)?.Text?.Trim();

            int searchId;
            bool isIdSearch = int.TryParse(search, out searchId);

            // Get the current user's company_id
            var companyId = LoginWindow.LoggedInUser.company_id;

            using (var _db = new wpfCrudEntities1())
            {
                // Start with all TaskHistories for the logged-in user's company
                var filteredData = _db.PackingLists
                    .Where(t => t.company_id == companyId) // Filter by company ID
                    .ToList();

                // Apply search filtering
                if (!string.IsNullOrEmpty(search))
                {
                    if (isIdSearch) // Search by numeric ID
                    {
                        filteredData = filteredData
                            .Where(t => t.Id == searchId) // Assuming TaskHistories has an Id column
                            .ToList();
                    }
                    else // Search by description or other text fields
                    {
                        filteredData = filteredData
                            .Where(t => t.Code.Contains(search) ||
                                        t.Description.Contains(search) ) 
                            .ToList();
                    }
                }

                // Re-bind the filtered data to the DataGrid
                myDataGrid10.ItemsSource = filteredData;
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Window.GetWindow(this)?.Close();
        }


        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text;

            // Check if searchText can be converted to an integer
            int searchId;
            bool isIdSearch = int.TryParse(searchText, out searchId);

            // Get the company_id from the logged-in user
            var companyId = LoginWindow.LoggedInUser.company_id.GetValueOrDefault(0);

            // Fetch filtered data from the database using _dbContext
            var filteredData = _dbContext.members

     .Where(m =>
         m.company_id == companyId && // Ensure company_id is filtered
         (isIdSearch && m.Id == searchId) || // Check if the search text is an integer and match the Id
         (m.Description.ToLower().Contains(searchText.ToLower()) || // Search in Description
          m.warehousesId.HasValue && m.warehousesId.Value.ToString().ToLower().Contains(searchText.ToLower())) // Search in Location
     )
                 .Where(m => m.Id != 1 && m.company_id == companyId) // Exclude the member with Id 1 and filter by company_id
                .ToList();

            // Map the fetched members to PackingListWindow.Member objects
            var mappedMembers = filteredData.Select(m => new Member
            {
                Id = m.Id,
                Code = m.Code,
                Description = m.Description,
                Quantity = m.Quantity,
                Price = m.Price
            }).ToList();

            // Clear the ObservableCollection and add the mapped items
            _filteredMembers.Clear();
            foreach (var member in mappedMembers)
            {
                _filteredMembers.Add(member);
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Reset to original list
                AvailableItemsListBox.ItemsSource = originalMembers.Where(m => m.Id != 1).ToList();
            }
            else
            {
                // Update the AvailableItemsListBox with filtered data
                AvailableItemsListBox.ItemsSource = filteredData;
            }

            // Re-attach the PreviewMouseLeftButtonDown event handler
            AvailableItemsListBox.PreviewMouseLeftButtonDown -= AvailableItemsListBox_PreviewMouseLeftButtonDown;
            AvailableItemsListBox.PreviewMouseLeftButtonDown += AvailableItemsListBox_PreviewMouseLeftButtonDown;
        }
        private ObservableCollection<Member> _filteredMembers = new ObservableCollection<Member>();

        private void LoadAvailableItems()
        {
            // Get the company_id from the logged-in user (with a fallback default to 0 if null)
            var companyId = LoginWindow.LoggedInUser.company_id.GetValueOrDefault(0);

            string connectionString = "Server=tcp:stocksolve.database.windows.net,1433;Initial Catalog=wpfCrud;Persist Security Info=False;User ID=Denis;Password=Denvlad1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            string query = "SELECT * FROM members WHERE company_id = @companyId";

            List<Member> members = new List<Member>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@companyId", companyId);  // Add company_id filter
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    members.Add(new Member
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Code = reader["Code"].ToString(),
                        Description = reader["Description"].ToString(),
                        Quantity = reader["Quantity"].ToString(),
                        Price = reader["Price"].ToString()
                    });
                }
            }

            originalMembers = new List<Member>(members);
            AvailableItemsListBox.ItemsSource = members.Where(m => m.Id != 1).ToList();
        }
        private List<Member> originalMembers = new List<Member>();
        private void AvailableItemsListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            var item = listBox.SelectedItem as Member;
            if (item != null)
            {
                DragDrop.DoDragDrop(listBox, item, DragDropEffects.Move);
            }
        }
private void PackingListBox_Drop(object sender, DragEventArgs e)
        {
            var droppedItem = e.Data.GetData(typeof(Member)) as Member;
            if (droppedItem != null)
            {
                // Assuming PackingListBox is bound to an ObservableCollection
                var packingList = PackingListBox.ItemsSource as ObservableCollection<Member>;

                // If it's null (i.e., not bound), you might want to handle it differently
                if (packingList != null)
                {
                    // Add the dropped item to the ObservableCollection
                    packingList.Add(droppedItem);

                   
                }
                else
                {
                    // If the PackingListBox is not bound to an ObservableCollection, you can add directly to the Items collection
                    PackingListBox.Items.Add(droppedItem);
                }
            }
        }
        private void PackingListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            var item = listBox.SelectedItem as Member;
            if (item != null)
            {
                // Ask for confirmation before deleting the item
                var result = MessageBox.Show($"Are you sure you want to remove '{item.Description}'?",
                                              "Confirm Deletion",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    listBox.Items.Remove(item);
                    MessageBox.Show($"Item '{item.Description}' removed from the packing list.", "Item Removed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void SavePackingListToDatabase()
        {
            var packingListItems = new List<Member>();

            // Collect all items from the PackingListBox
            foreach (var item in PackingListBox.Items)
            {
                Member member = item as Member;
                if (member != null)
                {
                    packingListItems.Add(member);
                }
            }

            // Get the company_id from the logged-in user (with a fallback default to 0 if null)
            var companyId = LoginWindow.LoggedInUser.company_id.GetValueOrDefault(0);

            string connectionString = "Server=tcp:stocksolve.database.windows.net,1433;Initial Catalog=wpfCrud;Persist Security Info=False;User ID=Denis;Password=Denvlad1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var member in packingListItems)
                {
                    // Concatenate Code, Description, and Quantity into the Description field
                    string description = $"{member.Id} - {member.Description} - {member.Quantity}";

                    string query = "INSERT INTO PackingList (Code, Description, Quantity, company_id) " +  // Add company_id in the columns
                                   "VALUES (@Code, @Description, @Quantity, @companyId)";  // Add company_id in the values

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Code", member.Code);  // Assuming `member.Code` exists
                    command.Parameters.AddWithValue("@Description", description);  // Concatenated value
                    command.Parameters.AddWithValue("@Quantity", member.Quantity);
                    command.Parameters.AddWithValue("@companyId", companyId);  // Assign the company_id

                    command.ExecuteNonQuery();
                }
            }

            
           MessageBox.Show("Packing list saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadPackingLists();
        }
       
  
        private void LoadPackingLists()
        {
            
            var companyId = LoginWindow.LoggedInUser.company_id.GetValueOrDefault(0);

         
            var packingLists = _dbContext.PackingLists
                .Where(p => p.company_id == companyId)
                .Select(p => new
                {
                    p.Id,
                    p.Code,
                    p.Description,
                    p.Quantity
                })
                .ToList();

            myDataGrid10.ItemsSource = packingLists;
        }
        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            // Check if a row is selected
            var selectedPackingList = myDataGrid10.SelectedItem as PackingList;

            // Debugging: Check what is selected
            if (selectedPackingList == null)
            {
                MessageBox.Show("No item selected for deletion. Please select a row.");
                return;
            }

            int Id = selectedPackingList.Id;

            // Find the PackingList item to delete
            var deletePackingList = _db.PackingLists.Where(p => p.Id == Id).SingleOrDefault();

            if (deletePackingList != null)
            {
                // Log the delete action before removing the record
                LogHistory(deletePackingList.Id, "DELETE", $"Deleted Packing List: {deletePackingList.Description}");

                // Remove the record from the database
                _db.PackingLists.Remove(deletePackingList);
                _db.SaveChanges();

                // Refresh the DataGrid with the updated list
                myDataGrid10.ItemsSource = _db.PackingLists.ToList();

                MessageBox.Show("Packing List deleted successfully.", "Notification", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show("No matching item found to delete.", "Error", MessageBoxButton.OK);
            }
        }

        private void LogHistory(int id, string action, string message)
        {
            // Example of logging the action (you can implement your own log system)
            Console.WriteLine($"History: {action} - ID: {id}, Message: {message}");
        }



        private void SavePackingListBtn_Click(object sender, RoutedEventArgs e)
        {
            SavePackingListToDatabase();
        }
    
     
        public class Member
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Description { get; set; }
            public string Quantity { get; set; }
            public string Price { get; set; }


        }
    }
}
