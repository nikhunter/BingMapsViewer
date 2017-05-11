using System;
using Microsoft.Maps.MapControl.WPF;

namespace BingMapsViewer {
    public class Datapoint {
        public Datapoint(Location location, DateTime time, int speed, int gear, int rpm) {
            Location = location;
            Time = time;
            Speed = speed;
            Gear = gear;
            Rpm = rpm;
        }

        private Location Location { get; set; }
        private DateTime Time { get; set; }
        private int Speed { get; set; }
        private int Gear { get; set; }
        private int Rpm { get; set; }
    }
}