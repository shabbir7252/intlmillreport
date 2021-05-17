using ImillPda.ViewModels;
using System;
using System.Collections.Generic;

namespace ImillPda.Contracts
{
    public interface ITransactionRepo
    {
        List<TransactionVM> GetTransactions();
        TransactionDetailVm GetTransactionDetails(long entryId);
        ItemResponseViewmodel DeleteTransaction(long entryId);
        ItemResponseViewmodel SaveTransaction(string entryId, List<RequestedItemVM> itemList);
        List<TransactionVM> GetDeliveryRequest();
        List<RequestedItemVM> GetRequestDetails(long oid);
        ItemResponseViewmodel SaveDeliveryRequest(List<RequestedItemVM> itemList);
        List<TransactionVM> Wishlist();
        List<RequestedItemVM> GetWishlistDetails(long entryId);
        ItemResponseViewmodel UpdateWishListRequest(List<RequestedItemVM> itemList);
        ScanResponseViewModel CompleteInnovaTransaction();
        ScanResponseViewModel AddToInnovaRequest(string partNumber, string weight, string entryId);
        ScanResponseViewModel UpdateQuantity(string partNumber, int entryId, int qty);
        ItemResponseViewmodel DraftTransactions(string entryId, List<RequestedItemVM> itemList);
        ItemResponseViewmodel DeleteOpenTransaction(string entryId);
        List<ConsolidatedItems> GetConsolidatedItems(DateTime fromDate, string productIds, string group, string type);
        ProductViewModel GetGroupProduct(List<long> groupIds);
        string SaveConsTrans(DateTime fromDate, List<ConsolidatedItems> consolidatedItems);
    }
}
