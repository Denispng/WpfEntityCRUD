using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
using static WpfEntityCRUD.PackingListWindow;

namespace WpfEntityCRUD
{
    public partial class UpdateWarehouses : Window
    {
        private readonly wpfCrudEntities1 _db = new wpfCrudEntities1();
        private readonly int _warehousesId;

        public UpdateWarehouses(int warehousesId)
        {
            InitializeComponent();
            _warehousesId = warehousesId;
        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the warehouse to update
            var warehouseToUpdate = _db.Warehouses.SingleOrDefault(w => w.warehousesId == _warehousesId);

            if (warehouseToUpdate != null)
            {
                // Update warehouse properties
                warehouseToUpdate.Name = locationtextBox.Text;
                warehouseToUpdate.Location = addresstextBox.Text;

                // Save changes to the database
                _db.SaveChanges();

          

                // Optionally, close the update window
                this.Close();
            }
            else
            {
                MessageBox.Show("Warehouse not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            var companyId = LoginWindow.LoggedInUser.company_id;
            Warehouses.datagrid.ItemsSource = _db.Warehouses.Where(m => m.company_id == companyId)
                                       .ToList();
        }
    }
}