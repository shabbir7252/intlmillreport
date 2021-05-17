using ImillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImillReports.Contracts
{
    public interface IPurchaseOrderRepo
    {
        List<PurchaseOrderViewModel> GetPurchaseOrders(DateTime? fromDate, DateTime? toDate, string username);
        bool UpdateLpoStatus(long oid, int ldgrCd, long entryId, DateTime transDate, int lpoStatus, int paymentStatus, int qaStatus, 
            string gmcomment, string pmStatus, string qaRemarks, string username);
        List<TransDetailsViewModel> GetDetails(long entryId, DateTime recordDate);
        bool GetPurchaseEmailOrder(DateTime fromDate, DateTime toDate, string username);
    }
}
