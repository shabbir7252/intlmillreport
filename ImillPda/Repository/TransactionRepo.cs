using System;
using System.Net;
using System.Linq;
using ImillPda.Models;
using Newtonsoft.Json;
using System.Net.Mail;
using ImillPda.Contracts;
using ImillPda.ViewModels;
using System.Collections.Generic;

namespace ImillPda.Repository
{

    public class TransactionRepo : ITransactionRepo
    {
        private readonly IMILLEntities _context = new IMILLEntities();
        private readonly ImillPdaEntities _contextPda = new ImillPdaEntities();
        private readonly ILocationRepo _locationContext;
        private readonly IItemRepo _itemContext;

        public TransactionRepo(IMILLEntities context, ImillPdaEntities contextPda,
            ILocationRepo locationContext, IItemRepo itemContext)
        {
            _context = context;
            _contextPda = contextPda;
            _locationContext = locationContext;
            _itemContext = itemContext;
        }

        public List<TransactionVM> GetTransactions()
        {
            CheckEmptyTransactions();
            var transactionVMs = new List<TransactionVM>();

            try
            {
                var locations = _locationContext.GetLocations();
                var pdaTransEntryId = _contextPda.Transactions.Select(a => a.EntryId).ToList();
                var daysBack = DateTime.Now.AddDays(-2);
                var trans = _context.ICS_Transaction.Where(x => x.Voucher_Type == 423 && !pdaTransEntryId.Contains(x.Entry_Id) && x.Voucher_Date >= daysBack);

                //var fdate = new DateTime(2021, 02, 02);
                //var tdate = new DateTime(2021, 02, 02, 23, 59, 59);
                //var trans = _context.ICS_Transaction.Where(x => x.Voucher_Type == 423 && !pdaTransEntryId.Contains(x.Entry_Id) && x.Voucher_Date >= fdate && x.Voucher_Date <= tdate).ToList();

                // var entryIds = trans.Select(x => x.Entry_Id);
                // var transDetails = _context.ICS_Transaction_Details.Where(x => entryIds.Contains(x.Entry_Id));

                foreach (var tran in trans)
                {
                    // var count = transDetails.Where(x => x.Entry_Id == tran.Entry_Id).Count();
                    var location = locations.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd);
                    var locatCd = location != null ? location.Locat_Cd : 0;
                    var locationNameEn = location != null ? location.L_Locat_Name : "";
                    var locationNameAr = location != null ? location.A_Locat_Name : "";

                    transactionVMs.Add(
                        new TransactionVM
                        {
                            EntryId = tran.Entry_Id,
                            TransDate = tran.Voucher_Date,
                            CustomDate = tran.Voucher_Date.ToString("dd/MMM/yyyy"),
                            RequestNumber = tran.Voucher_No,
                            Locat_Cd = short.Parse(locatCd.ToString()),
                            LocationNameEn = locationNameEn,
                            LocationNameAr = locationNameAr,
                            UserDateTime = tran.User_Date_Time != null ? tran.User_Date_Time : DateTime.Now,
                            CustomTime = tran.User_Date_Time != null ? tran.User_Date_Time.ToShortTimeString() : DateTime.Now.ToShortTimeString(),
                            ItemCount = 0,
                            IsHidden = true
                        });
                }

                foreach (var rec in transactionVMs.GroupBy(x => x.TransDate))
                {
                    foreach (var rec2 in rec.GroupBy(x => x.Locat_Cd))
                    {
                        if (rec2.Count() > 1)
                        {
                            var firstRec = rec2.OrderBy(x => x.UserDateTime).FirstOrDefault();
                            firstRec.IsHidden = false;
                            var firstTime = firstRec.UserDateTime;
                            var ids = rec2.Where(x => x.Oid != firstRec.EntryId).Select(x => x.EntryId).ToList();
                            var newRec = rec2.Where(x => x.EntryId != firstRec.EntryId).OrderBy(x => x.UserDateTime);

                            while (ids.Count() > 1)
                            {
                                var newRecOid = newRec.Where(x => ids.Contains(x.EntryId) &&
                                                             x.UserDateTime.TimeOfDay >= firstTime.TimeOfDay &&
                                                             x.UserDateTime.TimeOfDay <= firstTime.AddMinutes(30).TimeOfDay)
                                                 .OrderBy(x => x.UserDateTime).Select(a => a.EntryId).ToList();

                                if (!newRecOid.Any())
                                {
                                    foreach (var remainingId in ids)
                                    {
                                        var remainingRec = newRec.FirstOrDefault(x => x.EntryId == remainingId);
                                        if (remainingRec != null)
                                            remainingRec.IsHidden = false;
                                    }

                                    break;
                                }

                                foreach (var id in newRecOid)
                                    ids.Remove(id);

                                if (ids.Any())
                                {
                                    newRec = newRec.Where(x => ids.Contains(x.EntryId)).OrderBy(x => x.UserDateTime);
                                    if (newRec.Any())
                                    {
                                        firstRec = newRec.OrderBy(x => x.UserDateTime).FirstOrDefault();
                                        firstRec.IsHidden = false;
                                        firstTime = firstRec.UserDateTime;
                                    }
                                }

                            }
                        }
                        else if (rec2.Count() == 1)
                        {
                            rec2.FirstOrDefault().IsHidden = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return transactionVMs.OrderByDescending(x => x.UserDateTime).ToList();
        }

        public TransactionDetailVm GetTransactionDetails(long entryId)
        {
            var modelList = new List<RequestedItemVM>();
            var items = _itemContext.GetItems();
            var trans = _context.ICS_Transaction.FirstOrDefault(x => x.Entry_Id == entryId);
            var transDetails = _context.ICS_Transaction_Details.Where(x => x.Entry_Id == entryId).ToList();

            modelList.AddRange(from det in transDetails
                               select new RequestedItemVM
                               {
                                   Oid = det.Line_No,
                                   Prod_Cd = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Prod_Cd,
                                   PartNumber = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Part_No,
                                   NameEn = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).L_Prod_Name,
                                   NameAr = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).A_Prod_Name,
                                   Qty = det.Qty,
                                   RequestedDate = trans.Voucher_Date,
                                   DeliveryDate = trans.Voucher_Date,
                                   ActQty = 0,
                                   LineNo = det.Line_No,
                                   OrgQty = det.Qty
                               });

            var openTransaction = _contextPda.OpenTransactions.Where(x => x.EntryId == entryId);

            if (openTransaction.Any())
            {
                foreach (var rec in openTransaction)
                {
                    if (modelList.Any(x => x.Prod_Cd == rec.Prod_Cd && x.PartNumber == rec.PartNumber))
                    {
                        var modelRec = modelList.FirstOrDefault(x => x.Prod_Cd == rec.Prod_Cd && x.PartNumber == rec.PartNumber);
                        modelRec.Qty = rec.RequestedQty ?? rec.RequiredQty;
                        modelRec.ActQty = rec.ActualQty;
                        modelRec.OrgQty = rec.RequiredQty;
                        modelRec.IsNewlyAdded = rec.IsNewlyAdded;
                        modelRec.IsReqQtyChanged = rec.IsReqQtyChanged;
                        modelRec.IsVerified = true;
                    }
                }
            }

            var locationNameAr = _locationContext.GetLocation(trans.Locat_Cd).A_Locat_Name;
            var model = new TransactionDetailVm
            {
                Comments = trans.Comments,
                RequestedItems = modelList.OrderByDescending(x => x.RequestedDate),
                LocationNameAr = locationNameAr,
                VoucherDate = trans.Voucher_Date,
                VoucherNo = trans.Voucher_No
            };

            return model;
        }

        private void CheckEmptyTransactions()
        {
            try
            {
                var transactions = _contextPda.Transactions.Where(x => !x.IsCompleted);
                var transDetail = _contextPda.TransactionDetails;

                if (transactions.Any())
                {
                    foreach (var rec in transactions)
                    {
                        if (!transDetail.Any(x => x.TransactionOid == rec.Oid))
                        {
                            _contextPda.Transactions.Remove(rec);
                            _contextPda.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }

        public ItemResponseViewmodel DeleteTransaction(long entryId)
        {
            var source = new ItemResponseViewmodel();
            try
            {
                var transDetails = _context.ICS_Transaction_Details.Where(x => x.Entry_Id == entryId);

                if (transDetails != null)
                    _context.ICS_Transaction_Details.RemoveRange(transDetails);

                var transaction = _context.ICS_Transaction.FirstOrDefault(x => x.Entry_Id == entryId);
                _context.ICS_Transaction.Remove(transaction);
                _context.SaveChanges();
                source.ReponseId = 1;
            }
            catch (Exception ex)
            {
                source.ReponseId = 0;
                source.Message = ex.Message;
            }

            return source;
        }

        public ItemResponseViewmodel SaveTransaction(string entryId, List<RequestedItemVM> itemList)
        {
            var source = new ItemResponseViewmodel();
            var transaction = new Transaction();
            string message = "";

            try
            {
                var _entryId = long.Parse(entryId);
                var trans = _context.ICS_Transaction.FirstOrDefault(x => x.Entry_Id == _entryId);

                var _trans = _contextPda.Transactions.OrderByDescending(x => x.TransactionId).FirstOrDefault();

                transaction.TransactionId = _trans != null ? _trans.TransactionId + 1 : 1;
                transaction.EntryId = trans.Entry_Id;
                transaction.Locat_Cd = trans.Locat_Cd;
                transaction.RequestedDate = trans.Voucher_Date;
                transaction.TransDate = DateTime.Now;
                transaction.IsCompleted = false;

                _contextPda.Transactions.Add(transaction);
                _contextPda.SaveChanges();

                var transactionDetails = new List<TransactionDetail>();
                var wishList = new List<WishList>();

                message = "Trans and wishlist created";
                foreach (var item in itemList)
                {
                    message = "Entered in itemlist";
                    if (item.ActQty > 0)
                    {
                        if (item.Qty - item.ActQty <= 0)
                        {
                            transactionDetails.Add(new TransactionDetail
                            {
                                Entry_Id = trans.Entry_Id,
                                ActualQty = item.ActQty,
                                NameAr = item.NameAr,
                                NameEn = item.NameEn,
                                Prod_Cd = item.Prod_Cd,
                                Qty = item.Qty,
                                RemainingQty = 0,
                                RequestedDate = trans.Voucher_Date,
                                TransactionOid = transaction.Oid,
                                IsDelivered = false,
                                Line_No = item.LineNo
                            });
                        }
                        else if (item.Qty != item.ActQty)
                        {
                            var remQty = item.Qty - item.ActQty;
                            transactionDetails.Add(new TransactionDetail
                            {
                                Entry_Id = trans.Entry_Id,
                                ActualQty = item.ActQty,
                                NameAr = item.NameAr,
                                NameEn = item.NameEn,
                                Prod_Cd = item.Prod_Cd,
                                Qty = item.IsNewlyAdded ? item.ActQty : item.Qty,
                                RemainingQty = item.IsNewlyAdded ? 0 : remQty,
                                RequestedDate = trans.Voucher_Date,
                                TransactionOid = transaction.Oid,
                                IsDelivered = false,
                                Line_No = item.LineNo
                            });

                            if (!item.IsNewlyAdded)
                                wishList.Add(new WishList
                                {
                                    EntryId = trans.Entry_Id,
                                    Prod_Cd = item.Prod_Cd,
                                    NameEn = item.NameEn,
                                    NameAr = item.NameAr,
                                    RemainingQty = remQty,
                                    RequestedDate = trans.Voucher_Date,
                                    RequestedQty = item.Qty,
                                    TransactionOid = transaction.Oid,
                                    TransDetailOid = item.LineNo
                                });
                        }
                    }
                    else
                    {
                        wishList.Add(new WishList
                        {
                            EntryId = trans.Entry_Id,
                            Prod_Cd = item.Prod_Cd,
                            RemainingQty = item.Qty,
                            RequestedDate = trans.Voucher_Date,
                            RequestedQty = item.Qty,
                            TransactionOid = transaction.Oid,
                            TransDetailOid = item.LineNo,
                            NameEn = item.NameEn,
                            NameAr = item.NameAr
                        });
                    }
                }

                _contextPda.TransactionDetails.AddRange(transactionDetails);
                _contextPda.WishLists.AddRange(wishList);
                message = "Saving PDA Trans Detail And Wishlist Detail";
                message = JsonConvert.SerializeObject(transactionDetails);
                _contextPda.SaveChanges();

                source.ReponseId = 3;
                source.Message = "Transaction Saved Successfully! ";
                message = "PDA Transaction Saved Successful";

                var innovaTransDetail = _context.ICS_Transaction_Details.Where(x => x.Entry_Id == _entryId);
                foreach (var transDet in transactionDetails)
                {
                    var innTransDet = innovaTransDetail.FirstOrDefault(x => x.Line_No == transDet.Line_No);
                    if (innTransDet != null)
                        innTransDet.Qty = transDet.ActualQty;
                }

                foreach (var wList in wishList)
                {
                    if (wList.RequestedQty - wList.RemainingQty == 0)
                    {
                        var innTransDet = innovaTransDetail.FirstOrDefault(x => x.Line_No == wList.TransDetailOid);
                        if (innTransDet != null)
                            _context.ICS_Transaction_Details.Remove(innTransDet);
                    }
                    else
                    {
                        var innTransDet = innovaTransDetail.FirstOrDefault(x => x.Line_No == wList.TransDetailOid);
                        if (innTransDet != null)
                            innTransDet.Qty = wList.RequestedQty - wList.RemainingQty;
                    }
                }

                message = "An Attempt to Save In Innova Failed.";
                _context.SaveChanges();
                source.ReponseId = 2;
                source.Message = "Transaction Saved Successfully! Changes Saved in System, ";
                message = "Transaction Saved Successfully In Innova and Changes Saved in System, ";
                transaction.IsCompleted = true;
                message = "Transaction Saved Successfully In Innova and Changes Saved in System, Error Occurred While Marking in the system that saving is done.";
                _contextPda.SaveChanges();
                source.ReponseId = 1;
                source.Message = "Transaction Saved Successfully! Changes Saved in System, Transaction Completed!";
                message = "Transaction Saved Successfully! Changes Saved in System, Transaction Completed!";

                SendEmail(trans.Voucher_No, trans.Locat_Cd);
            }
            catch (Exception ex)
            {
                source.ReponseId = 0;
                source.Message = ex.InnerException.Message + " : : " + message;
            }

            return source;
        }

        private void SendEmail(long voucher_No, short locat_Cd)
        {
            var locationNameAr = _context.SM_Location.FirstOrDefault(x => x.Locat_Cd == locat_Cd).A_Locat_Name;
            var settings = _contextPda.EmailSettings.Where(x => x.IsRegForEmail);
            var htmlString = $"Request has been created <br /> Request Number : <b>{voucher_No}</b> <br /> Location : <b>{locationNameAr}</b>";

            MailMessage mailMessage = new MailMessage
            {
                Subject = $"Request Created : {voucher_No}",
                Body = htmlString,
                IsBodyHtml = true,
                From = new MailAddress("imillmaterialreq@gmail.com"),
            };

            foreach (var setting in settings)
                mailMessage.To.Add(new MailAddress(setting.Emails));

            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("imillmaterialreq@gmail.com", "M@ter!alReq$t")
            };

            smtp.Send(mailMessage);
        }

        public List<TransactionVM> GetDeliveryRequest()
        {
            var transactionVMs = new List<TransactionVM>();
            var trans = _contextPda.Transactions.ToList();
            var locations = _locationContext.GetLocations();

            foreach (var tran in trans)
            {
                var location = locations.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd);

                transactionVMs.Add(
                    new TransactionVM
                    {
                        Oid = tran.Oid,
                        EntryId = tran.EntryId,
                        TransDate = tran.RequestedDate,
                        Locat_Cd = location.Locat_Cd,
                        LocationNameEn = location.L_Locat_Name,
                        LocationNameAr = location.A_Locat_Name,
                        IsCompleted = tran.IsCompleted
                    });
            }

            return transactionVMs.OrderByDescending(x => x.TransDate).ToList();
        }

        public List<RequestedItemVM> GetRequestDetails(long oid)
        {
            var modelList = new List<RequestedItemVM>();

            var items = _itemContext.GetItems();
            var trans = _contextPda.Transactions.FirstOrDefault(x => x.Oid == oid);
            var transDetails = _contextPda.TransactionDetails.Where(x => x.TransactionOid == trans.Oid).ToList();

            foreach (var det in transDetails)
            {
                modelList.Add(new RequestedItemVM
                {
                    Oid = det.Oid,
                    Prod_Cd = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Prod_Cd,
                    PartNumber = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Part_No,
                    NameEn = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).L_Prod_Name,
                    NameAr = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).A_Prod_Name,
                    Qty = det.Qty,
                    RequestedDate = trans.RequestedDate,
                    ActQty = det.ActualQty,
                    IsDelivered = det.IsDelivered
                });
            }

            return modelList.OrderByDescending(x => x.RequestedDate).ToList();
        }

        public ItemResponseViewmodel SaveDeliveryRequest(List<RequestedItemVM> itemList)
        {
            var source = new ItemResponseViewmodel();

            long entryId = 0;

            try
            {
                var trans = _contextPda.TransactionDetails.ToList();

                foreach (var item in itemList)
                {
                    var transaction = trans.FirstOrDefault(x => x.Oid == item.Oid);
                    if (item.IsVerified)
                    {
                        var transDetails = transaction;
                        transDetails.DeliveredQty = item.ActQty;
                        transDetails.DeliveryDate = DateTime.Now;
                        transDetails.IsDelivered = true;
                    }

                    entryId = transaction.Entry_Id;
                }

                _contextPda.SaveChanges();

                source.ReponseId = 1;
                source.Message = "Transaction Saved Successful";
            }
            catch (Exception ex)
            {
                source.ReponseId = 2;
                source.Message = ex.Message;
            }

            try
            {
                var transDetails = _contextPda.TransactionDetails.Where(x => x.Entry_Id == entryId);
                if (!transDetails.Any(x => !x.IsDelivered))
                {
                    _contextPda.Transactions.FirstOrDefault(x => x.EntryId == entryId).IsCompleted = true;
                    _context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                source.ReponseId = 3;
                source.Message = ex.Message;
            }

            return source;
        }

        public List<TransactionVM> Wishlist()
        {
            var transactionVMs = new List<TransactionVM>();
            var wishlistTransOid = _contextPda.WishLists.Select(x => x.TransactionOid).ToList();
            var trans = _contextPda.Transactions.Where(x => wishlistTransOid.Contains(x.Oid)).ToList();
            var locations = _locationContext.GetLocations();

            foreach (var tran in trans)
            {
                transactionVMs.Add(
                    new TransactionVM
                    {
                        Oid = tran.Oid,
                        EntryId = tran.EntryId,
                        TransDate = tran.RequestedDate,
                        Locat_Cd = locations.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd).Locat_Cd,
                        LocationNameEn = locations.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd).L_Locat_Name,
                        LocationNameAr = locations.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd).A_Locat_Name,
                        IsCompleted = tran.IsCompleted
                    });
            }

