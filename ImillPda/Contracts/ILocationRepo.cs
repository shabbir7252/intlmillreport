using ImillPda.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImillPda.Contracts
{
    public interface ILocationRepo
    {
        IQueryable<LocationVm> GetLocations();
        LocationVm GetLocation(long locat_Cd);
    }
}
