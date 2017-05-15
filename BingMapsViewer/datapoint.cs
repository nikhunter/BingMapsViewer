using Microsoft.Maps.MapControl.WPF;

namespace BingMapsViewer {
    public class Datapoint {
        public Location Location { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int Speed { get; set; }
        public int Rpm { get; set; }
        public MapPolyline PolyLine { get; set; }

        public Datapoint(Location location, string date, string time, int speed, int rpm) {
            Location = location;
            Date = date;
            Time = time;
            Speed = speed;
            Rpm = rpm;
        }

        public Datapoint(Location location, string date, string time, int speed, int rpm, MapPolyline polyLine)
        {
            Location = location;
            Date = date;
            Time = time;
            Speed = speed;
            Rpm = rpm;
            PolyLine = polyLine;
        }
    }
}