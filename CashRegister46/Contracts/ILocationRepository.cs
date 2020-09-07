using Cash_Register.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cash_Register.Contracts
{
    public interface ILocationRepository
    {
        CRLocation GetLocation(short oid);
        List<CRLocation> GetLocations();
        decimal GetReserveAmount(short locationId);
    }
}
