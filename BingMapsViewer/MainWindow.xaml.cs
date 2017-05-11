using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace BingMapsViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Datapoint> PushPinCollection = new ObservableCollection<Datapoint>();

        public MainWindow()
        {
            InitializeComponent();

            PushPinCollection.Add(new Datapoint(
                new Location(55.732627, 12.342962),
                DateTime.Now,
                60,
                4,
                3000
                ));
        }
    }
}
