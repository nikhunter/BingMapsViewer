using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Win32;

namespace BingMapsViewer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public static ObservableCollection<Datapoint> PushPinCollection { get; set; } =
            new ObservableCollection<Datapoint>();

        public MainWindow() {
            InitializeComponent();
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject),
                new FrameworkPropertyMetadata(Int32.MaxValue));

            for (int count = 0; count < PushPinCollection.Count - 1; count++) {
                DrawLine(PushPinCollection[count].Location, PushPinCollection[count + 1].Location);
            }
        }

        private void DrawLine(Location oldPin, Location newPin) {
            MapPolyline polyLine = new MapPolyline();
            SolidColorBrush colourBrush = new SolidColorBrush();
            colourBrush.Color = Color.FromRgb(232, 123, 45);
            polyLine.Stroke = colourBrush;
            polyLine.StrokeThickness = 3;
            polyLine.Opacity = 0.8;
            polyLine.Locations = new LocationCollection {
                oldPin,
                newPin
            };

            Map.Children.Add(polyLine);
        }

        /*
        #define PID_GPS_LATITUDE 0xA
        #define PID_GPS_LONGITUDE 0xB
        #define PID_GPS_ALTITUDE 0xC
        #define PID_GPS_SPEED 0xD
        #define PID_GPS_HEADING 0xE
        #define PID_GPS_SAT_COUNT 0xF
        #define PID_GPS_TIME 0x10
        #define PID_GPS_DATE 0x11
        */

        private void ImportBtn_OnClick(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            var result = ofd.ShowDialog();

            if (result != false) {
                ImportData(ofd.FileName, 20);
            }
        }

        public class Data {
            // TODO Make Date and Time variables
            public static string Rpm => "10C";
            public static string Speed => "10D";
            public static string Lat => "A";
            public static string Lng => "B";
        }

        private void ImportData(string file, int skip) {
            var lines = File.ReadAllLines(file);
            var skipCount = 0; // TODO Replace with something that checks that current extract is more then X, X(lat, lng)+% away
            for (var i = 0; i <= lines.Length;) {
                var data = lines[i].Split(',');
                if (data[1] == Data.Rpm) {
                    var count = 1;

                    // TODO Make Date and Time variables
                    var rpm = int.Parse(data[2]);
                    var speed = 0;
                    double lat = 0;
                    double lng = 0;

                    try {
                        // TODO Extract Date and Time
                        while (lines[i + count].Split(',')[1] != Data.Rpm) {
                            data = lines[i + count].Split(',');

                            if (data[1] == Data.Speed) {
                                speed = int.Parse(data[2]);
                            }
                            else if (data[1] == Data.Lat) {
                                lat = double.Parse(data[2].Insert(2, ","));
                            }
                            else if (data[1] == Data.Lng) {
                                lng = double.Parse(data[2].Insert(2, ","));
                            }

                            count++;
                        }
                    }
                    catch (IndexOutOfRangeException) {
                        // import is probably at the end of the file, so we escape the loop here
                        break;
                    }

                    if (lng != 0 && lat != 0) {
                        if (skipCount == 0) {
                            if (PushPinCollection.Count > 0) {
                                DrawLine(PushPinCollection[PushPinCollection.Count - 1].Location,
                                    new Location(lat, lng));
                            }

                            // TODO Check that Latitude and Longitude is within a certain distance from the previous pin
                            PushPinCollection.Add(
                                new Datapoint(new Location(lat, lng), "TEST", "TEST", speed, rpm));

                            skipCount++;
                        }
                        else if (skipCount >= skip) {
                            skipCount = 0;
                        }
                        else {
                            skipCount++;
                        }
                    }

                    i += count;
                }
            }
        }

        private void ClearBtn_OnClick(object sender, RoutedEventArgs e) {
            // TODO Clear MapPolyLine from Map (Map.Children.Clear breaks PushPinCollection binding)
            PushPinCollection.Clear();
        }

        private void ExitBtn_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}