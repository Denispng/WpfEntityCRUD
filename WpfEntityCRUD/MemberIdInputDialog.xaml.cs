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
    /// Interaction logic for MemberIdInputDialog.xaml
    /// </summary>
    public partial class MemberIdInputDialog : Window
    {
        public int MemberId { get; private set; }

        public MemberIdInputDialog()
        {
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate if the input is a valid integer
            if (int.TryParse(MemberIdTextBox.Text, out int memberId))
            {
                MemberId = memberId;
                this.DialogResult = true; // Close the dialog and signal success
            }
            else
            {
                MessageBox.Show("Please enter a valid Member ID.");
            }
        }
    }
}
