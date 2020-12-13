using System;
namespace GwClient02.Models
{
    public class RootObject
    {
        public string dateTime { get; set; }
        public string CountryCode { get; set; }
        public Accident_[] Accidents { get; set; }
    }
    public class Accident_
    {
        public string OccurenceDate { get; set; }
        public string TransactionId { get; set; }
        public string VehicleId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Message { get; set; }
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
