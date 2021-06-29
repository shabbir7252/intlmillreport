using System.Linq;
using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;
using System;

namespace ImillReports.Repository
{
    public class LocationRepository : ILocationRepository
    {
        private readonly IMILLEntities _context;
        private readonly IBaseRepository _baseRepository;

        public LocationRepository(IMILLEntities context, IBaseRepository baseRepository)
        {
            _context = context;
            _baseRepository = baseRepository;
        }

        public enum LocationType
        {
            Mall = 1,
            Coops = 2,
            HO = 3
        }

        public LocationViewModel GetLocations()
        {
            var locations = _context.SM_Location.Where(x => x.Locat_Cd != 67 && x.Locat_Cd != 63 && x.Locat_Cd != 73 && x.Locat_Cd != 84).ToList();

            var locationItems = new List<LocationItem>();

            var cooperativeIds = _baseRepository.GetLocationIds(LocationType.Coops);
            var mallIds = _baseRepository.GetLocationIds(LocationType.Mall);

            locationItems.AddRange(from location in locations
                                   let locationItem = new LocationItem
                                   {
                                       LocationId = location.Locat_Cd,
                                       Name = location.L_Locat_Name,
                                       NameAr = location.A_Locat_Name,
                                       ShortName = location.L_Short_Name,
                                       Type = cooperativeIds.Contains(location.Locat_Cd)
                                                ? LocationType.Coops
                                                : mallIds.Contains(location.Locat_Cd)
                                                    ? LocationType.Mall
                                                    : LocationType.HO,
                                       TypeName = cooperativeIds.Contains(location.Locat_Cd)
                                                    ? Enum.GetName(typeof(LocationType), LocationType.Coops)
                                                    : mallIds.Contains(location.Locat_Cd)
                                                        ? Enum.GetName(typeof(LocationType), LocationType.Mall)
                                                        : Enum.GetName(typeof(LocationType), LocationType.HO)
                                   }
                                   select locationItem);

            return new LocationViewModel
            {
                LocationItems = locationItems
            };
        }
    }
}


// Coop - 
// Dasman (Dasma Cooperative Society - 55), 
// Dahiya (Dahiya Abdulla Al-Salem Cooperative - 57), 
// kefan (Kaifan Cooperative - 58), 
// nuzha (Nuzha Cooperative - 59), 
// khaldiya (Khaldiya Cooperative - 60), 
// qortuba (Qortuba Cooperative - 61), 
// yarmouk (Yarmouk Cooperative - 62), 
// zahra (Zahra Cooperative - 64), 
// surra (Surra Branch - 77), 
// bnied Al gar (Bnied Al-Gar - 80), 
// shuhada (Shuhada Branch - 74)



// Mall - 
// Salhia (Salhia Complex - 56), 
// Avenues 1 (The Avenues - 54), 
// Avenues 2 (The Avenues - Phase 4 - 83), 
// 360 Mall (65), 
// Gate Mall (66), 
// Al Hamra Mall (78), 
// Jahra (Sama Mall, Jahra - 81), 
// Al-Kout (82), 
// Salmiya-2 (Salmiya Branch 2 - 76), 
// KPC (68)

// HO
// Ardiya Store (84),
// Main Store (1)