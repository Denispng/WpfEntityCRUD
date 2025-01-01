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

namespace WpfEntityCRUD
{
    /// <summary>
    /// Interaction logic for AisleDialog.xaml
    /// </summary>

        public partial class AisleDialog : Window
        {
            public string AisleName { get; private set; }
            public int LayerCount { get; private set; }
            public int CellCount { get; private set; }
        public int RowCount { get; private set; }
        public int CompanyId { get; private set; }

        public AisleDialog(int companyId)  
        {
            InitializeComponent();
            CompanyId = companyId; 
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(AisleNameTextBox.Text))
            {
                MessageBox.Show("Aisle name cannot be empty.");
                return;
            }

            if (!int.TryParse(LayerCountTextBox.Text, out int layers) || layers <= 0 || layers > 20)
            {
                MessageBox.Show("Layer count must be a numerical value between 1 and 20.");
                return;
            }

            if (!int.TryParse(RowCountTextBox.Text, out int rows) || rows <= 0 || rows > 20)
            {
                MessageBox.Show("Row count must be a numerical value between 1 and 20.");
                return;
            }

            if (!int.TryParse(CellCountTextBox.Text, out int cells) || cells <= 0)
            {
                MessageBox.Show("Cell count must be a positive numerical value.");
                return;
            }

            // Assign values
            AisleName = AisleNameTextBox.Text;
            LayerCount = layers;
            CellCount = cells;
            RowCount = rows;

            // Close the dialog
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            {
                DialogResult = false; // Indicate cancellation
                Close();
            }
        }
    }


