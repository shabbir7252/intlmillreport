using System;
using System.Linq;
using ImillReports.Models;
using ImillReports.Contracts;

namespace ImillReports.Repository
{
    public class PdaRepo : IPdaRepo
    {
        private readonly ImillPdaEntities _context;
        private readonly IMILLEntities _imillContext;

        public PdaRepo(ImillPdaEntities context, IMILLEntities imillContext)
        {
            _context = context;
            _imillContext = imillContext;
        }

        public string ValidateTransDetail()
        {
            var message = "True";
            try
            {
                var checkDate = DateTime.Now.AddDays(-2).Date;
                var wishlist = _context.WishLists.Where(x => x.RequestedDate >= checkDate && x.RemainingQty == x.RequestedQty).GroupBy(x => x.EntryId);
                var entryIds = wishlist.Select(x => x.Key).ToList();
                var transDetails = _imillContext.ICS_Transaction_Details.Where(x => entryIds.Contains(x.Entry_Id)).ToList();

                if (transDetails.Any())
                {
                    var skipChange = true;
                    foreach (var itemList in wishlist)
                    {
                        foreach (var item in itemList)
                        {
                            var transDetailItem = transDetails.FirstOrDefault(x => x.Entry_Id == itemList.Key &&
                                                                                   x.Line_No == item.TransDetailOid);
                            if (transDetailItem != null)
                            {
                                // transDetails.Remove(transDetailItem);
                                //message += $"{transDetailItem.Entry_Id} ";
                                //var transDetail = _imillContext.ICS_Transaction_Details.FirstOrDefault(x => x.Line_No == item.TransDetailOid);
                                //transDetail.Reference_No = ($"Qty={item.RequestedQty}, Deleted By PDA User");
                                //transDetail.Qty = 0;
                                _imillContext.ICS_Transaction_Details.Remove(transDetailItem);
                                skipChange = false;
                            }
                        }
                    }

                    if (!skipChange)
                        _imillContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return $"{message} | Error :  {ex.Message}";
            }

            return message;
        }
    }
}