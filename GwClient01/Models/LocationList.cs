using System;
using System.Collections.Generic;

namespace GwClient01.Models
{
    public class LocationList
    {
        public List<Location> Locations { get; set; }
    }

    public class Location
    {
        public string DateTime { get; set; }
        public string TransactionId { get; set; }
        public string VehicleId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
    }
}
