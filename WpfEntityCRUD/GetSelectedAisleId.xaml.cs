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
using System.Data.SqlClient;
namespace WpfEntityCRUD
{
   public partial class GetSelectedAisleId : Window
    {
        // Replace with your actual database connection string
        private readonly string connectionString = "Server=tcp:stocksolve.database.windows.net,1433;Initial Catalog=wpfCrud;Persist Security Info=False;User ID=Denis;Password=Denvlad1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        public event Action AisleDeleted;
        public GetSelectedAisleId()
        {
            InitializeComponent();
           
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate user input
            if (int.TryParse(AisleIdTextBox.Text, out int aisleId))
            {
                // Proceed with deletion
                DeleteAisleFromDatabase(aisleId);
            }
            else
            {
                MessageBox.Show("Please enter a valid Aisle ID.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
            }
         

            // Call the DeleteAisleFromDatabase method
            DeleteAisleFromDatabase(aisleId);

            // Raise the AisleDeleted event to notify the parent window
            AisleDeleted?.Invoke();
        }
     

        private void DeleteAisleFromDatabase(int aisleId)
        {
            string deleteCellsQuery = "DELETE FROM Cells WHERE AisleID = @AisleID;";
            string deleteAisleQuery = "DELETE FROM Aisles WHERE AisleID = @AisleID;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Delete dependent cells first
                        using (SqlCommand deleteCellsCmd = new SqlCommand(deleteCellsQuery, connection, transaction))
                        {
                            deleteCellsCmd.Parameters.AddWithValue("@AisleID", aisleId);
                            deleteCellsCmd.ExecuteNonQuery();
                        }

                        // Delete the aisle
                        using (SqlCommand deleteAisleCmd = new SqlCommand(deleteAisleQuery, connection, transaction))
                        {
                            deleteAisleCmd.Parameters.AddWithValue("@AisleID", aisleId);
                            deleteAisleCmd.ExecuteNonQuery();
                        }

                        // Commit transaction
                        transaction.Commit();
                        MessageBox.Show("Aisle and its dependent cells deleted successfully.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                   
                }
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the window without making any selection
            this.DialogResult = false;
            this.Close();
        }
    }
}