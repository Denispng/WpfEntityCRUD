using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfEntityCRUD
{
    /// <summary>
    /// Interaction logic for Warehouses.xaml
    /// </summary>
    public partial class Warehouses : Page
    {
        wpfCrudEntities1 _db = new wpfCrudEntities1();
        public static DataGrid datagrid;
        public Warehouses()
        {
            InitializeComponent();
            LoadWarehouses();
        }
        private void LoadWarehouses()
        {
            var companyId = LoginWindow.LoggedInUser.company_id; 

            using (var _db = new wpfCrudEntities1())
            {


                var filteredMembers = _db.Warehouses
                                       .Where(m => m.company_id == companyId)
                                       .ToList();

                // Bind the filtered members to the DataGrid
                warehousesDataGrid.ItemsSource = filteredMembers;
                datagrid = warehousesDataGrid;
            }
        }
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Window.GetWindow(this)?.Close(); // Close the current window
        }
        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedWarehouse = warehousesDataGrid.SelectedItem as Warehouse;
            if (selectedWarehouse != null)
            {
                // Pass the warehouse ID to the UpdateWarehouses constructor
                UpdateWarehouses updatePage = new UpdateWarehouses(selectedWarehouse.warehousesId);
                updatePage.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a warehouse to update.", "Notification", MessageBoxButton.OK);
            }
        }
    }
}
