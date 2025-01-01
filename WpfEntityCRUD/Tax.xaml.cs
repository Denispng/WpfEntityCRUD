using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClosedXML.Excel;


namespace WpfEntityCRUD
{
    /// <summary>
    /// Interaction logic for Tax.xaml
    /// </summary>
    public partial class Tax : Page
    {
        wpfCrudEntities1 _db = new wpfCrudEntities1();
        public static System.Windows.Controls.DataGrid datagrid;


        public Tax()
        {
            InitializeComponent();
            Load();
        }
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Window.GetWindow(this)?.Close(); // Close the current window
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
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text;
            int searchId;
            bool isIdSearch = int.TryParse(searchText, out searchId);

            var companyId = LoginWindow.LoggedInUser.company_id;

            using (var _db = new wpfCrudEntities1())
            {
                // If the search box is empty, reload the original data
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // Fetch original data for the company
                    var originalData = _db.members
                        .Where(m => m.company_id == companyId && m.Id != 1) // Exclude member with Id = 1
                        .ToList(); // Retrieve the original members list

                    // Update the DataGrid with the full data
                    myDataGrid6.ItemsSource = originalData;
                }
                else
                {
                    var filteredData = _db.members
     .Where(m =>
         m.company_id == companyId && // Ensure company_id is filtered
         (isIdSearch && m.Id == searchId) || // Check if the search text is an integer and match the Id
         (m.Description.ToLower().Contains(searchText.ToLower()) || // Search in Description
          m.warehousesId.HasValue && m.warehousesId.Value.ToString().ToLower().Contains(searchText.ToLower())) // Search in Location
     )
     .Where(m => m.Id != 1) // Exclude the member with Id = 1
     .ToList(); // ToList() returns a List<Member>

                    // Update the DataGrid with filtered data
                    myDataGrid6.ItemsSource = filteredData;
                }
            }
        }
        private void generateBtn_Click1(object sender, RoutedEventArgs e)
        {
            ExportToExcel();
        }
        private void ExportToExcel()
        {
           
            var members = (List<WpfEntityCRUD.member>)myDataGrid6.ItemsSource;

            // Create a new workbook and worksheet using ClosedXML
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Members");

                // Add column headers
                worksheet.Cell(1, 1).Value = "Id";
                worksheet.Cell(1, 2).Value = "Code";
                worksheet.Cell(1, 3).Value = "Description";
                worksheet.Cell(1, 4).Value = "Quantity";
                worksheet.Cell(1, 5).Value = "Price";
                worksheet.Cell(1, 6).Value = "Sold";

                // Add data rows
                for (int i = 0; i < members.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = members[i].Id;
                    worksheet.Cell(i + 2, 2).Value = members[i].Code;
                    worksheet.Cell(i + 2, 3).Value = members[i].Description;
                    worksheet.Cell(i + 2, 4).Value = members[i].Quantity;
                    worksheet.Cell(i + 2, 5).Value = members[i].Price;
                    worksheet.Cell(i + 2, 6).Value = members[i].Sold;
                }

                string filePath = @"C:\TAXEXCEL\" + "file_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                workbook.SaveAs(filePath);
            }

            MessageBox.Show("Exported to Excel successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public class member
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Description { get; set; }
            public string Quantity { get; set; }
            public string Price { get; set; }
            public string Sold { get; set; }
        }
    }




}
