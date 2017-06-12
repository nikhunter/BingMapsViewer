using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Device.Location;
using System.Globalization;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace BingMapsViewer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public static ObservableCollection<Datapoint> PushPinCollection { get; set; } =
            new ObservableCollection<Datapoint>(); // Collection of Map  pins, uses custom Datapoint class as type

        public static MapLayer PolyLineLayer = new MapLayer(); // Layer used only for MapPolyLines for easier cleaning

        private const string ConnectionString = "Server=raven-gps.com;Database=raven;Uid=root;Pwd=Raven123";

        public MainWindow() {
            InitializeComponent();
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject),
                new FrameworkPropertyMetadata(Int32.MaxValue)); // Sets ToolTip duration to the max value of a long
        }

        // Draws lines on the map between all pins in the PushPinCollection
        private void DrawLines() {
            if (PushPinCollection.Count > 0) {
                for (var i = 1; i < PushPinCollection.Count; i++) {
                    var polyLine = new MapPolyline();
                    var colourBrush = new SolidColorBrush {Color = Color.FromRgb(232, 123, 45)};
                    polyLine.Stroke = colourBrush;
                    polyLine.StrokeThickness = 3;
                    polyLine.Opacity = 0.8;

                    polyLine.Locations = new LocationCollection {
                        PushPinCollection[i - 1].Location,
                        PushPinCollection[i].Location
                    };
                    PolyLineLayer.Children.Add(polyLine); // Adds a new line to the layer
                }
            }
            Map.Children.Add(PolyLineLayer); // Adds the PolyLineLayer to the main Map as a child
        }

        private void ImportFromSqlBtn_OnClick(object sender, RoutedEventArgs e) {
            int parsedValue;
            if (!int.TryParse(SqlIdBox.Text, out parsedValue)) {
                MessageBox.Show("Invalid number in text field");
                return;
            }
            ImportJson(SqlIdBox.Text);
            ImportFromSqlBtn.IsEnabled = false;
        }

        private void ImportJson(string id) {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            DataTable dt = new DataTable();
            connection.Open();

            try {
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM trips WHERE id='{id}'";

                // TODO Make this into a separate method
                using (MySqlDataReader dr = command.ExecuteReader()) {
                    dt.Load(dr);
                    foreach (DataRow row in dt.Rows) {
                        try {
                            var logs = row["log_file"].ToString();

                            // TODO Fetch coordinates of first and last 'result'
                            List<RavenObject> results = JsonConvert.DeserializeObject<List<RavenObject>>(logs);

                            var lineCount = 1;
                            foreach (var line in results) {
                                var lat = double.Parse(line.Latitude, CultureInfo.InvariantCulture);
                                var lng = double.Parse(line.Longitude, CultureInfo.InvariantCulture);
                                var date = line.Date;
                                var time = line.Time;
                                var speed = int.Parse(line.Speed);
                                var rpm = int.Parse(line.Rpm);

                                if (PushPinCollection.Count > 0 &&
                                    CalculateDistance(PushPinCollection[PushPinCollection.Count - 1].Location,
                                        new Location(lat, lng)) > 100 &&
                                    CalculateDistance(PushPinCollection[PushPinCollection.Count - 1].Location,
                                        new Location(lat, lng)) < 1000) {
                                    PushPinCollection.Add(new Datapoint(new Location(lat, lng), date, time, speed,
                                        rpm));
                                }
                                else if (PushPinCollection.Count == 0) {
                                    PushPinCollection.Add(
                                        new Datapoint(new Location(lat, lng), date, time, speed, rpm));
                                }
                                lineCount = lineCount + 1;
                            }
                        }
                        catch {
                            // ignored
                        }
                    }
                }
            }
            catch {
                // ignored
            }
            DrawLines();
        }

        // Opens a file dialog where you can select a CSV file
        private void ImportFromFileBtn_OnClick(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog {
                Filter = "Supported Files (*.csv, *.json)|*.csv;*.json|All Files (*.*)|*.*"
            };

            var result = ofd.ShowDialog();

            if (result == false) return;
            // If file is selected, start data import and disable ImportBtn
            ImportData(ofd.FileName);
            ImportFromFileBtn.IsEnabled = false;
        }

        /*
            PID Codes
            Rpm     =   10C
            Speed   =   10D
            Lat     =   A
            Lng     =   B
            Time    =   10
            Date    =   11
        */
        private void ImportData(string file) {
            switch (Path.GetExtension(file)) {
                case ".csv": {
                    // load each file line into array as element
                    var lines = File.ReadAllLines(file);
                    var date = "";
                    for (var i = 0; i < lines.Length;) {
                        // splits line into array, datatype on [1], value on [2]
                        var data = lines[i].Split(',');
                        if (data[1] == "10C") {
                            // Count used for running through next lines until the next 0x10c
                            var count = 1;

                            // Initialization of variables used for pin information
                            var rpm = int.Parse(data[2]);
                            var speed = 0;
                            double lat = 0;
                            double lng = 0;
                            var time = "";

                            try {
                                // Check if file starts by logging RPM (0x10c)
                                while (lines[i + count].Split(',')[1] != "10C") {
                                    data = lines[i + count].Split(',');

                                    // Fill previously declared variables with corresponding data
                                    switch (data[1]) {
                                        case "10D":
                                            speed = int.Parse(data[2]);
                                            break;
                                        case "A":
                                            lat = double.Parse(Convert
                                                .ToDecimal(data[2].Insert(2, "."), new CultureInfo("en-US"))
                                                .ToString());
                                            break;
                                        case "B":
                                            lng = double.Parse(Convert
                                                .ToDecimal(data[2].Insert(2, "."), new CultureInfo("en-US"))
                                                .ToString());
                                            break;
                                        case "10": // TODO Make this Time extract prettier
                                            var hour = int.Parse(data[2].Substring(0, 2));
                                            var minute = int.Parse(data[2].Substring(2, 2));
                                            var second = int.Parse(data[2].Substring(4, 2));
                                            hour = hour + 2; // TimeZone correction

                                            time = $"{hour:D2}:{minute:D2}:{second:D2}";
                                            break;
                                        case "11": // TODO Make this Date extract prettier
                                            var day = data[2].Substring(0, 2);
                                            var month = data[2].Substring(2, 2);
                                            var year = data[2].Substring(4, 2);

                                            date = $"{day}/{month}/{year}";
                                            break;
                                    }

                                    count++;
                                }
                            }
                            catch (IndexOutOfRangeException) {
                                // import is probably at the end of the file, so we escape the loop here
                                break;
                            }

                            // If dataset doesn't have gps coordinates, discard it
                            if (lng != 0 && lat != 0) {
                                // TODO Make this threshold check prettier

                                // if distance is greater than 100 meter and under 1000, set a pin, otherwise it's too close or a GPS glitch
                                // Magic numbers for now but, easily could be replaced with actual variables
                                if (PushPinCollection.Count > 0 &&
                                    CalculateDistance(PushPinCollection[PushPinCollection.Count - 1].Location,
                                        new Location(lat, lng)) > 100 &&
                                    CalculateDistance(PushPinCollection[PushPinCollection.Count - 1].Location,
                                        new Location(lat, lng)) < 1000) {
                                    PushPinCollection.Add(new Datapoint(new Location(lat, lng), date, time, speed,
                                        rpm));
                                }
                                else if (PushPinCollection.Count == 0) {
                                    PushPinCollection.Add(
                                        new Datapoint(new Location(lat, lng), date, time, speed, rpm));
                                }
                            }

                            i += count;
                        }
                    }
                    DrawLines();
                }
                    break;
                case ".json": {
                    // TODO Fetch coordinates of first and last 'result'
                    List<RavenObject> results =
                        JsonConvert.DeserializeObject<List<RavenObject>>(File.ReadAllText(file));

                    var lineCount = 1;
                    foreach (var index in results) {
                        var lat = double.Parse(index.Latitude, CultureInfo.InvariantCulture);
                        var lng = double.Parse(index.Longitude, CultureInfo.InvariantCulture);
                        var date = index.Date;
                        var time = index.Time;
                        var speed = int.Parse(index.Speed);
                        var rpm = int.Parse(index.Rpm);

                        if (PushPinCollection.Count > 0 &&
                            CalculateDistance(PushPinCollection[PushPinCollection.Count - 1].Location,
                                new Location(lat, lng)) > 50) {
                            PushPinCollection.Add(new Datapoint(new Location(lat, lng), date, time, speed,
                                rpm));
                        }
                        else if (PushPinCollection.Count == 0) {
                            PushPinCollection.Add(
                                new Datapoint(new Location(lat, lng), date, time, speed, rpm));
                        }
                        lineCount = lineCount + 1;
                    }
                    DrawLines();
                }
                    break;
            }
        }

        public class RavenObject {
            [JsonProperty(PropertyName = "DeltaTime")]
            public string DeltaTime { get; set; }

            [JsonProperty(PropertyName = "Rpm")]
            public string Rpm { get; set; }

            [JsonProperty(PropertyName = "Speed")]
            public string Speed { get; set; }

            [JsonProperty(PropertyName = "Lat")]
            public string Latitude { get; set; }

            [JsonProperty(PropertyName = "Lng")]
            public string Longitude { get; set; }

            [JsonProperty(PropertyName = "Date")]
            public string Date { get; set; }

            [JsonProperty(PropertyName = "Time")]
            public string Time { get; set; }
        }

        // Returns the distance (in metric meters) between two locations
        public static double CalculateDistance(Location previousPin, Location currentPin) {
            try {
                var firstCoordinate = new GeoCoordinate(previousPin.Longitude, previousPin.Latitude);
                var secondCoordinate = new GeoCoordinate(currentPin.Longitude, currentPin.Latitude);

                // Returns distance in meters
                return firstCoordinate.GetDistanceTo(secondCoordinate);
            }
            catch (Exception) {
                return 0; // If returned 0, aka under 100 meters it will not add the pin because something went wrong
            }
        }

        // Clears the pin collection, clears the PolyLineLayer childrens(Map lines) and removes the PolyLineLayer as a child of the main Map
        private void ClearBtn_OnClick(object sender, RoutedEventArgs e) {
            PushPinCollection.Clear();
            PolyLineLayer.Children.Clear();
            Map.Children.Remove(PolyLineLayer);

            // Renables the import buttons because now the user may import data again
            ImportFromFileBtn.IsEnabled = true;
            ImportFromSqlBtn.IsEnabled = true;
        }

        // Closes the program
        private void ExitBtn_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}