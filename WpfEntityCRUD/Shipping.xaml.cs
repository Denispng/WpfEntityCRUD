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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace WpfEntityCRUD
{
    /// <summary>
    /// Interaction logic for Shipping.xaml
    /// </summary>
    public partial class Shipping : Page
    {
        private readonly string _apiKey = "h7uveupTw/r7Cmbvqacxi7xcHKyJWwj7N6gbYOiuXR0";
        private readonly HttpClient _httpClient;

        public Shipping()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("API-Key", _apiKey);
            _httpClient.BaseAddress = new Uri("https://api.shipengine.com/");
            LoadCarriers();

        }

        // Generate Shipping Label
        private void btnGenerateLabel_Click(object sender, RoutedEventArgs e)
        {
            ShippingLabel shippingLabel = new ShippingLabel();
            shippingLabel.Show();
        }
     
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Window.GetWindow(this)?.Close();
        }
        private async void btn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("The selected option is unavailable for your current version of StockSolve. Please refer to the carrier's website for more information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            var selectedCarrier = cmbCarrier.SelectedItem as Carrier;
            var trackingNumber = txtTrackingNumber.Text;

            if (selectedCarrier == null || string.IsNullOrEmpty(trackingNumber))
            {
                MessageBox.Show("Please select a carrier and enter a tracking number.");
                return;
            }

            try
            {
                var trackingResponse = await TrackShipmentAsync(selectedCarrier.CarrierId, trackingNumber);
                txtTrackingResult.Text = $"Tracking Information: {trackingResponse}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        public async Task<string> TrackShipmentAsync(string carrierCode, string trackingNumber)

        {
         
            var response = await _httpClient.GetAsync($"v1/tracking?carrier_code={carrierCode}&tracking_number={trackingNumber}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode} - {error}");
            }

            var trackingData = await response.Content.ReadAsStringAsync();
            dynamic trackingResponse = JsonConvert.DeserializeObject(trackingData);

            // Extract tracking information (this part may vary depending on the actual response structure)
            var status = trackingResponse?.status?.ToString() ?? "No status available";
            return $"Status: {status}";
        }
    

public async Task<string> GetRatesAsync(string carrierId, string fromAddress, string toAddress, double weight)
        {
            var requestData = new
            {
                rate_options = new { carrier_ids = new[] { carrierId } },
                shipment = new
                {
                    ship_from = fromAddress,
                    ship_to = toAddress,
                    packages = new[] { new { weight = new { value = weight, unit = "pound" } } }
                }
            };

            var json = JsonConvert.SerializeObject(requestData);
            var response = await _httpClient.PostAsync("v1/rates", new StringContent(json, Encoding.UTF8, "application/json"));
            return await response.Content.ReadAsStringAsync();
        }

        public class Carrier
        {
            public string Name { get; set; }
            public string CarrierId { get; set; }
        }

        private void LoadCarriers()
        {
            var carriers = new List<Carrier>
    {
        new Carrier { Name = "Sendle", CarrierId = "se-1427687" },
        new Carrier { Name = "CouriersPlease", CarrierId = "se-1427688" },
        new Carrier { Name = "Aramex AU", CarrierId = "se-1427689" }
    };

            cmbCarrier.ItemsSource = carriers;  // Bind to the ComboBox
        }
        public async Task<string> CreateLabelAsync(
            string carrierId,        // Carrier ID
            string serviceCode,      // Shipping service code
            dynamic fromAddress,     // From Address object
            dynamic toAddress,       // To Address object
            double weight)           // Package weight
        {
            var requestData = new
            {
                shipment = new
                {
                    service_code = serviceCode,
                    carrier_id = carrierId,
                    ship_from = fromAddress,
                    ship_to = toAddress,
                    packages = new[]
                    {
                        new { weight = new { value = weight, unit = "pound" } }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestData, Formatting.Indented);
            Console.WriteLine("Request JSON: " + json); // Debugging

            var response = await _httpClient.PostAsync("v1/labels",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("The selected option is unavailable for your current version of StockSolve. Please refer to the carrier's website for more information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            RateCalculator rateCalculator = new RateCalculator();
            rateCalculator.Show();
        }
    }
}