            return transactionVMs.OrderByDescending(x => x.TransDate).ToList();
        }

        public List<RequestedItemVM> GetWishlistDetails(long entryId)
        {
            var modelList = new List<RequestedItemVM>();
            var items = _itemContext.GetItems();
            var trans = _contextPda.Transactions.FirstOrDefault(x => x.EntryId == entryId);
            var wishlist = _contextPda.WishLists.Where(x => x.EntryId == entryId && x.RemainingQty > 0).ToList();

            foreach (var det in wishlist)
            {
                modelList.Add(new RequestedItemVM
                {
                    Oid = det.Oid,
                    Prod_Cd = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Prod_Cd,
                    PartNumber = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Part_No,
                    NameEn = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).L_Prod_Name,
                    NameAr = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).A_Prod_Name,
                    Qty = det.RemainingQty,
                    RequestedDate = trans.RequestedDate,
                    DeliveryDate = trans.RequestedDate,
                    ActQty = 0,
                    Locat_Cd = trans.Locat_Cd
                });
            }

            return modelList.OrderByDescending(x => x.RequestedDate).ToList();
        }

        public ItemResponseViewmodel UpdateWishListRequest(List<RequestedItemVM> itemList)
        {
            var source = new ItemResponseViewmodel();
            var locat_Cd = itemList.FirstOrDefault().Locat_Cd;
            var today = DateTime.Now.Date;
            var transInnova = _context.ICS_Transaction.FirstOrDefault(x => x.Locat_Cd == locat_Cd && x.Voucher_Date >= today && x.Voucher_Date <= today);

            if (transInnova == null)
            {
                source.ReponseId = 2;
                source.Message = "No transaction for the location found!";
            }
            else
            {
                var transEntryId = transInnova.Entry_Id;
                var itemUnitDetails = _context.ICS_Item_Unit_Details;
                var items = _itemContext.GetItems();
                var remWishItem = new List<WishList>();

                try
                {
                    var wishlist = _contextPda.WishLists.ToList();
                    foreach (var item in itemList)
                    {
                        var itemUnitDetail = itemUnitDetails.FirstOrDefault(x => x.Prod_Cd == item.Prod_Cd);
                        if (item.IsVerified)
                        {
                            var _wishlist = wishlist.FirstOrDefault(x => x.Oid == item.Oid);
                            var transDetail = new ICS_Transaction_Details
                            {
                                Entry_Id = transEntryId,
                                Prod_Cd = _wishlist.Prod_Cd,
                                Qty = _wishlist.RemainingQty,
                                Unit_Entry_ID = itemUnitDetail.Unit_Entry_Id,
                                Exchange_Rate = 1,
                                Prod_Name = items.FirstOrDefault(x => x.Prod_Cd == item.Prod_Cd).A_Prod_Name,
                                Base_Qty = itemUnitDetail.Base_Qty,
                                Processed_Qty = 0,
                                FC_Rate = 0,
                                FC_Prod_Dis = 0,
                                FC_Amount = 0,
                                Prod_Exp_Amt = 0,
                                Promo_Type = "R",
                                Item_Cost = 0,
                                Dis_In_Percent = false,
                                Linked = false,
                                Show = true,
                                GL_Post_Sequence_Counter = 0,
                                Status = 2,
                                User_Date_Time = DateTime.Now,
                                row_id = new Guid(),
                                Non_Stock_Manual_Cost = 0,
                                Item_Type_Cd = items.FirstOrDefault(x => x.Prod_Cd == item.Prod_Cd).Item_Type_Cd,
                                Is_Dented = false,
                                Weight = 0,
                                Processed_SQ_Base_Qty = 0,
                                Processed_PO_Base_Qty = 0,
                                Processed_GRN_Base_Qty = 0,
                                Processed_CQ_Base_Qty = 0,
                                Processed_SO_Base_Qty = 0,
                                Processed_INV_Base_Qty = 0,
                                Processed_CSR_Base_Qty = 0,
                                Processed_SR_Base_Qty = 0,
                                Processed_STS_Base_Qty = 0,
                                Processed_ESTK_Base_Qty = 0,
                                Processed_SSTK_Base_Qty = 0,
                                Processed_Others_Base_Qty = 0,
                                Fc_Prod_Dis_Per = 0,
                                Commission_Amt = 0,
                                master_discount = 0
                            };

                            _context.ICS_Transaction_Details.Add(transDetail);
                            remWishItem.Add(_wishlist);
                        }
                    }

                    _context.SaveChanges();
                    _contextPda.WishLists.RemoveRange(remWishItem);
                    _contextPda.SaveChanges();

                    source.ReponseId = 1;
                    source.Message = "Transaction Successful!";
                }
                catch (Exception ex)
                {
                    source.ReponseId = 2;
                    source.Message = ex.Message;
                }

            }

            return source;
        }

        public ScanResponseViewModel CompleteInnovaTransaction()
        {
            var source = new ScanResponseViewModel();
            var transactions = _contextPda.Transactions.Where(x => !x.IsCompleted).ToList();
            var transDetail = _contextPda.TransactionDetails.ToList();
            return source;
        }

        public ScanResponseViewModel AddToInnovaRequest(string partNumber, string weight, string entryId)
        {
            var source = new ScanResponseViewModel();
            string message = "";

            try
            {
                var _entryId = int.Parse(entryId);
                var _weight = string.IsNullOrEmpty(weight) ? 1 : decimal.Parse(weight);
                var transaction = _context.ICS_Transaction.FirstOrDefault(x => x.Entry_Id == _entryId);
                if (transaction != null)
                {
                    var item = _itemContext.GetItems().FirstOrDefault(x => x.Part_No == partNumber);
                    var unit_Entry_Id = _context.ICS_Item_Unit_Details.FirstOrDefault(x => x.Prod_Cd == item.Prod_Cd).Unit_Entry_Id;

                    if (item != null)
                    {
                        var transDetail = new ICS_Transaction_Details
                        {
                            Entry_Id = transaction.Entry_Id,
                            Add_Locat_Cd = transaction.Locat_Cd,
                            Prod_Cd = item.Prod_Cd,
                            Unit_Entry_ID = unit_Entry_Id,
                            Exchange_Rate = 1,
                            Prod_Name = item.A_Prod_Name,
                            Qty = _weight,
                            Base_Qty = _weight,
                            Processed_Qty = 0,
                            FC_Rate = 0,
                            FC_Prod_Dis = 0,
                            FC_Amount = 0,
                            Prod_Exp_Amt = 0,
                            Promo_Type = "R",
                            Item_Cost = 0,
                            Dis_In_Percent = false,
                            Linked = false,
                            Show = false,
                            GL_Post_Sequence_Counter = 0,
                            Status = 2,
                            User_Date_Time = DateTime.Now,
                            row_id = new Guid(),
                            Non_Stock_Manual_Cost = 0,
                            Item_Type_Cd = 1,
                            Is_Dented = false,
                            Weight = 0,
                            Processed_SQ_Base_Qty = 0,
                            Processed_PO_Base_Qty = 0,
                            Processed_GRN_Base_Qty = 0,
                            Processed_CQ_Base_Qty = 0,
                            Processed_SO_Base_Qty = 0,
                            Processed_INV_Base_Qty = 0,
                            Processed_CSR_Base_Qty = 0,
                            Processed_SR_Base_Qty = 0,
                            Processed_STS_Base_Qty = 0,
                            Processed_ESTK_Base_Qty = 0,
                            Processed_SSTK_Base_Qty = 0,
                            Processed_Others_Base_Qty = 0,
                            Fc_Prod_Dis_Per = 0,
                            Commission_Amt = 0,
                            master_discount = 0
                        };

                        _context.ICS_Transaction_Details.Add(transDetail);
                        _context.SaveChanges();

                        source.Oid = item.Prod_Cd;
                        source.Weight = _weight;
                        source.CheckDigit = "NA";
                        source.Identifier = "NA";
                        source.PartNumber = item.Part_No;
                        source.Message = "Transaction Successful";
                        source.ResponseId = 1;
                        source.ItemNameEn = item.L_Prod_Name;
                        source.ItemNameAr = item.A_Prod_Name;
                        source.RequestedDate = transaction.Voucher_Date;
                        source.LineNo = transDetail.Line_No;
                        source.Locat_Cd = transaction.Locat_Cd;
                        source.Prod_Cd = item.Prod_Cd;
                    }
                    else
                    {
                        source.ResponseId = 0;
                        source.Message = $"Item Not Found with part number : {partNumber}";
                    }
                }
                else
                {
                    source.ResponseId = 0;
                    source.Message = $"Id doesn't match any transaction | {entryId}";
                }
            }
            catch (Exception ex)
            {
                source.ResponseId = 0;
                source.Message = ex.InnerException.Message + " : : " + message;
            }

            return source;
        }

        public ScanResponseViewModel UpdateQuantity(string partNumber, int entryId, int qty)
        {
            var source = new ScanResponseViewModel();
            try
            {
                var item = _itemContext.GetItems().FirstOrDefault(x => x.Part_No == partNumber);
                var transDetail = _context.ICS_Transaction_Details.FirstOrDefault(x => x.Entry_Id == entryId && x.Prod_Cd == item.Prod_Cd);
                transDetail.Qty = qty;
                _context.SaveChanges();

                source.ResponseId = 1;
                source.Message = "Updated Successfully";
            }
            catch (Exception ex)
            {
                source.ResponseId = 0;
                source.Message = ex.InnerException.Message;
            }
            return source;
        }

        public ItemResponseViewmodel DraftTransactions(string entryId, List<RequestedItemVM> itemList)
        {
            var source = new ItemResponseViewmodel();
            string message = "";

            try
            {
                var openTransactions = new List<OpenTransaction>();
                var _entryId = int.Parse(entryId);
                var dbOpenTrans = _contextPda.OpenTransactions.Where(x => x.EntryId == _entryId).ToList();

                foreach (var item in itemList)
                {
                    if (item.IsVerified)
                    {
                        if (dbOpenTrans.Any(x => x.Prod_Cd == item.Prod_Cd && x.PartNumber == item.PartNumber))
                        {
                            message = $"Changing Transaction in PDA Prod : {item.Prod_Cd}";
                            var dbOpenTran = dbOpenTrans.FirstOrDefault(x => x.Prod_Cd == item.Prod_Cd && x.PartNumber == item.PartNumber);
                            dbOpenTran.RequestedQty = item.Qty;
                            dbOpenTran.ActualQty = item.ActQty;
                            dbOpenTran.RequiredQty = item.OrgQty;
                            dbOpenTran.IsNewlyAdded = item.IsNewlyAdded;
                            dbOpenTran.IsReqQtyChanged = item.IsReqQtyChanged;
                        }
                        else
                        {
                            message = $"Adding Transaction in PDA Prod : {item.Prod_Cd}";
                            openTransactions.Add(new OpenTransaction
                            {
                                EntryId = _entryId,
                                Locat_Cd = item.Locat_Cd,
                                Prod_Cd = item.Prod_Cd,
                                PartNumber = item.PartNumber,
                                RequestedQty = item.Qty,
                                ActualQty = item.ActQty,
                                RequiredQty = item.OrgQty,
                                IsNewlyAdded = item.IsNewlyAdded,
                                IsReqQtyChanged = item.IsReqQtyChanged,
                                CreatedOn = DateTime.Now
                            });
                        }
                    }
                    else
                    {
                        if (dbOpenTrans.Any(x => x.Prod_Cd == item.Prod_Cd && x.PartNumber == item.PartNumber))
                        {
                            var dbOpenTran = dbOpenTrans.FirstOrDefault(x => x.Prod_Cd == item.Prod_Cd && x.PartNumber == item.PartNumber);
                            _contextPda.OpenTransactions.Remove(dbOpenTran);
                        }
                    }
                }

                _contextPda.OpenTransactions.AddRange(openTransactions);
                message = "saving Transaction in PDA";
                _contextPda.SaveChanges();
                source.ReponseId = 1;
                source.Message = "Transaction Saved Successfully!";
                message = "Transaction Saved Successfully!";
            }
            catch (Exception ex)
            {
                source.ReponseId = 0;
                source.Message = ex.InnerException.Message + " : : " + message;
            }

            return source;
        }

        public ItemResponseViewmodel DeleteOpenTransaction(string entryId)
        {
            var source = new ItemResponseViewmodel();
            string message = "";

            try
            {
                var openTransactions = new List<OpenTransaction>();
                var _entryId = int.Parse(entryId);
                var dbOpenTrans = _contextPda.OpenTransactions.Where(x => x.EntryId == _entryId).ToList();
                _contextPda.OpenTransactions.RemoveRange(dbOpenTrans);
                _contextPda.SaveChanges();
                source.ReponseId = 1;
                source.Message = "Transaction Deleted Successfully!";
                message = "Transaction Deleted Successfully!";
            }
            catch (Exception ex)
            {
                source.ReponseId = 0;
                source.Message = ex.InnerException.Message + " : : " + message;
            }
            return source;
        }
    }
}