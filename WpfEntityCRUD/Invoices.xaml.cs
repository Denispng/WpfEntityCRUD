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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfEntityCRUD
{
    /// <summary>
    /// Interaction logic for Invoices.xaml
    /// </summary>
    public partial class Invoices : Page
    {
        wpfCrudEntities1 _db = new wpfCrudEntities1();
        public static DataGrid datagrid;

        public Invoices()
        {
            InitializeComponent();
            Load();
        }
        private void Load()
        {
            // Get the company_id from the logged-in user
            var companyId = LoginWindow.LoggedInUser.company_id;

            // Filter members by company_id and exclude Id = 1 (if needed)
            myDataGrid6.ItemsSource = _db.members
                                          .Where(m => m.company_id == companyId && m.Id != 1)
                                          .ToList();

            datagrid = myDataGrid6;
        }


        private void generateBtn_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected member from the DataGrid
            var selectedMember = myDataGrid6.SelectedItem as member;
            if (selectedMember != null)
            {
                // Pass the selected member to the GenerateInvoice window
                GenerateInvoice generateInvoice = new GenerateInvoice(selectedMember);
                generateInvoice.Show();
                Window.GetWindow(this)?.Close();
            }
            else
            {
                MessageBox.Show("Please select a member to generate an invoice.", "Notification", MessageBoxButton.OK);
            }
        }
        private void generateBtn_Click1(object sender, RoutedEventArgs e)
        {
            // Get the selected member from the DataGrid
            var selectedMember = myDataGrid6.SelectedItem as member;
            if (selectedMember != null)
            {
                // Pass the selected member to the GenerateInvoice window
                ReturnInvoice generateInvoice = new ReturnInvoice(selectedMember);
                generateInvoice.Show();
                Window.GetWindow(this)?.Close();
            }
            else
            {
                MessageBox.Show("Please select a member to generate an invoice.", "Notification", MessageBoxButton.OK);
            }
        }
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Window.GetWindow(this)?.Close(); // Close the current window
        }

    }
}

    

