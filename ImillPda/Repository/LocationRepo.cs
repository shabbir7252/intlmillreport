using ImillPda.Contracts;
using ImillPda.Models;
using ImillPda.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillPda.Repository
{
    public class LocationRepo : ILocationRepo
    {
        private readonly IMILLEntities _context = new IMILLEntities();

        public LocationRepo(IMILLEntities context)
        {
            _context = context;
        }

        public LocationVm GetLocation(long locat_Cd)
        {
            var location = _context.SM_Location.FirstOrDefault(x => x.Locat_Cd == locat_Cd);

            return new LocationVm
            {
                Locat_Cd = location.Locat_Cd,
                L_Locat_Name = location.L_Locat_Name,
                A_Locat_Name = location.A_Locat_Name
            };
        }

        public IQueryable<LocationVm> GetLocations()
        {
            var locations = _context.SM_Location.Select(x => new LocationVm
            {
                Locat_Cd = x.Locat_Cd,
                L_Locat_Name = x.L_Locat_Name,
                A_Locat_Name = x.A_Locat_Name
            });

            return locations;
        }
    }
}