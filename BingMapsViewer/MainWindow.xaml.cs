using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

        private void ImportBtn_OnClick(object sender, RoutedEventArgs e) {
            // ignored
        }

        private void ClearBtn_OnClick(object sender, RoutedEventArgs e) {
            PushPinCollection.Clear();
        }

        private void ExitBtn_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}