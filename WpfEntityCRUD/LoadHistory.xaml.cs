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
    /// Interaction logic for LoadHistory.xaml
    /// </summary>
    public partial class LoadHistory : Page
    {
        public LoadHistory()
        {
            InitializeComponent();
            LoadHistory1();
        }
        private void LoadHistory1()
        {
            // Get the company_id from the logged-in user
            var companyId = LoginWindow.LoggedInUser.company_id;

            using (var _db = new wpfCrudEntities1())
            {
                // Filter task history by company_id and order by ChangeDate
                var history = _db.TaskHistories
                                 .Where(h => h.company_id == companyId)
                                 .OrderByDescending(h => h.ChangeDate)
                                 .ToList();

                // Bind the filtered history to the DataGrid
                historyDataGrid.ItemsSource = history;
            }
        }
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Window.GetWindow(this)?.Close(); // Close the current window
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
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
                var filteredData = _db.TaskHistories
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
                            .Where(t => t.Action.Contains(search) ||
                                        t.ChangedBy.Contains(search) ||
                                        t.Details.Contains(search)) // Add other text columns as needed
                            .ToList();
                    }
                }

                // Re-bind the filtered data to the DataGrid
                historyDataGrid.ItemsSource = filteredData;
            }
        }

    }
    }
