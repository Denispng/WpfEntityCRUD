using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace WpfEntityCRUD
{
    /// <summary>
    /// Interaction logic for ReturnInvoice.xaml
    /// </summary>
    public partial class ReturnInvoice : Window
    {
        private static readonly Random rand = new Random();
        wpfCrudEntities1 _db = new wpfCrudEntities1();
        private member _selectedMember;


        public ReturnInvoice(member selectedMember)
        {
            InitializeComponent();
            _selectedMember = selectedMember;
            SetInvoiceNumber();
            PopulateFields();
        }

        private void SetInvoiceNumber()
        {
            string invoiceNumber = GenerateInvoiceNumber();
            invoiceNumberTextBox.Text = invoiceNumber;
        }
        private void PopulateFields()
        {
            if (_selectedMember != null)
            {

                Quantity.Text = _selectedMember.Quantity.ToString();
                Price.Text = _selectedMember.Price.ToString();
                Description.Text = _selectedMember.Description.ToString();

            }
        }

        public string GenerateInvoiceNumber()
        {
            // Combine current date-time with a random number for uniqueness
            string invoiceNumber = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + rand.Next(1000, 9999).ToString();
            return invoiceNumber;
        }
        private void SaveAsImage_Click(object sender, RoutedEventArgs e)
        {
            // 1. Deduct Quantity from the Database
            if (!int.TryParse(Quantity.Text, out int quantityToDeduct) || quantityToDeduct <= 0)
            {
                MessageBox.Show("Please enter a valid positive quantity.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Connection String (replace with your settings)
            string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=wpfCrud;Trusted_Connection=True;Encrypt=False;";

            try
            {
                // Deduct quantity from the database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Step 1: Retrieve the Current Quantity
                    string selectQuery = "SELECT Quantity FROM members WHERE Id = @Id";
                    SqlCommand selectCmd = new SqlCommand(selectQuery, connection);
                    selectCmd.Parameters.AddWithValue("@Id", _selectedMember.Id);

                    object result = selectCmd.ExecuteScalar();
                    if (result == null)
                    {
                        MessageBox.Show("Member not found in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    int currentQuantity = Convert.ToInt32(result);

                    int newQuantity = currentQuantity + quantityToDeduct;

                    string updateQuery = "UPDATE members SET Quantity = @NewQuantity WHERE Id = @Id";
                    SqlCommand updateCmd = new SqlCommand(updateQuery, connection);
                    updateCmd.Parameters.AddWithValue("@NewQuantity", newQuantity);
                    updateCmd.Parameters.AddWithValue("@Id", _selectedMember.Id);

                    updateCmd.ExecuteNonQuery();

                    MessageBox.Show($"Item returned successfully! New Quantity: {newQuantity}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // 2. Save the Image (Your Provided Code)
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deducting quantity: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Open MainWindow and close the current window
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            MainWindow anotherWindow = new MainWindow();
            anotherWindow.Show();

            // Optionally, close the current window (MainWindow)
            this.Close();

        }

    }
}
