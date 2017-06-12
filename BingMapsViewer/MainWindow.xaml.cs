using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Device.Location;
using System.Globalization;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Win32;

namespace BingMapsViewer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public static ObservableCollection<Datapoint> PushPinCollection { get; set; } =
            new ObservableCollection<Datapoint>(); // Collection of Map  pins, uses custom Datapoint class as type

        public static MapLayer PolyLineLayer = new MapLayer(); // Layer used only for MapPolyLines for easier cleaning

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
        
        // Opens a file dialog where you can select a CSV file
        private void ImportBtn_OnClick(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            };

            var result = ofd.ShowDialog();

            if (result != false) { // If file is selected, start data import and disable ImportBtn
                ImportData(ofd.FileName);
                ImportBtn.IsEnabled = false;
            }
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
        private void ImportData(string file)
        {
            // load each file line into array as element
            var lines = File.ReadAllLines(file);
            var date = "";
            for (var i = 0; i < lines.Length;)
            {
                // splits line into array, datatype on [1], value on [2]
                var data = lines[i].Split(',');
                if (data[1] == "10C")
                {
                    // Count used for running through next lines until the next 0x10c
                    var count = 1;

                    // Initialization of variables used for pin information
                    var rpm = int.Parse(data[2]);
                    var speed = 0;
                    double lat = 0;
                    double lng = 0;
                    var time = "";


                    try
                    {
                        // Check if file starts by logging RPM (0x10c)
                        while (lines[i + count].Split(',')[1] != "10C")
                        {
                            data = lines[i + count].Split(',');

                            // Fill previously declared variables with corresponding data
                            switch (data[1])
                            {
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
                                    try {
                                        var hour = int.Parse(data[2].Substring(0, 2));
                                        var minute = int.Parse(data[2].Substring(2, 2));
                                        var second = int.Parse(data[2].Substring(4, 2));
                                        hour = hour + 2; // TimeZone correction

                                        time = $"{hour:D2}:{minute:D2}:{second:D2}";
                                    }
                                    catch (Exception) {
                                        // ignored
                                    }
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
                    catch (IndexOutOfRangeException)
                    {
                        // import is probably at the end of the file, so we escape the loop here
                        break;
                    }

                    // If dataset doesn't have gps coordinates, discard it
                    if (lng != 0 && lat != 0)
                    {
                        // TODO Make this threshold check prettier

                        // if distance is greater than 10 meter and under 1000, set a pin, otherwise it's too close or a GPS glitch
                        // Magic numbers for now but, easily could be replaced with actual variables
                        if (PushPinCollection.Count > 0 &&
                            CalculateDistance(PushPinCollection[PushPinCollection.Count - 1].Location,
                                new Location(lat, lng)) > 100 &&
                            CalculateDistance(PushPinCollection[PushPinCollection.Count - 1].Location,
                                new Location(lat, lng)) < 1000)
                        {
                            PushPinCollection.Add(new Datapoint(new Location(lat, lng), date, time, speed, rpm));
                        }
                        else if (PushPinCollection.Count == 0)
                        {
                            PushPinCollection.Add(
                                new Datapoint(new Location(lat, lng), date, time, speed, rpm));
                        }
                    }

                    i += count;
                }
            }
            DrawLines();
        }

        // Returns the distance (in metric meters) between two locations
        public static double CalculateDistance(Location previousPin, Location currentPin) {
            var firstCoordinate = new GeoCoordinate(previousPin.Longitude, previousPin.Latitude);
            var secondCoordinate = new GeoCoordinate(currentPin.Longitude, currentPin.Latitude);

            // Returns distance in meters
            return firstCoordinate.GetDistanceTo(secondCoordinate);
        }

        // Clears the pin collection, clears the PolyLineLayer childrens(Map lines) and removes the PolyLineLayer as a child of the main Map
        private void ClearBtn_OnClick(object sender, RoutedEventArgs e) {
            PushPinCollection.Clear();
            PolyLineLayer.Children.Clear();
            Map.Children.Remove(PolyLineLayer);

            ImportBtn.IsEnabled = true; // Renables the ImportBtn because now the user may import data again
        }

        // Closes the program
        private void ExitBtn_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}