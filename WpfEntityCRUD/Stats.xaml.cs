using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Diagnostics;
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
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;


namespace WpfEntityCRUD
{
    /// <summary>
    /// Interaction logic for Stats.xaml
    /// </summary>
    public partial class Stats : Page
    {
        public SeriesCollection ChartSeries { get; set; }


        wpfCrudEntities1 _db = new wpfCrudEntities1(); // Database context
        public static DataGrid datagrid;

        // Properties for chart data
        public ChartValues<int> Prices { get; set; }
        public List<string> Quantities { get; set; }

        public Stats()
        {
            ChartSeries = new SeriesCollection();
            InitializeComponent();
            Load(); // Load data when the page is initialized
            DataContext = this; // Bind the page's DataContext
        }

        private void Load()
        {
            var companyId = LoginWindow.LoggedInUser.company_id;
            var membersData = _db.members.Where(m => m.company_id == companyId).ToList();

            // Initialize the collections
            Prices = new ChartValues<int>();
            Quantities = new List<string>();
            string connectionString = "Server=tcp:stocksolve.database.windows.net,1433;Initial Catalog=wpfCrud;Persist Security Info=False;User ID=Denis;Password=Denvlad1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            string query = "SELECT Description, Quantity FROM members WHERE Id != 1 AND company_id = @company_id";


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@company_id", companyId);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ChartSeries.Add(new PieSeries
                    {
                        Title = reader["Description"].ToString(),
                        Values = new ChartValues<double> { Convert.ToDouble(reader["Quantity"]) }
                       
                    });
                }

            }


            foreach (var member in membersData)
            {
                // Ensure Quantity and Price are valid
                if (int.TryParse(member.Quantity, out var quantity))
                {
                    Quantities.Add(member.Quantity);
                }
                else
                {
                    Quantities.Add("0"); 
                }

                if (int.TryParse(member.Price, out var price))
                {
                    Prices.Add(price);
                }
                else
                {
                    Prices.Add(0);
                }
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
