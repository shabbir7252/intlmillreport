using IntlmillReports.Contracts;
using IntlmillReports.Models;
using IntlmillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IntlmillReports.Repository
{
    public class LocationRepository : ILocationRepository
    {
        private readonly IMILLEntities _context;

        public LocationRepository(IMILLEntities context)
        {
            _context = context;
        }

        public LocationViewModel GetLocations()
        {
            var locations = _context.SM_Location;

            var locationItems = new List<LocationItem>();

            foreach (var location in locations)
            {
                var locationItem = new LocationItem
                {
                    LocationId = location.Locat_Cd,
                    Name = location.L_Locat_Name,
                    NameAr = location.A_Locat_Name,
                    ShortName = location.L_Short_Name
                };

                locationItems.Add(locationItem);
            }

            return new LocationViewModel
            {
                 LocationItems = locationItems
            };
        }
    }
}