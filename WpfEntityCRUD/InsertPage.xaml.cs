using System;
using System.Collections.Generic;
using System.IO;
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
using ZXing;
using System.Drawing;
using ZXing.Common;
using System.Drawing.Imaging;
using ZXing.Rendering;
using WpfEntityCRUD;
using System.Windows.Media.Animation;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;

namespace WpfEntityCRUD
{
    public partial class InsertPage : Window
    {
        wpfCrudEntities1 _db = new wpfCrudEntities1();

        public InsertPage()
        {
            InitializeComponent();
            LoadWarehouses();
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

               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading warehouses: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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


        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var companyId = LoginWindow.LoggedInUser.company_id;

            // Step 2: Validate if the company_id exists in the Company table
            using (var _db = new wpfCrudEntities1())
            {
                if (companyId == null)
                {
                    MessageBox.Show("Invalid company. Unable to add the member.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // Step 1: Generate a unique barcode
                string uniqueBarcode = GenerateUniqueBarcode();

                var selectedWarehouseId = warehouseComboBox.SelectedValue;
                string sold = "0";

                if (selectedWarehouseId != null)
                {
                    int warehousesId = (int)selectedWarehouseId; // Ensure the value is an integer
                    var warehouse = _db.Warehouses.SingleOrDefault(w => w.warehousesId == warehousesId);

                    if (warehouse == null)
                    {
                        MessageBox.Show("The specified warehouse does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        member newMember = new member()
                        {
                            Description = nametextBox.Text,
                            Quantity = quantitytextBox.Text,
                            warehousesId = warehousesId,
                            Date = datetextBox.Text,
                            Supplier = suppliertextBox.Text,
                            Price = pricetextBox.Text,
                            Despatched = dispatchedtextBox.Text,
                            Barcode = uniqueBarcode,
                            company_id = companyId.Value,
                            Sold = sold

                        };  


                        // Step 3: Save the new member to the database
                        _db.members.Add(newMember);
                        _db.SaveChanges();

                        // Step 4: Generate and save the barcode as an image
                        SaveBarcodeAsImage(uniqueBarcode, newMember.Id);
                        string details = $"New member added: {newMember.Description}, {newMember.Quantity}, {newMember.Code}, {newMember.warehousesId}, {newMember.Date}, {newMember.Supplier}, {newMember.Price}, {newMember.Despatched}, Barcode: {newMember.Barcode}";
                        LogHistory(newMember.Id, "INSERT", details);







                        try
                        {
                            // The grid or any UIElement you want to save as an image
                            var gridToCapture = this.captureContainer as UIElement;

                            if (gridToCapture == null)
                            {
                                MessageBox.Show("Nothing to save as an image!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // Create a RenderTargetBitmap object
                            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                                (int)this.ActualWidth, // Width of the window
                                (int)this.ActualHeight, // Height of the window
                                96, // DPI
                                96, // DPI
                                PixelFormats.Pbgra32); // Pixel format

                            renderBitmap.Render(gridToCapture);

                            // Encode the RenderTargetBitmap to a PNG image
                            PngBitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                            // Define the folder path on the Desktop
                            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                            string folderPath = System.IO.Path.Combine(desktopPath, "Invoices");

                            // Create the folder if it doesn't exist
                            if (!Directory.Exists(folderPath))
                            {
                                Directory.CreateDirectory(folderPath);
                            }

                            // Define the image file path
                            string filePath = System.IO.Path.Combine(folderPath, $"Invoice_{DateTime.Now:yyyyMMddHHmmss}.png");

                            // Save the image to the file
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                encoder.Save(fileStream);
                            }

                            MessageBox.Show($"Invoice saved as image at {filePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error saving as image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                            // Open MainWindow and close the current window
                            MainWindow anotherWindow = new MainWindow();
                            anotherWindow.Show();
                            this.Close();
                        }



                        MainWindow.datagrid.ItemsSource = _db.members.Where(m => m.company_id == companyId && m.Id != 1).ToList(); // Filter by company_id
                        this.Hide();
                    }
                }
            }
        }

        // Method to generate a unique barcode
        private string GenerateUniqueBarcode()
        {
            return "ITEM" + Guid.NewGuid().ToString("N").Substring(0, 8); // Generate a unique 8-character string
        }



        private void LogHistory(int memberId, string action, string details)
        {
            var username = LoginWindow.LoggedInUser.Username; // Assuming you have a logged-in user object

            var historyRecord = new TaskHistory
            {
                TaskId = memberId,
                Action = action,
                ChangedBy = username,
                ChangeDate = DateTime.Now,
                Details = details
            };

            // Save the history record
            using (var _db = new wpfCrudEntities1())
            {
                _db.TaskHistories.Add(historyRecord);
                _db.SaveChanges();
            }
        }

        
            // Open MainWindow and close the current window
         
    private void SaveBarcodeAsImage(string barcodeText, int memberId)
{
    try
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.CODE_128,
            Options = new ZXing.Common.EncodingOptions
            {
                Height = 150,
                Width = 300,
                Margin = 2
            },
            Renderer = new BitmapRenderer()
        };

        // Generate the barcode bitmap
        Bitmap barcodeBitmap = writer.Write(barcodeText);

        // Define the directory to save the barcode
        string directory = @"C:\Barcodes\";
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        // Define file path explicitly using System.IO.Path
        string filePath = System.IO.Path.Combine(directory, $"Barcode_{memberId}.png");

        // Save the barcode image
        barcodeBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

        MessageBox.Show($"Barcode saved at: {filePath}");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error saving barcode image: {ex.Message}");
    }
}
    }
}
