using System;
using System.Collections.Generic;

namespace CommonLibrary.Models
{
    public class LocationList
    {
        public List<Location> Locations { get; set; }
    }

    public class LocationList2
    {
        public Location[] Locations { get; set; }
    }

    public class Location
    {
        public long Id { get; set; }
        public string DateTime { get; set; }
        public string TransactionId { get; set; }
        public string VehicleId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
    }
}
