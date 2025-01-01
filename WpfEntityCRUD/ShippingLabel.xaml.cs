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
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;
using LiveCharts.Wpf.Charts.Base;
using ShipEngine.ApiClient;
using ShipEngine.ApiClient.Model;
using ShipEngine.ApiClient.Api;
using ShipEngine.ApiClient.Client;
using System.Diagnostics;


namespace WpfEntityCRUD
{
    public partial class ShippingLabel : Window
    {
        private readonly string _apiKey = "h7uveupTw/r7Cmbvqacxi7xcHKyJWwj7N6gbYOiuXR0"; // Replace with your ShipEngine API key
        private readonly HttpClient _httpClient;

        public ShippingLabel()
        {
            InitializeComponent();
            LoadCarrierList();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("API-Key", _apiKey);
            _httpClient.BaseAddress = new Uri("https://api.shipengine.com/");

            
        }

       

        // Carrier class to hold carrier information
        public class Carrier
        {
            public string Name { get; set; }
            public string CarrierId { get; set; }
        }

        // Load carrier list into ComboBox
        private void LoadCarrierList()
        {
            var carriers = new List<Carrier>
        {
            new Carrier { Name = "Sendle", CarrierId = "se-1427687" },
            new Carrier { Name = "CouriersPlease", CarrierId = "se-1427688" },
            new Carrier { Name = "Aramex AU", CarrierId = "se-1427689" }
        };

            cmbCarrier.ItemsSource = carriers;
            cmbCarrier.DisplayMemberPath = "Name";
            cmbCarrier.SelectedValuePath = "CarrierId";
        }

        // Event handler when carrier selection changes
        private async void cmbCarrier_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cmbCarrier.SelectedItem != null)
            {
                var selectedCarrierId = (cmbCarrier.SelectedItem as Carrier).CarrierId;

                // Predefined services for each carrier
                var carrierServices = new Dictionary<string, List<string>>
{
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
    // Add other carriers here in similar format...

                { "se-1427688", new List<string> { "couriersplease_Ground", "couriersplease_air" } }, // CouriersPlease services
            { "se-1427689", new List<string> { "aramexaU_ground", "aramexau_air" } }  // Aramex AU services
        };

                // Fetch services for the selected carrier
                if (carrierServices.ContainsKey(selectedCarrierId))
                {
                    var services = carrierServices[selectedCarrierId];
                    cmbOperation.Items.Clear();
                    foreach (var service in services)
                    {
                        cmbOperation.Items.Add(service); // Populate the ComboBox with service types
                    }
                }
                else
                {
                    MessageBox.Show("No services found for the selected carrier.");
                }
            }
        }


        private async void btnGenerateLabel_Click(object sender, RoutedEventArgs e)
        {
            var fromAddress = new
            {
                name = txtFromName.Text,
                address_line1 = txtFromAddress.Text,
                city_locality = txtFromCity.Text,
                state_province = txtFromState.Text,
                postal_code = txtFromPostal.Text,
                country_code = txtFromCode.Text,
                phone = txtFromPhone.Text,
                email = txtFromEmail.Text
            };

            var toAddress = new
            {
                name = txtToName.Text,
                address_line1 = txtToAddress.Text,
                city_locality = txtToCity.Text,
                state_province = txtToState.Text,
                postal_code = txtToPostal.Text,
                country_code = txtToCode.Text,
                phone = txtToPhone.Text,
                email = txtToEmail.Text
            };

            try
            {
                // Get selected carrier and service
                var selectedCarrier = cmbCarrier.SelectedItem as Carrier;
                var selectedService = cmbOperation.SelectedItem.ToString();

                if (selectedCarrier == null || string.IsNullOrEmpty(selectedService))
                {
                    MessageBox.Show("Please select a carrier and service.");
                    return;
                }

                // Get dimensions from UI fields
                var length = double.Parse(txtLength.Text);
                var width = double.Parse(txtWidth.Text);
                var height = double.Parse(txtHeight.Text);
                var weight = double.Parse(txtWeight.Text);

                var labelResponse = await CreateLabelAsync(
                    selectedCarrier.CarrierId,   // Use selected carrier ID
                    selectedService,             // Use selected service
                    fromAddress,
                    toAddress,
                    weight,
                    length,
                    width,
                    height
                );

                var labelUrl = GetLabelUrlFromResponse(labelResponse);

                if (labelUrl != null)
                {
                    MessageBox.Show($"Label generated successfully! Download it from: {labelUrl}");
                }
                else
                {
                    MessageBox.Show("Failed to retrieve label URL.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // Method for creating a shipping label via ShipEngine API
        public async Task<string> CreateLabelAsync(
            string carrierId,        // Carrier ID
            string serviceCode,      // Shipping service code
            dynamic fromAddress,     // From Address object
            dynamic toAddress,       // To Address object
            double weight,           // Package weight
            double length,           // Package length
            double width,            // Package width
            double height)           // Package height
        {
            var requestData = new
            {
                shipment = new
                {
                    service_code = serviceCode,
                    carrier_id = carrierId,
                    ship_from = fromAddress,
                    ship_to = toAddress,
                    packages = new[] {
                new {
                    weight = new { value = weight, unit = "pound" },
                    dimensions = new { length, width, height, unit = "inch" }
                }
            }
                }
            };

            var json = JsonConvert.SerializeObject(requestData, Formatting.Indented);
            var response = await _httpClient.PostAsync("v1/labels",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode} - {error}");
            }

            // Get the raw label response
            var labelResponse = await response.Content.ReadAsStringAsync();

            dynamic responseObj = JsonConvert.DeserializeObject(labelResponse);
            var labelUrl = responseObj?.label_download?.pdf;

            if (labelUrl == null)
                throw new Exception("Failed to retrieve label PDF URL from response.");

            return labelUrl.ToString();
        }
        // Parse the label URL from the label creation response
        private string GetLabelUrlFromResponse(string labelResponse)
        {
            try
            {
                dynamic responseObj = JsonConvert.DeserializeObject(labelResponse);

                // Navigate to the correct path: label_download.label_pdf
                if (responseObj?.label_download?.label_pdf != null)
                {
                    return responseObj.label_download.label_pdf.ToString();
                }

                // Alternative path: Check for `label_url` directly
                if (responseObj?.label_url != null)
                {
                    return responseObj.label_url.ToString();
                }

                // If no valid URL found
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing label response: {ex.Message}");
                return null;
            }
        }

        // Method to download the PDF label
        public void DownloadLabelPdf(string labelUrl)
        {
            Thread downloadThread = new Thread(() =>
            {
                try
                {
                    // Ensure URL is valid
                    if (string.IsNullOrEmpty(labelUrl))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("Error: Label URL is null or empty.");
                        });
                        return;
                    }

                    // Perform the HTTP GET request
                    var pdfResponse = _httpClient.GetAsync(labelUrl).Result;

                    if (pdfResponse.IsSuccessStatusCode)
                    {
                        var pdfBytes = pdfResponse.Content.ReadAsByteArrayAsync().Result;
                        var pdfPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShippingLabel.pdf");
                        System.IO.File.WriteAllBytes(pdfPath, pdfBytes);

                        // Notify the UI thread
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"PDF Label saved to: {pdfPath}");
                        });
                    }
                    else
                    {
                        // Log response failure
                        var errorContent = pdfResponse.Content.ReadAsStringAsync().Result;
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Error: Failed to download PDF. Status Code: {pdfResponse.StatusCode}. Details: {errorContent}");
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Error downloading PDF: {ex.Message}");
                    });
                }
            });

            // Start the thread
            downloadThread.Start();
        }
    }
}