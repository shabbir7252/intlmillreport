using System.Collections.Generic;
using static ImillReports.Repository.LocationRepository;

namespace ImillReports.ViewModels
{
    public class LocationViewModel
    {
        public List<LocationItem> LocationItems { get; set; }
    }

    public class LocationItem
    {
        public int LocationId { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string ShortName { get; set; }
        public bool IsSelected { get; set; }
        public LocationType Type { get; internal set; }
        public string TypeName { get; internal set; }
    }

    public class LocationGroupItem
    {
        public int LocationId { get; set; }
        public string Name { get; set; }
    }

    public class TaLocation
    {
        public int Oid { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string DeviceCode { get; set; }
    }
}