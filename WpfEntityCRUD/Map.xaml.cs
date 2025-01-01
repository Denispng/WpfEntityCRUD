using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;

namespace WpfEntityCRUD
{
    public partial class Map : Page
    {
        public ObservableCollection<Warehouse> Warehouses { get; set; }
        private Dictionary<int, Rectangle> cellMap = new Dictionary<int, Rectangle>();

        private string connectionString = "Server=tcp:stocksolve.database.windows.net,1433;Initial Catalog=wpfCrud;Persist Security Info=False;User ID=Denis;Password=Denvlad1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        wpfCrudEntities1 _db = new wpfCrudEntities1();
        public Map()
        {
            InitializeComponent();
            RenderAllAisles();
            this.Loaded += MainWindow_Loaded;
            LoadWarehouses();
        }

        
        public static readonly DependencyProperty CellMetadataProperty = DependencyProperty.RegisterAttached(
    "CellMetadata", typeof(object), typeof(Map), new PropertyMetadata(null));

        private Point lastMousePosition;
        private bool isRightMousePressed = false;

        // Mouse down event
        private ModelVisual3D lightVisual; // Declare lightVisual as a class-level variable

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            WarehouseViewport.Children.Clear();

            try
            {
                // Validate ComboBox selection
                if (warehouseComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Please select a warehouse.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Attempt to parse the selected value to an integer
                if (int.TryParse(warehouseComboBox.SelectedValue.ToString(), out int selectedWarehouseId))
                {
                    // Proceed with the selectedWarehouseId
                    var selectedWarehouse = _db.Warehouses.FirstOrDefault(w => w.warehousesId == selectedWarehouseId);
                    if (selectedWarehouse != null)
                    {
                        aisleRenderIndex = 0;
                        RenderAllAisles();
                    }
                    else
                    {
                        MessageBox.Show("Selected warehouse not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid warehouse selection.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        
          
                DirectionalLight directionalLight = new DirectionalLight
                {
                    Color = Colors.White,
                    Direction = new Vector3D(-1, -1, -1) // Light direction (adjust as needed)
                };

                // Create a ModelVisual3D to hold the light
                lightVisual = new ModelVisual3D
                {
                    Content = directionalLight
                };

                // Add the light to the viewport
                WarehouseViewport.Children.Add(lightVisual);
            }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWarehouses();
        }
        private void LoadWarehouses()
        {
            try
            {
                var companyId = LoginWindow.LoggedInUser.company_id;
                var warehouses = _db.Warehouses
                                    .Where(w => w.company_id == companyId)
                                    .ToList();

                warehouseComboBox.ItemsSource = warehouses;
                warehouseComboBox.DisplayMemberPath = "Name";           // Property to display
                warehouseComboBox.SelectedValuePath = "warehousesId";   // Property for selected value
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                isRightMousePressed = true;
                lastMousePosition = e.GetPosition(this);
                Mouse.Capture(WarehouseViewport);
            }
        }




        // Mouse move event
        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (isRightMousePressed)
            {
                Point currentMousePosition = e.GetPosition(this);
                double deltaX = currentMousePosition.X - lastMousePosition.X;
                double deltaY = currentMousePosition.Y - lastMousePosition.Y;

                // Rotate the camera
                RotateCamera(deltaX, deltaY);

                lastMousePosition = currentMousePosition;
            }
        }
   
        // Mouse up event
        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                isRightMousePressed = false;
                Mouse.Capture(null);
            }
        }

        // Mouse wheel event (zooming)
        private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomCamera(e.Delta);
        }

