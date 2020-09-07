using Cash_Register.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cash_Register.Contracts
{
    public interface ISalesmanRepository
    {
        List<CRSalesman> GetSalesmans(short locationId);
        CRSalesman GetSalesman(short oid);
    }
}
