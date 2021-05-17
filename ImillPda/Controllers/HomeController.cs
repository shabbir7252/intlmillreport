using System;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using ImillPda.ViewModels;
using ImillPda.Repository;
using System.Collections.Generic;
using ImillPda.Contracts;

namespace ImillPda.Controllers
{
    public class HomeController : Controller
    {
        //public IMILLEntities _context = new IMILLEntities();
        //public ImillPdaEntities _contextPda = new ImillPdaEntities();
        private readonly TransactionRepo _transContext;
        private readonly ItemRepo _itemContext;
        private readonly IProductRepository _productRepository;

        public HomeController(TransactionRepo transContext, ItemRepo itemContext, IProductRepository productRepository)
        {
            _transContext = transContext;
            _itemContext = itemContext;
            _productRepository = productRepository;
        }

        [HttpGet]
        public ActionResult Index()
        {
            //CheckEmptyTransactions();
            //var location = _context.SM_Location;

            //var transactionVMs = new List<TransactionVM>();

            //var pdaTransEntryId = _contextPda.Transactions.Select(a => a.EntryId).ToList();
            //var daysBack = DateTime.Now.AddDays(-2);
            //var trans = _context.ICS_Transaction.Where(x => x.Voucher_Type == 423 && !pdaTransEntryId.Contains(x.Entry_Id) && x.Voucher_Date >= daysBack);

            ////var fdate = new DateTime(2021, 02, 02);
            ////var tdate = new DateTime(2021, 02, 02, 23, 59, 59);
            ////var trans = _context.ICS_Transaction.Where(x => x.Voucher_Type == 423 && !pdaTransEntryId.Contains(x.Entry_Id) && x.Voucher_Date >= fdate && x.Voucher_Date <= tdate).ToList();
            //var entryIds = trans.Select(x => x.Entry_Id);
            //var transDetails = _context.ICS_Transaction_Details.Where(x => entryIds.Contains(x.Entry_Id));

            //foreach (var tran in trans)
            //{
            //    var count = transDetails.Where(x => x.Entry_Id == tran.Entry_Id).Count();
            //    transactionVMs.Add(
            //        new TransactionVM
            //        {
            //            EntryId = tran.Entry_Id,
            //            TransDate = tran.Voucher_Date,
            //            CustomDate = tran.Voucher_Date.ToString("dd/MMM/yyyy"),
            //            RequestNumber = tran.Voucher_No,
            //            Locat_Cd = location.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd).Locat_Cd,
            //            LocationNameEn = location.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd).L_Locat_Name,
            //            LocationNameAr = location.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd).A_Locat_Name,
            //            UserDateTime = tran.User_Date_Time,
            //            CustomTime = tran.User_Date_Time.ToShortTimeString(),
            //            ItemCount = count,
            //            IsHidden = true
            //        });
            //}

            //_context.Dispose();
            //_contextPda.Dispose();


            //foreach (var rec in transactionVMs.GroupBy(x => x.TransDate))
            //{
            //    foreach (var rec2 in rec.GroupBy(x => x.Locat_Cd))
            //    {
            //        if (rec2.Count() > 1)
            //        {
            //            var firstRec = rec2.OrderBy(x => x.UserDateTime).FirstOrDefault();
            //            firstRec.IsHidden = false;
            //            var firstTime = firstRec.UserDateTime;
            //            var ids = rec2.Where(x => x.Oid != firstRec.EntryId).Select(x => x.EntryId).ToList();
            //            var newRec = rec2.Where(x => x.EntryId != firstRec.EntryId).OrderBy(x => x.UserDateTime);

            //            while (ids.Count() > 1)
            //            {
            //                var newRecOid = newRec.Where(x => ids.Contains(x.EntryId) &&
            //                                             x.UserDateTime.TimeOfDay >= firstTime.TimeOfDay &&
            //                                             x.UserDateTime.TimeOfDay <= firstTime.AddMinutes(30).TimeOfDay)
            //                                 .OrderBy(x => x.UserDateTime).Select(a => a.EntryId);

            //                foreach (var id in newRecOid)
            //                    ids.Remove(id);

            //                if (ids.Any())
            //                {
            //                    newRec = newRec.Where(x => ids.Contains(x.EntryId)).OrderBy(x => x.UserDateTime);
            //                    if (newRec.Any())
            //                    {
            //                        firstRec = newRec.OrderBy(x => x.UserDateTime).FirstOrDefault();
            //                        firstRec.IsHidden = false;
            //                        firstTime = firstRec.UserDateTime;
            //                    }
            //                }

            //            }
            //            //foreach(var item in rec2.GroupBy(x => x.ItemCount))
            //            //{
            //            //    if (item.Count() > 1)
            //            //    {
            //            //        item.OrderByDescending(x => x.UserDateTime).FirstOrDefault().IsHidden = false;
            //            //    }
            //            //    else
            //            //    {
            //            //        item.FirstOrDefault().IsHidden = false;
            //            //    }
            //            //}
            //        }
            //        else if (rec2.Count() == 1)
            //        {
            //            rec2.FirstOrDefault().IsHidden = false;
            //        }
            //    }
            //}

            //ViewBag.DataSource = transactionVMs.OrderByDescending(x => x.UserDateTime);
            ViewBag.DataSource = _transContext.GetTransactions();
            return View();
        }