        // Rotate the camera around a point
        private void RotateCamera(double deltaX, double deltaY)
        {
            const double rotationSpeed = 0.2;

            // Get current camera settings
            var camera = MainCamera;
            var position = camera.Position;

            // Rotate horizontally
            double angleY = deltaX * rotationSpeed;
            double angleX = deltaY * rotationSpeed;

            // Use simple trigonometry for rotation
            double radius = Math.Sqrt(position.X * position.X + position.Z * position.Z);
            double theta = Math.Atan2(position.Z, position.X);

            theta -= angleX * Math.PI / 180;

            double newX = radius * Math.Cos(theta);
            double newZ = radius * Math.Sin(theta);

            camera.Position = new Point3D(newX, position.Y - angleY, newZ);
            camera.LookDirection = new Vector3D(-newX, -position.Y, -newZ);
        }
      private void DeleteAsiel_Click(object sender, RoutedEventArgs e)
{

    GetSelectedAisleId aisleSelectionDialog = new GetSelectedAisleId();
            try { 
    
    aisleSelectionDialog.AisleDeleted += ReloadAisles;
                aisleRenderIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            aisleSelectionDialog.ShowDialog();

}


        

        private void InitializeLight()
        {
            if (lightVisual == null) // Only create the light if it hasn't been created yet
            {
                // Create a DirectionalLight
                DirectionalLight directionalLight = new DirectionalLight
                {
                    Color = Colors.White,
                    Direction = new Vector3D(-1, -1, -1) // Light direction (adjust as needed)
                };

                // Create a ModelVisual3D to hold the light
                lightVisual = new ModelVisual3D
                {
                    Content = directionalLight
                };
            }
        }

        private void ReloadAisles()
        {
     
            WarehouseViewport.Children.Clear();

            
            RenderAllAisles();

            // Ensure the light is initialized
            InitializeLight();


            // Add the light to the viewport
            if (lightVisual != null) // Check to make sure the light is initialized
            {
                WarehouseViewport.Children.Add(lightVisual);
            }
            else
            {
                MessageBox.Show("Light could not be initialized.");
            }

        }
        private void DeleteAisleFromDatabase(int aisleId)
        {
            string query = @"
    DELETE FROM Aisles WHERE AisleID = @AisleID;
    ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add the aisleId parameter to the query
                    command.Parameters.AddWithValue("@AisleID", aisleId);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Aisle deleted successfully.");
                            // Optionally, refresh your aisle list or UI
                        }
                        else
                        {
                            MessageBox.Show("Aisle not found or could not be deleted.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting aisle: " + ex.Message);
                    }
                }
            }
        }

        // Zoom the camera
        private void ZoomCamera(int delta)
        {
            const double zoomSpeed = 20.0;

            var camera = MainCamera;

            // Move the camera closer or further along its LookDirection
            Vector3D lookDirection = camera.LookDirection;
            lookDirection.Normalize();

            double zoomFactor = delta > 0 ? zoomSpeed : -zoomSpeed;

            camera.Position = new Point3D(
                camera.Position.X + lookDirection.X * zoomFactor,
                camera.Position.Y + lookDirection.Y * zoomFactor,
                camera.Position.Z + lookDirection.Z * zoomFactor
            );
        }
        public static void SetCellMetadata(DependencyObject element, object value)
        {
            element.SetValue(CellMetadataProperty, value);
        }

        public static object GetCellMetadata(DependencyObject element)
        {
            return element.GetValue(CellMetadataProperty);
        }
        private List<Aisle> GetAislesFromDatabase()
        {
            List<Aisle> aisles = new List<Aisle>();

            string query = "SELECT AisleID, Name, RowCount, LayerCount, CellCount FROM Aisles";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            aisles.Add(new Aisle
                            {
                                AisleID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                LayerCount = reader.GetInt32(2),
                                CellCount = reader.GetInt32(3),
                                RowCount = reader.GetInt32(4)
                            });
                        }
                    }
                }
            }

            return aisles;
        }
        private void FindAndHighlightMemberCells_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MemberIdInputDialog();
            if (dialog.ShowDialog() == true)
            {
                int memberId = dialog.MemberId;
                Debug.WriteLine($"Input MemberId: {memberId}");

                List<int> cellIds = GetCellIdsForMember(memberId);
                Debug.WriteLine($"Retrieved CellIDs: {string.Join(", ", cellIds)}");

                foreach (ModelVisual3D visual3D in WarehouseViewport.Children)
                {
                    if (visual3D.Content is GeometryModel3D cube)
                    {
                        var metadata = cube.GetValue(TagProperty) as CellMetadata;
                        Debug.WriteLine($"Processing Cube - Metadata MemberId: {metadata?.MemberId}");

                        if (metadata != null && cellIds.Contains(metadata.CellID))
                        {
                            Debug.WriteLine("Match found. Highlighting cube...");
                            cube.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                        }
                        else
                        {
                            Debug.WriteLine("No match. Skipping cube...");
                        }
                    }
                }
            }
        }

        private List<int> GetCellIdsForMember(int memberId)
        {
            List<int> cellIds = new List<int>();
            string query = "SELECT CellID FROM Cells WHERE Id = @Id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", memberId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cellIds.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            return cellIds;
        }

        private void AddAisle_Click(object sender, RoutedEventArgs e)
        {
           
            int loggedInCompanyId = LoginWindow.LoggedInUser.company_id ?? 0;  // Renamed to avoid conflict
            int selectedWarehouseId = (int)warehouseComboBox.SelectedValue;
            if (selectedWarehouseId == 0) 
            {
                MessageBox.Show("Please select a warehouse.");
                return;
            }

           
            var dialog = new AisleDialog(loggedInCompanyId); 
            if (dialog.ShowDialog() == true)
            {
                // Save the aisle with the company_id
                int aisleId = SaveAisleToDatabase(dialog.AisleName, dialog.LayerCount, dialog.RowCount, dialog.CellCount, dialog.CompanyId, selectedWarehouseId);



                RenderAisle(aisleId, dialog.LayerCount, dialog.CellCount, dialog.RowCount, dialog.CompanyId);
            }
        }
        private void RenderAllAisles()
        {
            WarehouseViewport.Children.Clear();
            var loggedInCompanyId = LoginWindow.LoggedInUser.company_id ?? 0;

            if (warehouseComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a warehouse.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int warehouseId;
            if (int.TryParse(warehouseComboBox.SelectedValue.ToString(), out warehouseId))
            {
                // Use Entity Framework to get aisles for the specific company and warehouse
                using (var _db = new wpfCrudEntities1())
                {
                    // Filter aisles based on company_id and warehouseId
                    List<Aisle> aisles = _db.Aisles
                                             .Where(a => a.company_id == loggedInCompanyId && a.warehousesId == warehouseId) // Filter aisles by company_id and warehouseId
                                             .ToList();

                    // Loop through the aisles and render each one
                    foreach (var aisle in aisles)
                    {
                        RenderAisle(aisle.AisleID, aisle.LayerCount ?? 1, aisle.CellCount ?? 1, aisle.RowCount ?? 1, loggedInCompanyId);
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid warehouse selection.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ClearAislesFromDatabase()
        {
            var loggedInCompanyId = LoginWindow.LoggedInUser.company_id; // Retrieve company_id from logged-in user
            if (loggedInCompanyId == null)
            {
                MessageBox.Show("No company ID found for the logged-in user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Assuming you have a ComboBox or control to select the warehouseId
            int selectedWarehouseId = (int)warehouseComboBox.SelectedValue; // Get selected warehouseId

            if (selectedWarehouseId == 0) // If no warehouse is selected
            {
                MessageBox.Show("Please select a warehouse.");
                return;
            }

            // Show confirmation dialog
            var result = MessageBox.Show("Are you sure you want to delete all aisles and associated cells for your company and selected warehouse?",
                                         "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            // If the user confirms (clicks Yes)
            if (result == MessageBoxResult.Yes)
            {
                string query = @"
    -- Delete cells associated with aisles for the specific company_id and warehouse_id
    DELETE FROM Cells WHERE AisleID IN (SELECT AisleID FROM Aisles WHERE company_id = @CompanyId AND warehousesId = @WarehouseId);

    -- Now delete aisles for the specific company_id and warehouse_id
    DELETE FROM Aisles WHERE company_id = @CompanyId AND warehousesId = @WarehouseId;
";

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            // Add the company_id and warehouseId parameters to the SQL command
                            command.Parameters.AddWithValue("@CompanyId", loggedInCompanyId);
                            command.Parameters.AddWithValue("@WarehouseId", selectedWarehouseId);

                            connection.Open();
                            int rowsAffected = command.ExecuteNonQuery();

                            // Show appropriate message based on whether rows were affected
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("All aisles and associated cells for the selected company and warehouse have been deleted.",
                                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("No aisles found for the selected company and warehouse, or they have already been deleted.",
                                                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Handle SQL exceptions
                    MessageBox.Show($"An error occurred while clearing aisles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    // Handle general exceptions
                    MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // If the user clicks 'No', we simply do nothing
                MessageBox.Show("Aisle deletion has been cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void ClearAislesButton_Click(object sender, RoutedEventArgs e)
        {
            // Call the method to clear aisles from the database
            ClearAislesFromDatabase();
        }

        private int aisleRenderIndex = 0;
        private void RenderAisle(int aisleId, int layerCount, int cellCount, int rowCount, int companyId)
        {
            int defaultMemberId = 1; // Replace with a valid MemberId from your database

            double cellWidth = 10;   // Width of each cube
            double cellHeight = 10;  // Height of each cube
            double cellDepth = 10;   // Depth of each cube
            double aisleSpacing = 50; // Space between aisles

            // Initialize startX position
            double startX = aisleRenderIndex * (30 + aisleSpacing) - 200;
            aisleRenderIndex++;

            Debug.WriteLine($"Rendering Aisle {aisleId} at X={startX}");

            // Use Entity Framework to get relevant data
            using (var _db = new wpfCrudEntities1())
            {
                // Filter members based on company_id and exclude Id = 1
                var filteredMembers = _db.members
                                          .Where(m => m.company_id == companyId && m.Id != 8)
                                          .ToList();

                // Loop through the layers (Z axis)
                for (int z = 0; z < layerCount; z++)
                {
                    // Loop through rows (Y axis)
                    for (int y = 0; y < rowCount; y++)
                    {
                        // Loop through cells (X axis)
                        for (int x = 0; x < cellCount; x++)
                        {
                            // Position for the current cube
                            Point3D position = new Point3D(startX + x * cellWidth, y * cellHeight, -z * cellDepth);

                            // Create a 3D cube (GeometryModel3D)
                            GeometryModel3D cube = CreateCube(position, cellWidth, cellHeight, cellDepth, Colors.LightGray);

                            // Optional: Save to database
                            int cellId = SaveCellToDatabase(aisleId, x, y, z, defaultMemberId, companyId);

                            // Attach metadata via a visual model
                            cube.SetValue(TagProperty, new CellMetadata
                            {
                                AisleID = aisleId,
                                X = x,
                                Y = y,
                                Z = z,
                                CellID = cellId,
                                company_id = companyId // Use the passed-in companyId directly
                            });

                            // Add the cube to the viewport
                            var visual3D = new ModelVisual3D { Content = cube };
                            WarehouseViewport.Children.Add(visual3D);
                        }
                    }
                }
            }
        }
        private GeometryModel3D CreateCube(Point3D position, double width, double height, double depth, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            // Define vertices for the cube
            Point3D[] vertices = new Point3D[]
            {
        // Front face
        new Point3D(position.X, position.Y, position.Z),
        new Point3D(position.X + width, position.Y, position.Z),
        new Point3D(position.X + width, position.Y + height, position.Z),
        new Point3D(position.X, position.Y + height, position.Z),

        // Back face
        new Point3D(position.X, position.Y, position.Z - depth),
        new Point3D(position.X + width, position.Y, position.Z - depth),
        new Point3D(position.X + width, position.Y + height, position.Z - depth),
        new Point3D(position.X, position.Y + height, position.Z - depth)
            };

            // Define triangles for the cube faces
            int[] indices = new int[]
            {
        0, 1, 2, 2, 3, 0, // Front
        1, 5, 6, 6, 2, 1, // Right
        5, 4, 7, 7, 6, 5, // Back
        4, 0, 3, 3, 7, 4, // Left
        3, 2, 6, 6, 7, 3, // Top
        4, 5, 1, 1, 0, 4  // Bottom
            };


            mesh.Positions = new Point3DCollection(vertices);
            mesh.TriangleIndices = new Int32Collection(indices);

            // Apply material and color
            Material material = new DiffuseMaterial(new SolidColorBrush(Colors.NavajoWhite));
            return new GeometryModel3D(mesh, material);
        }
        private int SaveAisleToDatabase(string aisleName, int rowCount, int layerCount, int cellCount, int companyId, int warehouseId)
        {
            string query = @"
INSERT INTO Aisles (Name, LayerCount, CellCount, [RowCount], company_id, warehousesId)
VALUES (@AisleName, @LayerCount, @CellCount, @AisleRowCount, @CompanyId, @WarehouseId);
SELECT SCOPE_IDENTITY();";


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AisleName", aisleName);
                    command.Parameters.AddWithValue("@LayerCount", rowCount);
                    command.Parameters.AddWithValue("@CellCount", cellCount);
                    command.Parameters.AddWithValue("@AisleRowCount", layerCount);
                    command.Parameters.AddWithValue("@CompanyId", companyId);
                    command.Parameters.AddWithValue("@WarehouseId", warehouseId);

                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        private int SaveCellToDatabase(int aisleId, int x, int y, int z, int memberId, int companyId)
        {
            memberId = 8;
            string query = @"
IF EXISTS (SELECT 1 FROM Cells WHERE AisleID = @AisleID AND XCoordinate = @X AND YCoordinate = @Y AND ZCoordinate = @Z AND company_id = @CompanyID)
BEGIN
    -- If the cell exists, just return the existing CellID
    SELECT CellID FROM Cells WHERE AisleID = @AisleID AND XCoordinate = @X AND YCoordinate = @Y AND ZCoordinate = @Z AND company_id = @CompanyID
END
ELSE
BEGIN
    -- If the cell doesn't exist, insert a new cell and return the new CellID
    INSERT INTO Cells (AisleID, XCoordinate, YCoordinate, ZCoordinate, Id, company_id)
    OUTPUT INSERTED.CellID
    VALUES (@AisleID, @X, @Y, @Z, @Id, @CompanyID)
END";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AisleID", aisleId);
                    command.Parameters.AddWithValue("@X", x);
                    command.Parameters.AddWithValue("@Y", y);
                    command.Parameters.AddWithValue("@Z", z);
                    command.Parameters.AddWithValue("@Id", memberId);  // Pass a valid Member Id here
                    command.Parameters.AddWithValue("@CompanyID", companyId);  // Pass the companyId here

                    connection.Open();
                    var result = command.ExecuteScalar();

                    if (result != null && int.TryParse(result.ToString(), out int cellId))
                    {
                        return cellId; // Return the existing or newly inserted CellID
                    }
                    throw new Exception("Failed to retrieve CellID after insert.");
                }
            }
        }

        private void WarehouseViewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(WarehouseViewport);

            // Perform a hit test
            HitTestResult result = VisualTreeHelper.HitTest(WarehouseViewport, mousePosition);

            if (result != null && result.VisualHit is ModelVisual3D visual3D)
            {
                var metadata = visual3D.Content.GetValue(TagProperty) as CellMetadata;
                if (metadata != null)
                {
                    MessageBox.Show($"Cell ID: {metadata.CellID}\nAisle ID: {metadata.AisleID}\nPosition: X={metadata.X}, Y={metadata.Y}, Z={metadata.Z}");
                }
            }
        }
        // New method to handle binding cell with member data (from the "Members" table)
        private void BindCellWithMemberCode(int aisleId, int x, int y, int z)
        {
            // Show a dialog to get the member code
            var dialog = new BindMemberDialog(); // Create an instance of the dialog
            if (dialog.ShowDialog() == true) // Check if the dialog was confirmed with 'OK'
            {
                int cellId = dialog.CellID; // Get the CellID from the dialog
                string memberCode = dialog.Id; // Get the MemberCode from the dialog (now as a string)

                // Now update the database with the CellID and MemberCode
                UpdateCellBinding(cellId, memberCode); // Pass MemberCode as a string
            }
        }
        private void Button_Click(Object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Window.GetWindow(this)?.Close();
        }

        // Method to update the database with the member code
        public void UpdateCellBinding(int cellId, string newId)
        {
            // If the user wants to unbind the member, set the Id to "1" automatically
            if (newId == "unbind")  // Use a keyword to indicate unbinding (or any other flag you prefer)
            {
                newId = "1";     
                 MessageBox.Show("Unbound Successfully");
                 return;
                
            }

            // Update the CellId with the new Id (either valid or default "1")
            string query = "UPDATE Cells SET Id = @Id WHERE CellID = @CellID";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", newId);  // Use "1" if unbinding
                    command.Parameters.AddWithValue("@CellID", cellId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void BindCell_Click(object sender, RoutedEventArgs e)
    {
        // Implement binding functionality with another SQL table (Members)
        var dialog = new BindMemberDialog(); // Replace with your own dialog implementation
        if (dialog.ShowDialog() == true)
        {
            UpdateCellBinding(dialog.CellID, dialog.Id); // Pass MemberCode as string
        }
    }
     
        // Model class for cell metadata (since dynamic isn't used in C# 7.3)
        public class CellMetadata
        {
            public int AisleID { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            public int CellID { get; set; }
            public int MemberId { get; set; } 
            public int company_id { get; set; }
        }
    }
}
