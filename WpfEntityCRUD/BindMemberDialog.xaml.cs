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
    /// Interaction logic for BindMemberDialog.xaml
    /// </summary>
    public partial class BindMemberDialog : Window
    {
        public int CellID { get; private set; }
        public string Id { get; private set; } // Change type to string to handle the MemberCode as a string
        private bool Unbind { get; set; } // Flag to check if unbind is selected

        public BindMemberDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate user input
            if (string.IsNullOrEmpty(MemberCodeTextBox.Text) && !Unbind) // Ensure MemberCode is provided unless unbinding
            {
                MessageBox.Show("Please provide a valid Member Code or check the unbind option.");
                return;
            }

            if (!int.TryParse(CellIDTextBox.Text, out int cellId)) // Validate CellID
            {
                MessageBox.Show("Please provide a valid Cell ID.");
                return;
            }

            CellID = cellId;

            // If unbinding, set the ID to "1" automatically
            Id = Unbind ? "1" : MemberCodeTextBox.Text; // If unbinding, set Id to "1"

            DialogResult = true; // Indicate success
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Indicate cancellation
            Close();
        }

        // Event handler for when the Unbind checkbox is checked
        private void UnbindCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Unbind = true;
            MemberCodeTextBox.IsEnabled = false; // Disable the MemberCode TextBox if unbinding
        }

        // Event handler for when the Unbind checkbox is unchecked
        private void UnbindCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Unbind = false;
            MemberCodeTextBox.IsEnabled = true; // Enable the MemberCode TextBox if binding
        }
    }
}
