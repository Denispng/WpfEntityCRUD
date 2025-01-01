using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    /// Interaction logic for Forecasting.xaml
    /// </summary>
    public partial class Forecasting : Page
    {
        wpfCrudEntities1 _db = new wpfCrudEntities1();
        public static DataGrid datagrid;
      
        public Forecasting()
        {
            InitializeComponent();
            Load();

        }

        private void Load()
        {
        
            var companyId = LoginWindow.LoggedInUser.company_id;

          
            myDataGrid4.ItemsSource = _db.members
                                          .Where(m => m.company_id == companyId && m.Id != 1)
                                          .ToList();

            datagrid = myDataGrid4;
        }

        private void insertBtn_Click(object sender, RoutedEventArgs e)
        {
            InsertPage Ipage = new InsertPage();
            Ipage.ShowDialog();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Window.GetWindow(this)?.Close(); // Close the current window
        }

    }
}
