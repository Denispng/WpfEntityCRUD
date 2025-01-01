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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfEntityCRUD
{
    /// <summary>
    /// Interaction logic for Market.xaml
    /// </summary>
    public partial class Market : Page
    {
        wpfCrudEntities1 _db = new wpfCrudEntities1();
        public static DataGrid datagrid;

        public Market()
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
            var companyId = LoginWindow.LoggedInUser.company_id;

            // Filter members by company_id and exclude Id = 1 (if needed)
            myDataGrid5.ItemsSource = _db.members
                                          .Where(m => m.company_id == companyId && m.Id != 1)
                                          .ToList();

            datagrid = myDataGrid5;
        }

      
        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedMember = myDataGrid5.SelectedItem as member;
            if (selectedMember != null)
            {
                int Id = (myDataGrid5.SelectedItem as member).Id;
                UpdateSold Upage = new UpdateSold(Id);
                Upage.ShowDialog();

            }
            else
            {


                MessageBoxResult result;
                result = MessageBox.Show("Choose an item first", "Notification", MessageBoxButton.OK);

            }


        }

        
        private void DismissBtn_Click(Object sender, RoutedEventArgs e)
        {
            // Create the fade-out animation
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                From = 1.0,  // Starting opacity
                To = 0.0,    // Ending opacity
                Duration = new Duration(TimeSpan.FromSeconds(1)) // Duration of 1 second
            };

            // Set an event handler to collapse the grid after animation completes
            fadeOutAnimation.Completed += (s, ev) =>
            {
                grid.Visibility = Visibility.Collapsed;
            };

            // Start the animation on the grid's Opacity property
            grid.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
        }

    }
}
