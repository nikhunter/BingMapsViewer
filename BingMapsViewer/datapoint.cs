using System;
using Microsoft.Maps.MapControl.WPF;

namespace BingMapsViewer {
    public class Datapoint {
        public Location Location { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int Speed { get; set; }
        public int Gear { get; set; }
        public int Rpm { get; set; }

        public Datapoint(Location location, string date, string time, int speed, int gear, int rpm) {
            Location = location;
            Date = date;
            Time = time;
            Speed = speed;
            Gear = gear;
            Rpm = rpm;
        }
    }
}