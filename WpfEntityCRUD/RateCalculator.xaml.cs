using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    /// Interaction logic for RateCalculator.xaml
    /// </summary>
    public partial class RateCalculator : Window
    {
        private readonly string _apiKey = "h7uveupTw/r7Cmbvqacxi7xcHKyJWwj7N6gbYOiuXR0"; // Replace with your ShipEngine API key
        private readonly HttpClient _httpClient;
        public RateCalculator()
        {
            _httpClient = new HttpClient();
           
_httpClient.DefaultRequestHeaders.Add("API-Key", _apiKey); 
            _httpClient.BaseAddress = new Uri("https://api.shipengine.com/");
            InitializeComponent();
            var carriers = new List<Carrier>
    {
        new Carrier { CarrierId = "se-1427687", CarrierName = "Sendle" },
        new Carrier { CarrierId = "se-1427688", CarrierName = "CouriersPlease" },
        new Carrier { CarrierId = "se-1427689", CarrierName = "Aramex AU" }
    };

            cmbCarrier.ItemsSource = carriers;
            cmbCarrier.DisplayMemberPath = "CarrierName";

        }
        public class Carrier
        {
            public string CarrierId { get; set; }
            public string CarrierName { get; set; }
        }
        private async void cmbCarrier_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Ensure that a carrier is selected
            if (cmbCarrier.SelectedItem != null)
            {
                // Retrieve the selected carrier ID from the Carrier object
                var selectedCarrierId = (cmbCarrier.SelectedItem as Carrier)?.CarrierId;

                // Define a dictionary with carrier IDs and their respective services
                var carrierServices = new Dictionary<string, List<string>>
        {
            // Sample data: Carrier ID -> List of services
            { "se-1427687", new List<string>
                {
                    "sendle_flat_rate", // Sendle Preferred - Pickup
                    "sendle_standard_drop_off", // Sendle Preferred - Drop Off
                    "sendle_saver_pickup", // Sendle Saver - Pickup
                    "sendle_saver_drop_off", // Sendle Saver - Drop Off
                    "sendle_express_wallet", // Sendle Express
                    "sendle_wallet_3_day_pickup", // Sendle 3-Day Guaranteed - Pickup
                    "sendle_wallet_3_day_drop_off", // Sendle 3-Day Guaranteed - Drop Off
                    "sendle_wallet_2_day_pickup", // Sendle 2-Day Guaranteed - Pickup
                    "sendle_wallet_2_day_drop_off", // Sendle 2-Day Guaranteed Drop Off
                    "sendle_wallet_flat_rate", // Sendle Preferred - Pickup (Int) (Carbon Neutral)
                    "sendle_standard_drop_off_international", // Sendle Preferred - Drop Off (Int) (Carbon Neutral)
                    "sendle_international" // Sendle International
                }
            },
            { "se-1427688", new List<string> { "couriersplease_Ground", "couriersplease_air" } }, // CouriersPlease services
            { "se-1427689", new List<string> { "aramexaU_ground", "aramexau_air" } }  // Aramex AU services
        };

                // Check if services exist for the selected carrier
                if (carrierServices.ContainsKey(selectedCarrierId))
                {
                    var services = carrierServices[selectedCarrierId];
                    cmbOperation.Items.Clear(); // Clear previous services

                    // Populate the cmbOperation ComboBox with the services
                    foreach (var service in services)
                    {
                        cmbOperation.Items.Add(service); // Add each service to the ComboBox
                    }

                    // Optionally, select the first item
                    if (cmbOperation.Items.Count > 0)
                        cmbOperation.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No services found for the selected carrier.");
                }
            }
        }
        private async void btnCalculateRates_Click(object sender, RoutedEventArgs e)
        {
            // Validate and get "From" address
            if (string.IsNullOrWhiteSpace(txtFromName.Text) || string.IsNullOrWhiteSpace(txtFromPhone.Text) ||
                string.IsNullOrWhiteSpace(txtFromAddress.Text) || string.IsNullOrWhiteSpace(txtFromCity.Text) ||
                string.IsNullOrWhiteSpace(txtFromState.Text) || string.IsNullOrWhiteSpace(txtFromPostal.Text) ||
                string.IsNullOrWhiteSpace(txtFromCountry.Text))
            {
                MessageBox.Show("Please fill out all the 'From' address fields.");
                return;
            }

            var fromAddress = new
            {
                name = txtFromName.Text,
                phone = txtFromPhone.Text,
                company_name = txtFromCompany.Text,
                address_line1 = txtFromAddress.Text,
                city_locality = txtFromCity.Text,
                state_province = txtFromState.Text,
                postal_code = txtFromPostal.Text,
                country_code = txtFromCountry.Text
            };

            // Validate and get "To" address
            if (string.IsNullOrWhiteSpace(txtToName.Text) || string.IsNullOrWhiteSpace(txtToPhone.Text) ||
                string.IsNullOrWhiteSpace(txtToAddress.Text) || string.IsNullOrWhiteSpace(txtToCity.Text) ||
                string.IsNullOrWhiteSpace(txtToState.Text) || string.IsNullOrWhiteSpace(txtToPostal.Text) ||
                string.IsNullOrWhiteSpace(txtToCountry.Text))
            {
                MessageBox.Show("Please fill out all the 'To' address fields.");
                return;
            }

            var toAddress = new
            {
                name = txtToName.Text,
                phone = txtToPhone.Text,
                company_name = txtToCompany.Text,
                address_line1 = txtToAddress.Text,
                city_locality = txtToCity.Text,
                state_province = txtToState.Text,
                postal_code = txtToPostal.Text,
                country_code = txtToCountry.Text
            };

            // Validate and get package weight
            if (!double.TryParse(txtPackageWeight.Text, out double weight))
            {
                MessageBox.Show("Please enter a valid package weight.");
                return;
            }

            // Validate the ComboBox selection for weight unit
            var selectedUnit = cmbWeightUnit.SelectedItem as ComboBoxItem;
            if (selectedUnit == null)
            {
                MessageBox.Show("Please select a weight unit.");
                return;
            }

            var packageDetails = new
            {
                package_code = "package",
                weight = new
                {
                    value = double.Parse(txtPackageWeight.Text),
                    unit = selectedUnit.Content.ToString()
                }
            };
            var selectedCarrierId = (cmbCarrier.SelectedItem as Carrier)?.CarrierId;
            var requestData = new
            {
                rate_options = new { carrier_ids = new[] { selectedCarrierId } },
                shipment = new
                {
                    validate_address = "validate_and_clean",
                    ship_to = toAddress,
                    ship_from = fromAddress,
                    packages = new[] { packageDetails }
                }
            };

            var json = JsonConvert.SerializeObject(requestData, Formatting.Indented);

            try
            {
                var response = await _httpClient.PostAsync("https://api.shipengine.com/v1/rates",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error: {response.StatusCode} - {error}");
                }
                else
                {
                    var ratesResponse = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Rates calculated successfully! Response: {ratesResponse}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        }
}
