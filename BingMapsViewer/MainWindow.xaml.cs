﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Maps.MapControl.WPF;
using Json;
using Microsoft.Win32;

namespace BingMapsViewer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public static ObservableCollection<Datapoint> PushPinCollection { get; set; } =
            new ObservableCollection<Datapoint>();

        public MainWindow() {
            InitializeComponent();
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject),
                new FrameworkPropertyMetadata(Int32.MaxValue));

            PushPinCollection.Add(new Datapoint(
                new Location(55.732283, 12.343685),
                DateTime.Now.Date.ToString("d", CultureInfo.InvariantCulture),
                DateTime.Now.ToString("hh:mm:ss", CultureInfo.InvariantCulture),
                0,
                0,
                800
            ));

            PushPinCollection.Add(new Datapoint(
                new Location(55.731998, 12.343775),
                DateTime.Now.Date.ToString("d", CultureInfo.InvariantCulture),
                DateTime.Now.ToString("hh:mm:ss", CultureInfo.InvariantCulture),
                30,
                2,
                2500
            ));

            PushPinCollection.Add(new Datapoint(
                new Location(55.732018, 12.344381),
                DateTime.Now.Date.ToString("d", CultureInfo.InvariantCulture),
                DateTime.Now.ToString("hh:mm:ss", CultureInfo.InvariantCulture),
                10,
                1,
                1000
            ));

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
            /*var ofd = new OpenFileDialog {
                Filter = "CSV Files (*.CSV)|All Files (*.*)"
            };

            var result = ofd.ShowDialog();

            if (result != false) {
                ImportData(ofd.FileName);
            }*/

            ImportData("C:\\Users\\Cosmo\\Downloads\\DAT00004.CSV");
        }

        class DATA {
            public static string RPM => "10C";
            public static string SPEED => "10D";
            public static string LAT => "A";
            public static string LNG => "B";
        }

        private void ImportData(string file) {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i <= lines.Length;) {
                var data = lines[i].Split(',');
                if (data[1] == DATA.RPM) {
                    var count = 1;

                    var rpm = int.Parse(data[2]);
                    var speed = 0;
                    double lat = 0;
                    double lng = 0;


                    while (lines[i + count].Split(',')[1] != DATA.RPM) {
                        data = lines[i + count].Split(',');

                        if (data[1] == DATA.SPEED) {
                            speed = int.Parse(data[2]);
                        }
                        else if (data[1] == DATA.LAT) {
                            lat = double.Parse(data[2].Insert(2, "."));
                        }
                        else if (data[1] == DATA.LNG) {
                            lng = double.Parse(data[2].Insert(2, "."));
                        }

                        count++;
                    }

                    if (lng != 0) {
                        if (lat != 0) {
                            PushPinCollection.Add(new Datapoint(new Location(lat, lng), "TEST", "TEST", speed, 0, rpm));
                            MessageBox.Show($"rpm: {rpm}\nspeed: {speed}\nlat: {lat}\nlng: {lng}");
                            break;
                        }
                    }
                }
            }
        }

        private void ClearBtn_OnClick(object sender, RoutedEventArgs e) {
            PushPinCollection.Clear();
        }

        private void ExitBtn_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}