        [HttpPost]
        public ContentResult GetBarcodeValue(string barcode)
        {
            var source = new ScanResponseViewModel();
            try
            {
                if (!string.IsNullOrEmpty(barcode))
                {
                    var items = _itemContext.GetItems();
                    // var identifier = barcode.Substring(0, 2);
                    var partNumber = "";
                    // var checkDigit = "";
                    decimal _weight = 1;
                    var item = new ItemVm();
                    item = null;

                    var barcodeLength = barcode.Length;

                    if (barcodeLength < 4)
                    {
                        source.Oid = 0;
                        source.Weight = 0;
                        source.CheckDigit = "NA";
                        source.Identifier = "NA";
                        source.PartNumber = "NA";
                        source.Message = "Invalid Barcode!";
                        source.ResponseId = 0;

                        return new ContentResult
                        {
                            Content = JsonConvert.SerializeObject(source),
                            ContentType = "application/json"
                        };
                    }
                    else if (barcodeLength == 4 || barcodeLength > 10)
                    {
                        partNumber = barcode;
                    }
                    else
                    {
                        partNumber = barcode.Substring(0, 4);

                        if (barcodeLength == 10)
                        {
                            var weight = barcode.Substring(4, 6);

                            if (!string.IsNullOrEmpty(weight))
                                _weight = (decimal)((double)int.Parse(weight) / 1000);
                        }
                    }

                    if (item == null)
                        item = items.FirstOrDefault(x => x.Part_No == partNumber);

                    if (item != null)
                    {
                        source.Oid = item.Prod_Cd;
                        source.Weight = _weight == 0 ? 1 : _weight;
                        source.CheckDigit = "NA";
                        source.Identifier = "NA";
                        source.PartNumber = item.Part_No;
                        source.Message = "";
                        source.ResponseId = 1;
                        source.ItemNameEn = item.L_Prod_Name;
                        source.ItemNameAr = item.A_Prod_Name;
                    }
                }
            }
            catch (Exception ex)
            {
                source.Oid = 0;
                source.Weight = 0;
                source.CheckDigit = "NA";
                source.Identifier = "NA";
                source.PartNumber = "NA";
                source.Message = ex.Message;
                source.ResponseId = 0;
            }

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        public ActionResult GetTransactionDetails(long entryId)
        {
            //var items = _context.ICS_Item.ToList();
            //var location = _context.SM_Location.ToList();
            //var trans = _context.ICS_Transaction.FirstOrDefault(x => x.Entry_Id == entryId);

            //var modelList = new List<RequestedItemVM>();
            //var transDetails = _context.ICS_Transaction_Details.Where(x => x.Entry_Id == entryId).ToList();

            //foreach (var det in transDetails)
            //{
            //    modelList.Add(new RequestedItemVM
            //    {
            //        Oid = det.Line_No,
            //        Prod_Cd = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Prod_Cd,
            //        PartNumber = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Part_No,
            //        //NameEn = "(" + items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Part_No.ToString() + ") " + items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).L_Prod_Name,
            //        //NameAr = "(" + items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Part_No.ToString() + ") " + items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).A_Prod_Name,
            //        NameEn = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).L_Prod_Name,
            //        NameAr = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).A_Prod_Name,
            //        Qty = det.Qty,
            //        RequestedDate = trans.Voucher_Date,
            //        DeliveryDate = trans.Voucher_Date,
            //        ActQty = 0,
            //        LineNo = det.Line_No,
            //        OrgQty = det.Qty
            //    });
            //}

            //var openTransaction = _contextPda.OpenTransactions.Where(x => x.EntryId == entryId).ToList();

            //if (openTransaction.Any())
            //{
            //    foreach (var rec in openTransaction)
            //    {
            //        if (modelList.Any(x => x.Prod_Cd == rec.Prod_Cd && x.PartNumber == rec.PartNumber))
            //        {
            //            var modelRec = modelList.FirstOrDefault(x => x.Prod_Cd == rec.Prod_Cd && x.PartNumber == rec.PartNumber);
            //            modelRec.Qty = rec.RequestedQty ?? rec.RequiredQty;
            //            modelRec.ActQty = rec.ActualQty;
            //            modelRec.OrgQty = rec.RequiredQty;
            //            modelRec.IsNewlyAdded = rec.IsNewlyAdded;
            //            modelRec.IsReqQtyChanged = rec.IsReqQtyChanged;
            //            modelRec.IsVerified = true;
            //        }
            //    }
            //}

            var model = _transContext.GetTransactionDetails(entryId);
            ViewBag.Comments = model.Comments;
            ViewBag.DataSource = model.RequestedItems;
            ViewBag.EntryId = entryId;
            ViewBag.location = model.LocationNameAr;
            ViewBag.VoucherDate = model.VoucherDate;
            ViewBag.ReqNo = model.VoucherNo;

            return View();
        }

        [HttpPost]
        public ContentResult DeleteTransaction(long entryId)
        {
            var source = _transContext.DeleteTransaction(entryId);
            //try
            //{
            //    //var isTrans = _contextPda.DeletedTransIds.Any(x => x.EntryId == entryId);
            //    //if (!isTrans)
            //    //{
            //    //    var newPdaDeletedTrans = new DeletedTransId
            //    //    {
            //    //        EntryId = entryId,
            //    //        TransDate = DateTime.Now
            //    //    };

            //    //    _contextPda.DeletedTransIds.Add(newPdaDeletedTrans);
            //    //    _contextPda.SaveChanges();
            //    //}

            //    var transDetails = _context.ICS_Transaction_Details.Where(x => x.Entry_Id == entryId);

            //    if (transDetails != null)
            //        _context.ICS_Transaction_Details.RemoveRange(transDetails);

            //    var transaction = _context.ICS_Transaction.FirstOrDefault(x => x.Entry_Id == entryId);
            //    _context.ICS_Transaction.Remove(transaction);
            //    _context.SaveChanges();
            //    source.ReponseId = 1;
            //}
            //catch (Exception ex)
            //{
            //    source.ReponseId = 0;
            //    source.Message = ex.Message;
            //}

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPost]
        public ActionResult SaveTransaction(string entryId, List<RequestedItemVM> itemList)
        {
            var source = _transContext.SaveTransaction(entryId, itemList);

            //var source = new ItemResponseViewmodel();
            //var transaction = new Transaction();
            //string message = "";
            //try
            //{
            //    var _entryId = long.Parse(entryId);
            //    var trans = _context.ICS_Transaction.FirstOrDefault(x => x.Entry_Id == _entryId);

            //    var _trans = _contextPda.Transactions.OrderByDescending(x => x.TransactionId).FirstOrDefault();

            //    transaction.TransactionId = _trans != null ? _trans.TransactionId + 1 : 1;
            //    transaction.EntryId = trans.Entry_Id;
            //    transaction.Locat_Cd = trans.Locat_Cd;
            //    transaction.RequestedDate = trans.Voucher_Date;
            //    transaction.TransDate = DateTime.Now;
            //    transaction.IsCompleted = false;

            //    _contextPda.Transactions.Add(transaction);
            //    _contextPda.SaveChanges();

            //    var transactionDetails = new List<TransactionDetail>();
            //    var wishList = new List<WishList>();

            //    message = "trans and wishlist created";
            //    foreach (var item in itemList)
            //    {
            //        message = "entered in itemlist";
            //        if (item.ActQty > 0)
            //        {
            //            if (item.Qty - item.ActQty <= 0)
            //            {
            //                transactionDetails.Add(new TransactionDetail
            //                {
            //                    Entry_Id = trans.Entry_Id,
            //                    ActualQty = item.ActQty,
            //                    NameAr = item.NameAr,
            //                    NameEn = item.NameEn,
            //                    Prod_Cd = item.Prod_Cd,
            //                    Qty = item.Qty,
            //                    RemainingQty = 0,
            //                    RequestedDate = trans.Voucher_Date,
            //                    TransactionOid = transaction.Oid,
            //                    IsDelivered = false,
            //                    Line_No = item.LineNo
            //                });
            //            }
            //            else if (item.Qty != item.ActQty)
            //            {
            //                var remQty = item.Qty - item.ActQty;
            //                transactionDetails.Add(new TransactionDetail
            //                {
            //                    Entry_Id = trans.Entry_Id,
            //                    ActualQty = item.ActQty,
            //                    NameAr = item.NameAr,
            //                    NameEn = item.NameEn,
            //                    Prod_Cd = item.Prod_Cd,
            //                    Qty = item.IsNewlyAdded ? item.ActQty : item.Qty,
            //                    RemainingQty = item.IsNewlyAdded ? 0 : remQty,
            //                    RequestedDate = trans.Voucher_Date,
            //                    TransactionOid = transaction.Oid,
            //                    IsDelivered = false,
            //                    Line_No = item.LineNo
            //                });

            //                if (!item.IsNewlyAdded)
            //                    wishList.Add(new WishList
            //                    {
            //                        EntryId = trans.Entry_Id,
            //                        Prod_Cd = item.Prod_Cd,
            //                        NameEn = item.NameEn,
            //                        NameAr = item.NameAr,
            //                        RemainingQty = remQty,
            //                        RequestedDate = trans.Voucher_Date,
            //                        RequestedQty = item.Qty,
            //                        TransactionOid = transaction.Oid,
            //                        TransDetailOid = item.LineNo
            //                    });
            //            }
            //        }
            //        else
            //        {
            //            wishList.Add(new WishList
            //            {
            //                EntryId = trans.Entry_Id,
            //                Prod_Cd = item.Prod_Cd,
            //                RemainingQty = item.Qty,
            //                RequestedDate = trans.Voucher_Date,
            //                RequestedQty = item.Qty,
            //                TransactionOid = transaction.Oid,
            //                TransDetailOid = item.LineNo,
            //                NameEn = item.NameEn,
            //                NameAr = item.NameAr
            //            });
            //        }
            //    }

            //    _contextPda.TransactionDetails.AddRange(transactionDetails);
            //    _contextPda.WishLists.AddRange(wishList);
            //    message = "Saving PDA Trans Detail And Wishlist Detail";
            //    message = JsonConvert.SerializeObject(transactionDetails);
            //    _contextPda.SaveChanges();

            //    source.ReponseId = 3;
            //    source.Message = "Transaction Saved Successfully! ";
            //    message = "PDA Transaction Saved Successful";

            //    var innovaTransDetail = _context.ICS_Transaction_Details.Where(x => x.Entry_Id == _entryId);
            //    foreach (var transDet in transactionDetails)
            //    {
            //        var innTransDet = innovaTransDetail.FirstOrDefault(x => x.Line_No == transDet.Line_No);
            //        if (innTransDet != null)
            //            innTransDet.Qty = transDet.ActualQty;
            //    }

            //    foreach (var wList in wishList)
            //    {
            //        if (wList.RequestedQty - wList.RemainingQty == 0)
            //        {
            //            var innTransDet = innovaTransDetail.FirstOrDefault(x => x.Line_No == wList.TransDetailOid);
            //            if (innTransDet != null)
            //                _context.ICS_Transaction_Details.Remove(innTransDet);
            //        }
            //        else
            //        {
            //            var innTransDet = innovaTransDetail.FirstOrDefault(x => x.Line_No == wList.TransDetailOid);
            //            if (innTransDet != null)
            //                innTransDet.Qty = wList.RequestedQty - wList.RemainingQty;
            //        }
            //    }

            //    message = "An Attempt to Save In Innova Failed.";
            //    _context.SaveChanges();
            //    source.ReponseId = 2;
            //    source.Message = "Transaction Saved Successfully! Changes Saved in System, ";
            //    message = "Transaction Saved Successfully In Innova and Changes Saved in System, ";
            //    transaction.IsCompleted = true;
            //    message = "Transaction Saved Successfully In Innova and Changes Saved in System, Error Occurred While Marking in the system that saving is done.";
            //    _contextPda.SaveChanges();
            //    source.ReponseId = 1;
            //    source.Message = "Transaction Saved Successfully! Changes Saved in System, Transaction Completed!";
            //    message = "Transaction Saved Successfully! Changes Saved in System, Transaction Completed!";

            //    SendEmail(trans.Voucher_No, trans.Locat_Cd);
            //}
            //catch (Exception ex)
            //{
            //    source.ReponseId = 0;
            //    source.Message = ex.InnerException.Message + " : : " + message;
            //}

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        //private void SendEmail(long voucher_No, short locat_Cd)
        //{
        //    var locationNameAr = _context.SM_Location.FirstOrDefault(x => x.Locat_Cd == locat_Cd).A_Locat_Name;
        //    var settings = _contextPda.EmailSettings.Where(x => x.IsRegForEmail);
        //    var htmlString = $"Request has been created <br /> Request Number : <b>{voucher_No}</b> <br /> Location : <b>{locationNameAr}</b>";

        //    MailMessage mailMessage = new MailMessage
        //    {
        //        Subject = $"Request Created : {voucher_No}",
        //        Body = htmlString,
        //        IsBodyHtml = true,
        //        From = new MailAddress("imillmaterialreq@gmail.com"),
        //    };

        //    foreach (var setting in settings)
        //        mailMessage.To.Add(new MailAddress(setting.Emails));

        //    SmtpClient smtp = new SmtpClient
        //    {
        //        Host = "smtp.gmail.com",
        //        Port = 587,
        //        EnableSsl = true,
        //        DeliveryMethod = SmtpDeliveryMethod.Network,
        //        UseDefaultCredentials = false,
        //        Credentials = new NetworkCredential("imillmaterialreq@gmail.com", "M@ter!alReq$t")
        //    };

        //    smtp.Send(mailMessage);
        //}

        public ActionResult DeliveryRequest()
        {
            //var transactionVMs = new List<TransactionVM>();
            //var trans = _contextPda.Transactions.ToList();
            //var location = _context.SM_Location.ToList();

            //foreach (var tran in trans)
            //{
            //    transactionVMs.Add(
            //        new TransactionVM
            //        {
            //            Oid = tran.Oid,
            //            EntryId = tran.EntryId,
            //            TransDate = tran.RequestedDate,
            //            Locat_Cd = location.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd).Locat_Cd,
            //            LocationNameEn = location.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd).L_Locat_Name,
            //            LocationNameAr = location.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd).A_Locat_Name,
            //            IsCompleted = tran.IsCompleted
            //        });
            //}

            ViewBag.DataSource = _transContext.GetDeliveryRequest();

            return View();
        }

        public ActionResult GetRequestDetails(long oid)
        {
            //var items = _context.ICS_Item.ToList();
            //var trans = _contextPda.Transactions.FirstOrDefault(x => x.Oid == oid);

            //var modelList = new List<RequestedItemVM>();
            //var transDetails = _contextPda.TransactionDetails.Where(x => x.TransactionOid == trans.Oid).ToList();

            //foreach (var det in transDetails)
            //{
            //    modelList.Add(new RequestedItemVM
            //    {
            //        Oid = det.Oid,
            //        Prod_Cd = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Prod_Cd,
            //        PartNumber = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Part_No,
            //        NameEn = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).L_Prod_Name,
            //        NameAr = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).A_Prod_Name,
            //        Qty = det.Qty,
            //        RequestedDate = trans.RequestedDate,
            //        ActQty = det.ActualQty,
            //        IsDelivered = det.IsDelivered
            //    });
            //}

            ViewBag.DataSource = _transContext.GetRequestDetails(oid);
            return View();
        }

        public ActionResult SaveDeliveryRequest(List<RequestedItemVM> itemList)
        {
            //var source = new ItemResponseViewmodel();
            //long entryId = 0;
            //try
            //{
            //    var trans = _contextPda.TransactionDetails.ToList();

            //    foreach (var item in itemList)
            //    {
            //        if (item.IsVerified)
            //        {
            //            var transDetails = trans.FirstOrDefault(x => x.Oid == item.Oid);
            //            transDetails.DeliveredQty = item.ActQty;
            //            transDetails.DeliveryDate = DateTime.Now;
            //            transDetails.IsDelivered = true;
            //        }
            //    }

            //    _contextPda.SaveChanges();

            //    source.ReponseId = 1;
            //    source.Message = "Transaction Saved Successful";
            //}
            //catch (Exception ex)
            //{
            //    source.ReponseId = 2;
            //    source.Message = ex.Message;
            //    throw;
            //}

            //try
            //{
            //    var transDetails = _contextPda.TransactionDetails.Where(x => x.Entry_Id == entryId);
            //    if (!transDetails.Any(x => !x.IsDelivered))
            //    {
            //        _contextPda.Transactions.FirstOrDefault(x => x.EntryId == entryId).IsCompleted = true;
            //        _context.SaveChanges();
            //    }

            //}
            //catch (Exception)
            //{

            //    throw;
            //}

            var source = _transContext.SaveDeliveryRequest(itemList);

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        public ActionResult WishList()
        {
            //var transactionVMs = new List<TransactionVM>();
            //var wishlistTransOid = _contextPda.WishLists.Select(x => x.TransactionOid).ToList();
            //var trans = _contextPda.Transactions.Where(x => wishlistTransOid.Contains(x.Oid)).ToList();
            //var location = _context.SM_Location.ToList();

            //foreach (var tran in trans)
            //{
            //    transactionVMs.Add(
            //        new TransactionVM
            //        {
            //            Oid = tran.Oid,
            //            EntryId = tran.EntryId,
            //            TransDate = tran.RequestedDate,
            //            Locat_Cd = location.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd).Locat_Cd,
            //            LocationNameEn = location.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd).L_Locat_Name,
            //            LocationNameAr = location.FirstOrDefault(x => x.Locat_Cd == tran.Locat_Cd).A_Locat_Name,
            //            IsCompleted = tran.IsCompleted
            //        });
            //}

            ViewBag.DataSource = _transContext.Wishlist();

            return View();
        }

        public ActionResult GetWishlistDetails(long entryId)
        {
            //var items = _context.ICS_Item.ToList();
            //var trans = _contextPda.Transactions.FirstOrDefault(x => x.EntryId == entryId);
            //var wishlist = _contextPda.WishLists.Where(x => x.EntryId == entryId && x.RemainingQty > 0).ToList();
            //var modelList = new List<RequestedItemVM>();

            //foreach (var det in wishlist)
            //{
            //    modelList.Add(new RequestedItemVM
            //    {
            //        Oid = det.Oid,
            //        Prod_Cd = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Prod_Cd,
            //        PartNumber = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).Part_No,
            //        NameEn = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).L_Prod_Name,
            //        NameAr = items.FirstOrDefault(x => x.Prod_Cd == det.Prod_Cd).A_Prod_Name,
            //        Qty = det.RemainingQty,
            //        RequestedDate = trans.RequestedDate,
            //        DeliveryDate = trans.RequestedDate,
            //        ActQty = 0,
            //        Locat_Cd = trans.Locat_Cd
            //    });
            //}

            //ViewBag.DataSource = modelList.OrderByDescending(x => x.RequestedDate).ToList();

            ViewBag.DataSource = _transContext.GetWishlistDetails(entryId);
            ViewBag.EntryId = entryId;
            return View();
        }

        public ActionResult UpdateWishListRequest(List<RequestedItemVM> itemList)
        {
            var source = _transContext.UpdateWishListRequest(itemList);
            //var locat_Cd = itemList.FirstOrDefault().Locat_Cd;
            //var today = DateTime.Now.Date;
            //var transInnova = _context.ICS_Transaction.FirstOrDefault(x => x.Locat_Cd == locat_Cd && x.Voucher_Date >= today && x.Voucher_Date <= today);

            //if (transInnova != null)
            //{
            //    source.ReponseId = 2;
            //    source.Message = "No transaction for the location found!";
            //}

            //var transEntryId = transInnova.Entry_Id;
            //var itemUnitDetails = _context.ICS_Item_Unit_Details.ToList();
            //var items = _context.ICS_Item.ToList();
            //var remWishItem = new List<WishList>();

            //try
            //{
            //    var wishlist = _contextPda.WishLists.ToList();
            //    foreach (var item in itemList)
            //    {
            //        var itemUnitDetail = itemUnitDetails.FirstOrDefault(x => x.Prod_Cd == item.Prod_Cd);
            //        if (item.IsVerified)
            //        {
            //            var _wishlist = wishlist.FirstOrDefault(x => x.Oid == item.Oid);
            //            var transDetail = new ICS_Transaction_Details
            //            {
            //                Entry_Id = transEntryId,
            //                Prod_Cd = _wishlist.Prod_Cd,
            //                Qty = _wishlist.RemainingQty,
            //                Unit_Entry_ID = itemUnitDetail.Unit_Entry_Id,
            //                Exchange_Rate = 1,
            //                Prod_Name = items.FirstOrDefault(x => x.Prod_Cd == item.Prod_Cd).A_Prod_Name,
            //                Base_Qty = itemUnitDetail.Base_Qty,
            //                Processed_Qty = 0,
            //                FC_Rate = 0,
            //                FC_Prod_Dis = 0,
            //                FC_Amount = 0,
            //                Prod_Exp_Amt = 0,
            //                Promo_Type = "R",
            //                Item_Cost = 0,
            //                Dis_In_Percent = false,
            //                Linked = false,
            //                Show = true,
            //                GL_Post_Sequence_Counter = 0,
            //                Status = 2,
            //                User_Date_Time = DateTime.Now,
            //                row_id = new Guid(),
            //                Non_Stock_Manual_Cost = 0,
            //                Item_Type_Cd = items.FirstOrDefault(x => x.Prod_Cd == item.Prod_Cd).Item_Type_Cd,
            //                Is_Dented = false,
            //                Weight = 0,
            //                Processed_SQ_Base_Qty = 0,
            //                Processed_PO_Base_Qty = 0,
            //                Processed_GRN_Base_Qty = 0,
            //                Processed_CQ_Base_Qty = 0,
            //                Processed_SO_Base_Qty = 0,
            //                Processed_INV_Base_Qty = 0,
            //                Processed_CSR_Base_Qty = 0,
            //                Processed_SR_Base_Qty = 0,
            //                Processed_STS_Base_Qty = 0,
            //                Processed_ESTK_Base_Qty = 0,
            //                Processed_SSTK_Base_Qty = 0,
            //                Processed_Others_Base_Qty = 0,
            //                Fc_Prod_Dis_Per = 0,
            //                Commission_Amt = 0,
            //                master_discount = 0
            //            };

            //            _context.ICS_Transaction_Details.Add(transDetail);
            //            remWishItem.Add(_wishlist);
            //        }
            //    }

            //    _context.SaveChanges();
            //    _contextPda.WishLists.RemoveRange(remWishItem);
            //    _contextPda.SaveChanges();

            //    source.ReponseId = 1;
            //    source.Message = "Transaction Successful!";
            //}
            //catch (Exception ex)
            //{
            //    source.ReponseId = 2;
            //    source.Message = ex.Message;
            //}

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPost]
        public ContentResult CompleteInnovaTransaction()
        {
            var source = _transContext.CompleteInnovaTransaction();
            //var transactions = _contextPda.Transactions.Where(x => !x.IsCompleted).ToList();
            //var transDetail = _contextPda.TransactionDetails.ToList();

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        //private bool CheckEmptyTransactions()
        //{
        //    try
        //    {
        //        var transactions = _contextPda.Transactions.Where(x => !x.IsCompleted);
        //        var transDetail = _contextPda.TransactionDetails;

        //        if (transactions.Any())
        //        {
        //            foreach (var rec in transactions)
        //            {
        //                if (!transDetail.Any(x => x.TransactionOid == rec.Oid))
        //                {
        //                    _contextPda.Transactions.Remove(rec);
        //                    _contextPda.SaveChanges();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        public ContentResult AddToInnovaRequest(string partNumber, string weight, string entryId)
        {
            var source = _transContext.AddToInnovaRequest(partNumber, weight, entryId);

            //string message = "";

            //try
            //{
            //    var _entryId = int.Parse(entryId);
            //    var _weight = string.IsNullOrEmpty(weight) ? 1 : decimal.Parse(weight);
            //    var transaction = _context.ICS_Transaction.FirstOrDefault(x => x.Entry_Id == _entryId);
            //    if (transaction != null)
            //    {
            //        var item = _context.ICS_Item.FirstOrDefault(x => x.Part_No == partNumber);
            //        var unit_Entry_Id = _context.ICS_Item_Unit_Details.FirstOrDefault(x => x.Prod_Cd == item.Prod_Cd).Unit_Entry_Id;

            //        if (item != null)
            //        {
            //            var transDetail = new ICS_Transaction_Details
            //            {
            //                Entry_Id = transaction.Entry_Id,
            //                Add_Locat_Cd = transaction.Locat_Cd,
            //                Prod_Cd = item.Prod_Cd,
            //                Unit_Entry_ID = unit_Entry_Id,
            //                Exchange_Rate = 1,
            //                Prod_Name = item.A_Prod_Name,
            //                Qty = _weight,
            //                Base_Qty = _weight,
            //                Processed_Qty = 0,
            //                FC_Rate = 0,
            //                FC_Prod_Dis = 0,
            //                FC_Amount = 0,
            //                Prod_Exp_Amt = 0,
            //                Promo_Type = "R",
            //                Item_Cost = 0,
            //                Dis_In_Percent = false,
            //                Linked = false,
            //                Show = false,
            //                GL_Post_Sequence_Counter = 0,
            //                Status = 2,
            //                User_Date_Time = DateTime.Now,
            //                row_id = new Guid(),
            //                Non_Stock_Manual_Cost = 0,
            //                Item_Type_Cd = 1,
            //                Is_Dented = false,
            //                Weight = 0,
            //                Processed_SQ_Base_Qty = 0,
            //                Processed_PO_Base_Qty = 0,
            //                Processed_GRN_Base_Qty = 0,
            //                Processed_CQ_Base_Qty = 0,
            //                Processed_SO_Base_Qty = 0,
            //                Processed_INV_Base_Qty = 0,
            //                Processed_CSR_Base_Qty = 0,
            //                Processed_SR_Base_Qty = 0,
            //                Processed_STS_Base_Qty = 0,
            //                Processed_ESTK_Base_Qty = 0,
            //                Processed_SSTK_Base_Qty = 0,
            //                Processed_Others_Base_Qty = 0,
            //                Fc_Prod_Dis_Per = 0,
            //                Commission_Amt = 0,
            //                master_discount = 0
            //            };

            //            _context.ICS_Transaction_Details.Add(transDetail);
            //            _context.SaveChanges();

            //            source.Oid = item.Prod_Cd;
            //            source.Weight = _weight;
            //            source.CheckDigit = "NA";
            //            source.Identifier = "NA";
            //            source.PartNumber = item.Part_No;
            //            source.Message = "Transaction Successful";
            //            source.ResponseId = 1;
            //            source.ItemNameEn = item.L_Prod_Name;
            //            source.ItemNameAr = item.A_Prod_Name;
            //            source.RequestedDate = transaction.Voucher_Date;
            //            source.LineNo = transDetail.Line_No;
            //            source.Locat_Cd = transaction.Locat_Cd;
            //            source.Prod_Cd = item.Prod_Cd;
            //        }
            //        else
            //        {
            //            source.ResponseId = 0;
            //            source.Message = $"Item Not Found with part number : {partNumber}";
            //        }
            //    }
            //    else
            //    {
            //        source.ResponseId = 0;
            //        source.Message = $"Id doesn't match any transaction | {entryId}";
            //    }
            //}
            //catch (Exception ex)
            //{
            //    source.ResponseId = 0;
            //    source.Message = ex.InnerException.Message + " : : " + message;
            //}

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;

        }

        public ContentResult UpdateQuantity(string partNumber, int entryId, int qty)
        {
            var source = _transContext.UpdateQuantity(partNumber, entryId, qty);

            //try
            //{
            //    var item = _context.ICS_Item.FirstOrDefault(x => x.Part_No == partNumber);
            //    var transDetail = _context.ICS_Transaction_Details.FirstOrDefault(x => x.Entry_Id == entryId && x.Prod_Cd == item.Prod_Cd);
            //    transDetail.Qty = qty;
            //    _context.SaveChanges();

            //    source.ResponseId = 1;
            //    source.Message = "Updated Successfully";
            //}
            //catch (Exception ex)
            //{
            //    source.ResponseId = 0;
            //    source.Message = ex.InnerException.Message;
            //}

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPost]
        public ContentResult DraftTransactions(string entryId, List<RequestedItemVM> itemList)
        {
            var source = _transContext.DraftTransactions(entryId, itemList);

            //string message = "";

            //try
            //{
            //    var openTransactions = new List<OpenTransaction>();
            //    var _entryId = int.Parse(entryId);
            //    var dbOpenTrans = _contextPda.OpenTransactions.Where(x => x.EntryId == _entryId).ToList();

            //    foreach (var item in itemList)
            //    {
            //        if (item.IsVerified)
            //        {
            //            if (dbOpenTrans.Any(x => x.Prod_Cd == item.Prod_Cd && x.PartNumber == item.PartNumber))
            //            {
            //                message = $"Changing Transaction in PDA Prod : {item.Prod_Cd}";
            //                var dbOpenTran = dbOpenTrans.FirstOrDefault(x => x.Prod_Cd == item.Prod_Cd && x.PartNumber == item.PartNumber);
            //                dbOpenTran.RequestedQty = item.Qty;
            //                dbOpenTran.ActualQty = item.ActQty;
            //                dbOpenTran.RequiredQty = item.OrgQty;
            //                dbOpenTran.IsNewlyAdded = item.IsNewlyAdded;
            //                dbOpenTran.IsReqQtyChanged = item.IsReqQtyChanged;
            //            }
            //            else
            //            {
            //                message = $"Adding Transaction in PDA Prod : {item.Prod_Cd}";
            //                openTransactions.Add(new OpenTransaction
            //                {
            //                    EntryId = _entryId,
            //                    Locat_Cd = item.Locat_Cd,
            //                    Prod_Cd = item.Prod_Cd,
            //                    PartNumber = item.PartNumber,
            //                    RequestedQty = item.Qty,
            //                    ActualQty = item.ActQty,
            //                    RequiredQty = item.OrgQty,
            //                    IsNewlyAdded = item.IsNewlyAdded,
            //                    IsReqQtyChanged = item.IsReqQtyChanged,
            //                    CreatedOn = DateTime.Now
            //                });
            //            }
            //        }
            //        else
            //        {
            //            if (dbOpenTrans.Any(x => x.Prod_Cd == item.Prod_Cd && x.PartNumber == item.PartNumber))
            //            {
            //                var dbOpenTran = dbOpenTrans.FirstOrDefault(x => x.Prod_Cd == item.Prod_Cd && x.PartNumber == item.PartNumber);
            //                _contextPda.OpenTransactions.Remove(dbOpenTran);
            //            }
            //        }
            //    }

            //    _contextPda.OpenTransactions.AddRange(openTransactions);
            //    message = "saving Transaction in PDA";
            //    _contextPda.SaveChanges();
            //    source.ReponseId = 1;
            //    source.Message = "Transaction Saved Successfully!";
            //    message = "Transaction Saved Successfully!";
            //}
            //catch (Exception ex)
            //{
            //    source.ReponseId = 0;
            //    source.Message = ex.InnerException.Message + " : : " + message;
            //}

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPost]
        public ContentResult DeleteOpenTransaction(string entryId)
        {
            var source = _transContext.DeleteOpenTransaction(entryId);

            //string message = "";

            //try
            //{
            //    var openTransactions = new List<OpenTransaction>();
            //    var _entryId = int.Parse(entryId);
            //    var dbOpenTrans = _contextPda.OpenTransactions.Where(x => x.EntryId == _entryId).ToList();
            //    _contextPda.OpenTransactions.RemoveRange(dbOpenTrans);
            //    _contextPda.SaveChanges();
            //    source.ReponseId = 1;
            //    source.Message = "Transaction Deleted Successfully!";
            //    message = "Transaction Deleted Successfully!";
            //}
            //catch (Exception ex)
            //{
            //    source.ReponseId = 0;
            //    source.Message = ex.InnerException.Message + " : : " + message;
            //}

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        public ActionResult ConsolidatedReport()
        {
            ViewBag.startDate = DateTime.Now;

            var products = _productRepository.GetAllProducts().Items;
            ViewBag.products = products.OrderBy(x => x.Name);

            var groups = _productRepository.GetItemGroups();
            ViewBag.groups = groups.OrderBy(x => x.GroupNumber);

            ViewBag.ConsReportType = new List<ConsolidatedReportType> { 
                new ConsolidatedReportType
                {
                    Id = 1,
                    NameEn = "Location"
                },
                new ConsolidatedReportType
                {
                    Id = 2,
                    NameEn = "Item"
                }
            };

            return View();
        }

        public ContentResult GetConsolidatedItems(string from, string product, string productAr, bool isChecked, string group, string type)
        {
            try
            {
                var fromDate = DateTime.Now;
                var productIds = isChecked ? product : productAr;

                if (!string.IsNullOrEmpty(from))
                    fromDate = DateTime.Parse(from);

                ViewBag.startDate = fromDate;
                ViewBag.validation = "false";

                var source = _transContext.GetConsolidatedItems(fromDate, productIds, group, type);

                var result = new ContentResult
                {
                    Content = JsonConvert.SerializeObject(source),
                    ContentType = "application/json"
                };

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public ContentResult GetProducts(List<long> groupIds)
        {
            var source = _transContext.GetGroupProduct(groupIds);

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        public ContentResult SaveConsolidateTrans(string from, List<ConsolidatedItems> consolidatedItems)
        {

            if (!string.IsNullOrEmpty(from))
            {
                var fromDate = DateTime.Parse(from);
                var source = _transContext.SaveConsTrans(fromDate, consolidatedItems);

                var result = new ContentResult
                {
                    Content = JsonConvert.SerializeObject(source),
                    ContentType = "application/json"
                };

                return result;
            }
            return null;
        }
    }
}