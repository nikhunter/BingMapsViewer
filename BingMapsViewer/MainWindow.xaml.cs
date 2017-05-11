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
                new Location(55.732627, 12.342962),
                DateTime.Now.Date.ToString("d", CultureInfo.InvariantCulture),
                DateTime.Now.ToString("hh:mm:ss", CultureInfo.InvariantCulture),
                60,
                4,
                3000
            ));
        }

        private void Pushpin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            // Get a reference to the object that was clicked.
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null) {
                var clickedSearchResult = frameworkElement.DataContext;

                // Do something with it.
            }
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