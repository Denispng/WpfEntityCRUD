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
    /// Interaction logic for GenerateInvoice.xaml
    /// </summary>
    public partial class GenerateInvoice : Window
    {
        private static readonly Random rand = new Random();
        wpfCrudEntities1 _db = new wpfCrudEntities1();
        private member _selectedMember;


        public GenerateInvoice(member selectedMember)
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
            string connectionString = "Server=tcp:stocksolve.database.windows.net,1433;Initial Catalog=wpfCrud;Persist Security Info=False;User ID=Denis;Password=Denvlad1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

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

                    // Step 2: Check if Deduction is Possible
                    if (currentQuantity < quantityToDeduct)
                    {
                        MessageBox.Show("Not enough quantity available to deduct.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    // Step 3: Update the Quantity
                    int newQuantity = currentQuantity - quantityToDeduct;
                    
                    string updateQuantityQuery = "UPDATE members SET Quantity = @NewQuantity WHERE Id = @Id";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuantityQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@NewQuantity", newQuantity);
                        updateCmd.Parameters.AddWithValue("@Id", _selectedMember.Id);

                        updateCmd.ExecuteNonQuery(); // Execute the first update (Quantity)
                    }
                    try
                    {
                       

                        // Step 1: Retrieve the Current Sold value (assumes value is never NULL)
                        string selectSoldQuery = "SELECT Sold FROM members WHERE Id = @Id";
                        int currentSold = 0;

                        using (SqlCommand selectSoldCmd = new SqlCommand(selectSoldQuery, connection))
                        {
                            selectSoldCmd.Parameters.AddWithValue("@Id", _selectedMember.Id);

                            object soldResult = selectSoldCmd.ExecuteScalar();
                            currentSold = Convert.ToInt32(soldResult);  // Direct conversion since Sold is never NULL
                        }

                        // Step 2: Update the Sold column (increment by quantityToDeduct)
                        string updateSoldQuery = "UPDATE members SET Sold = Sold + @QuantitySold WHERE Id = @Id";
                        using (SqlCommand updateCmd1 = new SqlCommand(updateSoldQuery, connection))
                        {
                            updateCmd1.Parameters.AddWithValue("@QuantitySold", quantityToDeduct);  // Increment sold quantity
                            updateCmd1.Parameters.AddWithValue("@Id", _selectedMember.Id);         // Identify the member by Id

                            int rowsAffected = updateCmd1.ExecuteNonQuery(); // Execute the update (Sold)

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show(
                                    $"Quantity deducted successfully! Remaining Quantity: {newQuantity}. Updated Sold Quantity: {currentSold + quantityToDeduct}",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information
                                );
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Error: Could not update the Sold quantity.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            connection.Close(); // Ensure the connection is closed even if an error occurs
                        }
                    }
                    MessageBox.Show($"Quantity deducted successfully! Remaining Quantity: {newQuantity}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

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
