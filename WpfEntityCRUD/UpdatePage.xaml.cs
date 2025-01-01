using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfEntityCRUD
{
    /// <summary>
    /// Interaction logic for UpdatePage.xaml
    /// </summary>


    public partial class UpdatePage : Window
    {
        wpfCrudEntities1 _db = new wpfCrudEntities1();
        private member _selectedMember;

        public UpdatePage(member selectedMember)
        {
            InitializeComponent();
            _selectedMember = selectedMember;
            this.DataContext = _selectedMember;
            LoadWarehouses();
        }
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits using Regex
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void NumericTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Allow navigation keys (Backspace, Delete, etc.)
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right)
            {
                e.Handled = false;
            }
        }
        private void LoadWarehouses()
        {
            try
            {
                var companyId = LoginWindow.LoggedInUser.company_id;

                // Filter warehouses by company_id first, then select the required properties
                var warehouses = _db.Warehouses
                                    .Where(w => w.company_id == companyId) // Filter warehouses for the logged-in user's company
                                    .Select(w => new { w.warehousesId, w.Name }) // Select ID and Name
                                    .ToList();

                // Bind the warehouses to the ComboBox
                warehouseComboBox.ItemsSource = warehouses;
                warehouseComboBox.DisplayMemberPath = "Name";           // Display warehouse names
                warehouseComboBox.SelectedValuePath = "warehousesId";   // Use warehouse ID as the selected value

                // Set the ComboBox to display the current warehouse if available
                if (_selectedMember.warehousesId.HasValue)
                {
                    warehouseComboBox.SelectedValue = _selectedMember.warehousesId.Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading warehouses: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int.TryParse(warehouseComboBox.SelectedValue?.ToString(), out int warehousesId);
                if (warehousesId == 0)
                {
                    MessageBox.Show("Please select a valid warehouse.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                {
                    var newContext = new wpfCrudEntities1();

                    // Load the entity with the same ID from the new context
                    var updatedMember = newContext.members.Find(_selectedMember.Id);
                    var warehouse = _db.Warehouses.SingleOrDefault(w => w.warehousesId == warehousesId);
                    if (warehouse == null)
                    {
                        MessageBox.Show("The specified warehouse does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (updatedMember != null)
                    {
                        // Capture old values as a strongly typed variable
                        var oldDescription = updatedMember.Description;
                        var oldQuantity = updatedMember.Quantity;
                        var oldCode = updatedMember.Code;
                        var oldwarehouseId = updatedMember.warehousesId.HasValue ? updatedMember.warehousesId.Value.ToString() : "N/A";
                        var oldDate = updatedMember.Date;
                        var oldSupplier = updatedMember.Supplier;
                        var oldPrice = updatedMember.Price;
                        var oldDespatched = updatedMember.Despatched;

                        // Update only the fields that have been modified
                        updatedMember.Description = _selectedMember.Description;
                        updatedMember.Quantity = _selectedMember.Quantity;
                        updatedMember.Code = _selectedMember.Code;
                        updatedMember.warehousesId = warehousesId;
                        updatedMember.Date = _selectedMember.Date;
                        updatedMember.Supplier = _selectedMember.Supplier;
                        updatedMember.Price = _selectedMember.Price;
                        updatedMember.Despatched = _selectedMember.Despatched;

                        // Save changes to the database
                        newContext.SaveChanges();

                        // Generate and log change history with both old and new values
                        string details = GenerateUpdateDetails(
                            oldDescription, oldQuantity, oldCode, oldwarehouseId,
                            oldDate, oldSupplier, oldPrice, oldDespatched,
                            updatedMember.Description, updatedMember.Quantity, updatedMember.Code,
                         updatedMember.warehousesId.HasValue ? updatedMember.warehousesId.Value.ToString() : "N/A", updatedMember.Date, updatedMember.Supplier,
                            updatedMember.Price, updatedMember.Despatched
                        );

                        LogHistory(updatedMember.Id, "UPDATE", details);
                    }

                    // Refresh the DataGrid
                    var companyId = LoginWindow.LoggedInUser.company_id;
                    MainWindow.datagrid.ItemsSource = _db.members
                        .Where(m => m.Id != 1 && m.company_id == companyId)
                        .ToList();

                    // Show success message
                    MessageBox.Show("Member updated successfully", "Success", MessageBoxButton.OK);

                    this.Close();  // Close the update window
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK);
            }
        
    }
        private string GenerateUpdateDetails(
     string oldDescription, string oldQuantity, string oldCode, string oldwarehouseId,
     string oldDate, string oldSupplier, string oldPrice, string oldDespatched,
     string newDescription, string newQuantity, string newCode, string newwarehouseId,
     string newDate, string newSupplier, string newPrice, string newDespatched)

        {
            var changes = new List<string>();


            if (oldDescription != newDescription)
                changes.Add($"Description: '{oldDescription}' -> '{newDescription}'");

            if (oldQuantity != newQuantity)
                changes.Add($"Quantity: '{oldQuantity}' -> '{newQuantity}'");

            if (oldCode != newCode)
                changes.Add($"Code: '{oldCode}' -> '{newCode}'");

            if (oldwarehouseId != newwarehouseId)
                changes.Add($"warehouseId: '{oldwarehouseId}' -> '{newwarehouseId}'");

            if (oldDate != newDate)
                changes.Add($"Date: '{oldDate}' -> '{newDate}'");

            if (oldSupplier != newSupplier)
                changes.Add($"Supplier: '{oldSupplier}' -> '{newSupplier}'");

            if (oldPrice != newPrice)
                changes.Add($"Price: '{oldPrice}' -> '{newPrice}'");

            if (oldDespatched != newDespatched)
                changes.Add($"Despatched: '{oldDespatched}' -> '{newDespatched}'");

            return changes.Count > 0 ? string.Join(", ", changes) : "No changes made.";
        }
        private void LogHistory(int memberId, string action, string details)
        {
            var user = LoginWindow.LoggedInUser;
            var username = user.Username;
            var companyId = user.company_id;
            var historyRecord = new TaskHistory
            {
                TaskId = memberId,
                Action = action,
                ChangedBy = username,
                ChangeDate = DateTime.Now,
                Details = details,
                company_id = companyId
            };

            // Save the history record
            using (var _db = new wpfCrudEntities1())
            {
                _db.TaskHistories.Add(historyRecord);
                _db.SaveChanges();
            }
        }
    }
    }
