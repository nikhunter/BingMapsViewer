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
            new ObservableCollection<Datapoint>();

        public static MapLayer PolyLineLayer = new MapLayer();

        public MainWindow() {
            InitializeComponent();
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject),
                new FrameworkPropertyMetadata(Int32.MaxValue));
        }

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
                    PolyLineLayer.Children.Add(polyLine);
                }
            }
            Map.Children.Add(PolyLineLayer);
        }

        private void ImportBtn_OnClick(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Multiselect = true
            };

            var result = ofd.ShowDialog();

            if (result != false) {
                ImportData(ofd.FileName);
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
        private void ImportData(string file) {
            var lines = File.ReadAllLines(file);
            var date = "";
            for (var i = 0; i < lines.Length;) {
                var data = lines[i].Split(',');
                if (data[1] == "10C") {
                    var count = 1;

                    var rpm = int.Parse(data[2]);
                    var speed = 0;
                    double lat = 0;
                    double lng = 0;
                    var time = "";

                    try {
                        while (lines[i + count].Split(',')[1] != "10C") {
                            data = lines[i + count].Split(',');

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

                    if (lng != 0 && lat != 0) {
                        // TODO Make this threshold check prettier
                        if (PushPinCollection.Count > 0 &&
                            CalculateDistance(PushPinCollection[PushPinCollection.Count - 1].Location,
                                new Location(lat, lng)) > 100 &&
                            CalculateDistance(PushPinCollection[PushPinCollection.Count - 1].Location,
                                new Location(lat, lng)) < 1000) {
                            PushPinCollection.Add(
                                new Datapoint(new Location(lat, lng), date, time, speed, rpm));
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

        public static double CalculateDistance(Location previousPin, Location currentPin) {
            var firstCoordinate = new GeoCoordinate(previousPin.Longitude, previousPin.Latitude);
            var secondCoordinate = new GeoCoordinate(currentPin.Longitude, currentPin.Latitude);

            // Returns distance in meters
            return firstCoordinate.GetDistanceTo(secondCoordinate);
        }

        private void ClearBtn_OnClick(object sender, RoutedEventArgs e) {
            PushPinCollection.Clear();
            PolyLineLayer.Children.Clear();
            Map.Children.Remove(PolyLineLayer);
        }

        private void ExitBtn_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}