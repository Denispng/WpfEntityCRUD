using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for UpdateSold.xaml
    /// </summary>
    public partial class UpdateSold : Window
    {
        wpfCrudEntities1 _db = new wpfCrudEntities1();
        int Id;

        public UpdateSold(int memberId)
        {
            InitializeComponent();
            Id = memberId;
        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            member updateMember = (from m in _db.members
                                   where m.Id == Id
                                   select m).Single();

            updateMember.Sold = soldtextBox.Text;


            var companyId = LoginWindow.LoggedInUser.company_id;
            Market.datagrid.ItemsSource =  _db.members
                                          .Where(m => m.company_id == companyId && m.Id != 1)
                                          .ToList();



            _db.SaveChanges();
            MainWindow.datagrid.ItemsSource = _db.members.ToList();
          
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=tcp:stocksolve.database.windows.net,1433;Initial Catalog=wpfCrud;Persist Security Info=False;User ID=Denis;Password=Denvlad1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"; // Replace with your connection string.

            string query = "UPDATE members SET Quantity_r = ROUND((CAST(Sold AS FLOAT) / 30.0) * 7.0, 2);";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Calculation updated!");
            this.Hide();

        }
    }
}
