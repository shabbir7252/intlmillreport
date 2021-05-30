using ImillReports.Contracts;
using ImillReports.Models;
using ImillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Net.Mail;
using System.Net;

namespace ImillReports.Repository
{
    public class PurchaseOrderRepo : IPurchaseOrderRepo
    {
        private readonly IMILLEntities _context;
        private readonly ILocationRepository _locationRepository;
        private readonly ImillReportsEntities _report;
        private readonly ISalesReportRepository _salesReportRepository;

        public PurchaseOrderRepo(IMILLEntities context, ILocationRepository locationRepository,
            ImillReportsEntities report, ISalesReportRepository salesReportRepository)
        {
            _report = report;
            _context = context;
            _locationRepository = locationRepository;
            _salesReportRepository = salesReportRepository;
        }

        public List<TransDetailsViewModel> GetDetails(long entryId, DateTime recordDate)
        {
            var trans = _context.ICS_Transaction.Include(a => a.GL_Ledger).FirstOrDefault(x => x.Entry_Id == entryId);

            // var transDetails = _context.ICS_Transaction_Details.Where(x => x.Entry_Id == trans.Entry_Id).ToList();

            var voucher_date = trans.Voucher_Date;
            var to = new DateTime(voucher_date.Year, voucher_date.Month, voucher_date.Day, 00, 00, 00);
            var from = new DateTime(voucher_date.Year, voucher_date.Month, voucher_date.Day, 23, 59, 59);

            // var transDetails = _salesReportRepository.GetSalesDetailTransaction(to, from,"", "","");

            var entryIdArray = new List<string>
            {
                entryId.ToString()
            };

            var entryIdString = entryIdArray != null && entryIdArray.Count > 0 ? string.Join(",", entryIdArray) : "";
            var transactionDetails = _context.spICSTransDetail_GetAll(entryIdString, "").ToList();
            var items = _context.ICS_Item.ToList();

            var model = new List<TransDetailsViewModel>();

            foreach (var detail in transactionDetails)
            {
                var package = detail.Package;
                var qty = detail.Qty;
                var packQty = qty / package;

                var transDetailsViewModel = new TransDetailsViewModel
                {
                    EntryId = detail.Entry_Id,
                    Part_Number = items.FirstOrDefault(x => x.Prod_Cd == detail.ProdId).Part_No,
                    Voucher = trans.Voucher_Type.ToString(),
                    InvDateTime = trans.Voucher_Date,
                    CustomerId = trans.GL_Ledger2 != null ? trans.GL_Ledger2.Ldgr_Cd : 0,
                    CustomerName = trans.GL_Ledger2 != null ? trans.GL_Ledger2.L_Ldgr_Name : "",
                    CustomerNameAr = trans.GL_Ledger2 != null ? trans.GL_Ledger2.A_Ldgr_Name : "",
                    ProdId = detail.ProdId,
                    ProductNameEn = detail.ProdEn,
                    ProductNameAr = detail.ProdAr,
                    BaseQuantity = detail.IUDBaseQty,
                    BaseUnit = detail.BaseUnit,
                    BaseUnitId = detail.BaseUnitId,
                    UnitPrice = detail.UnitPrice,
                    SellQuantity = detail.Qty,
                    SellUnit = detail.SellUnit,
                    SellUnitId = detail.SellUnitId,
                    Package = detail.Package,
                    PackQty = packQty,
                    AltQty = detail.AltQty,
                    Amount = trans.Voucher_Type == 202 ||
                               trans.Voucher_Type == 2023 ||
                               trans.Voucher_Type == 2035 ||
                               trans.Voucher_Type == 2036 ||
                               trans.Voucher_Type == 2037
                                ? -detail.FC_Amount
                                : detail.FC_Amount,
                    Discount = trans.Voucher_Type == 202 ||
                               trans.Voucher_Type == 2023 ||
                               trans.Voucher_Type == 2035 ||
                               trans.Voucher_Type == 2036 ||
                               trans.Voucher_Type == 2037
                                ? -detail.FC_Prod_Dis
                                : detail.FC_Prod_Dis,
                    Comments = trans.Comments
                };

                model.Add(transDetailsViewModel);
            }

            return model;
        }

        public List<PurchaseOrderViewModel> GetPurchaseOrders(DateTime? fromDate, DateTime? toDate, string username)
        {
            try
            {
                var transactions = username == "shareef" 
                    ? _context.ICS_Transaction.Where(x => x.Voucher_Type == 105 && x.Voucher_Date >= fromDate && x.Voucher_Date <= toDate).OrderByDescending(x => x.Voucher_Type).ToList()
                    : _context.ICS_Transaction.Where(x => (x.Voucher_Type == 101 || x.Voucher_Type == 105) && x.Voucher_Date >= fromDate && x.Voucher_Date <= toDate).OrderByDescending(x => x.Voucher_Type).ToList();

                var entryIds = transactions.Select(x => x.Entry_Id).ToList();
                var POTransactions = _report.PurchaseOrders.Where(x => entryIds.Contains(x.EntryId)).ToList();
                var transType = _context.ICS_Transaction_Types.ToList();

                var models = new List<PurchaseOrderViewModel>();
                var voucherNos = new List<long>();
                foreach (var trans in transactions)
                {
                    var location = _locationRepository.GetLocations();
                    var poTrans = POTransactions.FirstOrDefault(x => x.EntryId == trans.Entry_Id);
                    var voucher_type = trans.Voucher_Type;
                    var lpoStatus = 0;
                    var lpoStatusString = "Pending";

                    if (poTrans != null)
                    {
                        if (poTrans.LpoStatus == 1 || poTrans.LpoStatus == 2)
                        {
                            lpoStatus = poTrans.LpoStatus;
                            if (lpoStatus == 1)
                            {
                                lpoStatusString = "Approved";
                            }
                            else
                            {
                                lpoStatusString = "Reject";
                            }
                        }
                        //else
                        //{
                        //    if(voucher_type == 105)
                        //    {
                        //        lpoStatus = 1;
                        //        lpoStatusString = "Approved";
                        //    }
                        //}
                    }
                    //else
                    //{
                    //    if (voucher_type == 105)
                    //    {
                    //        lpoStatus = 1;
                    //        lpoStatusString = "Approved";
                    //    }
                    //}



                    var invoiceCheckTrans = transactions.FirstOrDefault(x => x.Ldgr_Cd == trans.Ldgr_Cd && x.Voucher_Amt_FC == trans.Voucher_Amt_FC && x.Voucher_Type == 101 && x.Voucher_Date >= trans.Voucher_Date);

                    if (voucher_type == 105 && lpoStatus == 1 && invoiceCheckTrans != null && !voucherNos.Any(x => x == trans.Voucher_No))
                    {
                        voucherNos.Add(trans.Voucher_No);
                        voucherNos.Add(invoiceCheckTrans.Voucher_No);
                        var invPoTrans = POTransactions.FirstOrDefault(x => x.EntryId == invoiceCheckTrans.Entry_Id);

                        var purchaseOrder = new PurchaseOrderViewModel()
                        {
                            EntryId = invoiceCheckTrans.Entry_Id,
                            LdgrCd = invoiceCheckTrans.Ldgr_Cd ?? 0,
                            LocatCd = invoiceCheckTrans.Locat_Cd,
                            CustomerId = invoiceCheckTrans.GL_Ledger2 != null ? invoiceCheckTrans.GL_Ledger2.Ldgr_Cd : 0,
                            CustomerName = invoiceCheckTrans.GL_Ledger2 != null ? invoiceCheckTrans.GL_Ledger2.L_Ldgr_Name : "",
                            CustomerNameAr = invoiceCheckTrans.GL_Ledger2 != null ? invoiceCheckTrans.GL_Ledger2.A_Ldgr_Name : "",

                            LocationNameEn = location.LocationItems.FirstOrDefault(x => x.LocationId == invoiceCheckTrans.Locat_Cd).Name,
                            LocationNameAr = location.LocationItems.FirstOrDefault(x => x.LocationId == invoiceCheckTrans.Locat_Cd).NameAr,

                            LpoStatus = lpoStatus,
                            LpoStatusString = lpoStatusString,
                            LpoStatusTransDate = poTrans != null && poTrans.LpoStatusTransDate.HasValue ? poTrans.LpoStatusTransDate.Value.ToString("dd-MM-yyyy hh:mm tt") : "",
                            GmComments = poTrans != null ? poTrans.GmComments : "",

                            LpoInvoiceStatus = 1,
                            LpoInvoiceString = "Arrived",

                            LpoPaymentStatus = invPoTrans != null ? invPoTrans.LpoPaymentStatus : 0,
                            LpoPaymentStatusString = invPoTrans != null ? invPoTrans.LpoPaymentStatus == 1 ? "Cheque Ready" : "Pending" : "Pending",
                            LpoPaymentTransDate = invPoTrans != null && invPoTrans.LpoPaymentTransDate.HasValue ? invPoTrans.LpoPaymentTransDate.Value.ToString("dd-MM-yyyy hh:mm tt") : "",
                            PaymentRemarks = invPoTrans != null ? invPoTrans.PaymentRemarks : "",

                            QAStatus = invPoTrans != null && invPoTrans.QAStatus != null ? invPoTrans.QAStatus.Value : 0,
                            QAStatusString = invPoTrans != null && invPoTrans.QAStatus != null ? invPoTrans.QAStatus.Value == 0 ? "Pending" : invPoTrans.QAStatus.Value == 1 ? "Approved" : invPoTrans.QAStatus.Value == 2 ? "Reject" : "Pending" : "Pending",
                            QATransDate = invPoTrans != null && invPoTrans.QAStatusTransDate.HasValue ? invPoTrans.QAStatusTransDate.Value.ToString("dd-MM-yyyy hh:mm tt") : "",
                            QARemarks = invPoTrans != null ? invPoTrans.QARemarks : "",


                            // UserDateTime = trans.User_Date_Time,
                            UserDateTime = invoiceCheckTrans.User_Date_Time,
                            // VoucherAmount = trans.Voucher_Amt_FC,
                            VoucherAmount = invoiceCheckTrans.Voucher_Amt_FC,
                            // VoucherDate = trans.Voucher_Date,
                            VoucherDate = invoiceCheckTrans.Voucher_Date,
                            // VoucherNumber = trans.Voucher_No,
                            VoucherNumber = invoiceCheckTrans.Voucher_No,
                            VoucherType = invoiceCheckTrans.Voucher_Type,
                            VoucherNameAr = transType.FirstOrDefault(x => x.Voucher_Type == invoiceCheckTrans.Voucher_Type).A_Voucher_Name,
                            VoucherNameEn = transType.FirstOrDefault(x => x.Voucher_Type == invoiceCheckTrans.Voucher_Type).L_Voucher_Name,
                            Discount = invoiceCheckTrans.Discount_Amt_FC
                        };
                        models.Add(purchaseOrder);
                    }

                    else if (!voucherNos.Any(x => x == trans.Voucher_No))
                    {
                        voucherNos.Add(trans.Voucher_No);
                        var purchaseOrder = new PurchaseOrderViewModel()
                        {
                            EntryId = trans.Entry_Id,
                            LdgrCd = trans.Ldgr_Cd ?? 0,
                            LocatCd = trans.Locat_Cd,
                            CustomerId = trans.GL_Ledger2 != null ? trans.GL_Ledger2.Ldgr_Cd : 0,
                            CustomerName = trans.GL_Ledger2 != null ? trans.GL_Ledger2.L_Ldgr_Name : "",
                            CustomerNameAr = trans.GL_Ledger2 != null ? trans.GL_Ledger2.A_Ldgr_Name : "",

                            LocationNameEn = location.LocationItems.FirstOrDefault(x => x.LocationId == trans.Locat_Cd).Name,
                            LocationNameAr = location.LocationItems.FirstOrDefault(x => x.LocationId == trans.Locat_Cd).NameAr,

                            LpoStatus = lpoStatus,
                            LpoStatusString = lpoStatusString,

                            LpoInvoiceStatus = voucher_type == 101 ? 1 : 0,
                            LpoInvoiceString = voucher_type == 101 ? "Arrived" : "Not Arrived",

                            LpoPaymentStatus = poTrans != null ? poTrans.LpoPaymentStatus : 0,
                            LpoPaymentStatusString = poTrans != null ? poTrans.LpoPaymentStatus == 1 ? "Cheque Ready" : "Pending" : "Pending",

                            LpoStatusTransDate = poTrans != null && poTrans.LpoStatusTransDate.HasValue ? poTrans.LpoStatusTransDate.Value.ToString("dd-MM-yyyy hh:mm tt") : "",
                            LpoPaymentTransDate = poTrans != null && poTrans.LpoPaymentTransDate.HasValue ? poTrans.LpoPaymentTransDate.Value.ToString("dd-MM-yyyy hh:mm tt") : "",

                            QAStatus = poTrans != null && poTrans.QAStatus != null ? poTrans.QAStatus.Value : 0,
                            QAStatusString = poTrans != null && poTrans.QAStatus != null ? poTrans.QAStatus.Value == 0 ? "Pending" : poTrans.QAStatus.Value == 1 ? "Approved" : poTrans.QAStatus.Value == 2 ? "Reject" : "Pending" : "Pending",
                            QATransDate = poTrans != null && poTrans.QAStatusTransDate.HasValue ? poTrans.QAStatusTransDate.Value.ToString("dd-MM-yyyy hh:mm tt") : "",
                            QARemarks = poTrans != null ? poTrans.QARemarks : "",

                            PaymentRemarks = poTrans != null ? poTrans.PaymentRemarks : "",
                            GmComments = poTrans != null ? poTrans.GmComments : "",
                            UserDateTime = trans.User_Date_Time,
                            VoucherAmount = trans.Voucher_Amt_FC,
                            VoucherDate = trans.Voucher_Date,
                            VoucherNumber = trans.Voucher_No,
                            VoucherType = voucher_type,
                            VoucherNameAr = transType.FirstOrDefault(x => x.Voucher_Type == voucher_type).A_Voucher_Name,
                            VoucherNameEn = transType.FirstOrDefault(x => x.Voucher_Type == voucher_type).L_Voucher_Name,
                            Discount = trans.Discount_Amt_FC
                        };
                        models.Add(purchaseOrder);
                    }
                }

                return models.OrderByDescending(x => x.VoucherDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public bool UpdateLpoStatus(long oid, int ldgrCd, long entryId, DateTime transDate, int lpoStatus,
            int paymentStatus, int qaStatus, string gmcomment, string pmStatus, string qaRemarks, string username)
        {
            try
            {
                var sendEmail = false;
                var sendChequeReadyEmail = false;
                var emailPurchaseOrder = new PurchaseOrder();
                var dbPurchaseOrder = _report.PurchaseOrders.FirstOrDefault(x => x.LdgrCd == ldgrCd && x.EntryId == entryId && x.VoucherDate == transDate);

                if (dbPurchaseOrder != null)
                {
                    sendEmail = dbPurchaseOrder.LpoStatus == lpoStatus;
                    sendChequeReadyEmail = paymentStatus == 1;

                    if (dbPurchaseOrder.LpoStatus != lpoStatus)
                        dbPurchaseOrder.LpoStatusTransDate = (lpoStatus == 1 | lpoStatus == 2) ? DateTime.Now : DateTime.MinValue;

                    if (dbPurchaseOrder.LpoPaymentTransDate != null)
                    {
                        if (dbPurchaseOrder.LpoPaymentStatus != paymentStatus)
                            dbPurchaseOrder.LpoPaymentTransDate = (paymentStatus == 1 | paymentStatus == 2) ? DateTime.Now : DateTime.MinValue;
                    }
                    else
                    {
                        dbPurchaseOrder.LpoPaymentTransDate = DateTime.Now;
                    }

                    dbPurchaseOrder.LpoInvoiceStatus = 1;
                    dbPurchaseOrder.LpoPaymentStatus = paymentStatus;
                    dbPurchaseOrder.QAStatus = qaStatus;
                    dbPurchaseOrder.LpoStatus = lpoStatus;
                    dbPurchaseOrder.GmComments = gmcomment;
                    dbPurchaseOrder.PaymentRemarks = pmStatus;
                    dbPurchaseOrder.QARemarks = qaRemarks;

                    _report.SaveChanges();

                    emailPurchaseOrder = dbPurchaseOrder;
                }
                else
                {
                    var purchaseOrder = new PurchaseOrder
                    {
                        EntryId = entryId,
                        LdgrCd = ldgrCd,
                        VoucherDate = transDate,
                        LpoInvoiceStatus = 1,
                        LpoStatus = lpoStatus,
                        GmComments = gmcomment,
                        LpoStatusTransDate = DateTime.Now,
                        LpoPaymentStatus = paymentStatus,
                        PaymentRemarks = pmStatus,
                        LpoPaymentTransDate = DateTime.Now,
                        QAStatus = qaStatus,
                        QARemarks = qaRemarks,
                        QAStatusTransDate = DateTime.Now
                    };

                    _report.PurchaseOrders.Add(purchaseOrder);
                    _report.SaveChanges();

                    emailPurchaseOrder = purchaseOrder;
                }

                var trans = _context.ICS_Transaction.FirstOrDefault(x => x.Entry_Id == entryId && x.Voucher_Date == transDate && x.Ldgr_Cd == ldgrCd);
                trans.Comments = gmcomment;
                _context.SaveChanges();

                if (!sendEmail && username != "accounts" && username != "qader")
                    SendEmail(emailPurchaseOrder, trans);

                if (sendChequeReadyEmail && username == "accounts")
                    SendChequeReadyEmail(emailPurchaseOrder, trans);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void SendEmail(PurchaseOrder purchaseOrder, ICS_Transaction trans)
        {
            try
            {
                var setting = _report.Settings.FirstOrDefault();
                var lpoStatus = purchaseOrder.LpoStatus == 1 ? "Approved" : purchaseOrder.LpoStatus == 2 ? "Rejected" : "Pending";
                var customerNameAr = trans.GL_Ledger2 != null ? trans.GL_Ledger2.A_Ldgr_Name : "";

                var transDetails = GetDetails(trans.Entry_Id, trans.Voucher_Date);
                var midBodyString = "";

                foreach (var detail in transDetails)
                {
                    midBodyString += $"<tr>" +
                        $"<td style='border: 1px solid #dddddd; text-align: left'>({detail.Part_Number}) {detail.ProductNameAr}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.Package.Value.ToString("#0.000")}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.PackQty.Value.ToString("#0.000")}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.SellUnit}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.SellQuantity.Value:#0.000}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.UnitPrice.Value.ToString("#0.000")}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.Amount.Value.ToString("#0.000")}</td>" +
                                $"</tr>";
                }

                var firstBodyString = "<html><body>" +
                    "<a style='color:#FF33CD' href='http://192.168.250.2:70/ImillReports/PurchaseOrder/Index' target='_blank'>Click Here To Open Purchase Order</a>" +
                    "<h4>Date of Purchase Order : {0} </h4> " +
                    "<h4>LPO Number : {1} </h4> " +
                    "<h4>Supplier Name : {2} </h4> " +
                    "<h4>Total Amount : {3} </h4> " +
                    "<h4>LPO Status : <span style='background-color: {9}'>{4}</span></h4> " +
                    "<h4>Gm Comments : {5} </h4> " +
                    "<table style='min-width: 820px; width:100%'> <thead> " +
                    "<tr> " +
                    "<th style='text-align: left'>Description</th> " +
                    "<th style='text-align: left'>Pack Unit</th>" +
                    "<th style='text-align: left'>Pack Qty</th> " +
                    "<th style='text-align: left'>Unit</th>" +
                    "<th style='text-align: left'>Qty</th>" +
                    "<th style='text-align: left'>Price</th>" +
                    "<th style='text-align: left'>Amount</th> " +
                    "</tr></thead> <tbody>";
                var midBodyString2 = "<tr><td></td><td></td><td></td><td></td><td></td><td>Total Amount</td><td>{6}</td></tr><tr> <td></td><td></td><td></td><td></td><td></td><td>Discount</td><td>{7}</td></tr><tr> <td></td><td></td><td></td><td></td><td></td><td>Net Amount</td><td>{8}</td></tr>";
                string lastBodyString = "</tbody> </table></body></html>";
                string color = lpoStatus == "Approved" ? "#00ff00" : lpoStatus == "Rejected" ? "#ff0000" : "";
                var netAmount = trans.Voucher_Amt_FC - trans.Discount_Amt_FC;
                string htmlString = string.Format(firstBodyString + midBodyString + midBodyString2 + lastBodyString, trans.Voucher_Date.ToString("dd/MMM/yyyy"), trans.Voucher_No, customerNameAr, netAmount.ToString("#0.000"), lpoStatus, purchaseOrder.GmComments, trans.Voucher_Amt_FC.ToString("#0.000"), trans.Discount_Amt_FC.ToString("#0.000"), netAmount.ToString("#0.000"), color);


                MailMessage mailMessage = new MailMessage
                {
                    Subject = $"Purchase Order # {trans.Voucher_No} has been {lpoStatus}",
                    Body = htmlString,
                    IsBodyHtml = true,
                    From = new MailAddress("lpo.intlmill@gmail.com"),
                };

                mailMessage.To.Add(new MailAddress(setting.PurchaseEmail));

                if (!string.IsNullOrEmpty(setting.AdditionalCc1))
                    mailMessage.To.Add(new MailAddress(setting.AdditionalCc1));

                if (!string.IsNullOrEmpty(setting.AdditionalCc2))
                    mailMessage.To.Add(new MailAddress(setting.AdditionalCc2));

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("lpo.intlmill@gmail.com", "Intlmill2021")
                };

                smtp.Send(mailMessage);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool GetPurchaseEmailOrder(DateTime fromDate, DateTime toDate, string username)
        {
            try
            {
                var po = GetPurchaseOrders(fromDate, toDate, username).Where(x => x.VoucherType == 105);
                var dbPo = _report.PurchaseOrders.Where(x => x.VoucherDate >= fromDate && x.VoucherDate <= toDate);

                foreach (var _po in po)
                {
                    PurchaseOrder purchaseOrder = null;

                    if (dbPo != null)
                        purchaseOrder = dbPo.FirstOrDefault(x => x.EntryId == _po.EntryId && x.LdgrCd == _po.LdgrCd && x.VoucherDate == _po.VoucherDate);

                    if (purchaseOrder == null)
                    {
                        var emailPurchaseOrder = new PurchaseOrder
                        {
                            EntryId = _po.EntryId,
                            LdgrCd = _po.LdgrCd,
                            VoucherDate = _po.VoucherDate,
                            LpoStatus = _po.LpoStatus,
                            LpoInvoiceStatus = _po.LpoInvoiceStatus,
                            LpoPaymentStatus = _po.LpoPaymentStatus,
                            GmComments = _po.GmComments,
                            PaymentRemarks = _po.PaymentRemarks
                        };

                        _report.PurchaseOrders.Add(emailPurchaseOrder);
                        _report.SaveChanges();

                        var firstBodyString = "<html><body>" +
                            "<a style='color:#FF33CD' href='http://192.168.250.2:70/ImillReports/PurchaseOrder/Index' target='_blank'>Click Here To Open Purchase Order</a>" +
                            "<h4>Date of Purchase Order : {0} </h4> <h4>LPO Number : {1} </h4> <h4>Supplier Name : {2}</h4> <h4>Total Amount : {3}</h4> <h4>LPO Status : {4}</h4> <table style='min-width: 820px; width:100%'> <thead> " +
                            "<tr> " +
                            "<th style='text-align: left'>Description</th>" +
                            "<th style='text-align: left'>Pack Unit</th>" +
                            "<th style='text-align: left'>Pack Qty</th>" +
                            "<th style='text-align: left'>Unit</th>" +
                            "<th style='text-align: left'>Qty</th>" +
                            "<th style='text-align: left'>Price</th>" +
                            "<th style='text-align: left'>Amount</th>" +
                            "</tr></thead> <tbody>";
                        var midBodyString = "";
                        string lastBodyString = "</tbody></table></body></html>";

                        var transDetails = GetDetails(_po.EntryId, _po.VoucherDate);
                        var setting = _report.Settings.FirstOrDefault();

                        foreach (var detail in transDetails)
                        {
                            midBodyString += $"<tr>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>({detail.Part_Number}) {detail.ProductNameAr}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.Package.Value:#0.000}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.PackQty.Value:#0.000}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.SellUnit}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.SellQuantity.Value:#0.000}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.UnitPrice.Value:#0.000}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.Amount.Value:#0.000}</td>" +
                                $"</tr>";
                        }

                        var midBodyString2 = "<tr><td></td><td></td><td></td><td></td><td></td><td>Total Amount</td><td>{5}</td></tr><tr> <td></td><td></td><td></td><td></td><td></td><td>Discount</td><td>{6}</td></tr><tr> <td></td><td></td><td></td><td></td><td></td><td>Net Amount</td><td>{7}</td></tr>";
                        var netAmount = (_po.VoucherAmount.Value) - _po.Discount;
                        var htmlString = string.Format(firstBodyString + midBodyString + midBodyString2 + lastBodyString, _po.VoucherDate.ToString("dd/MMM/yyyy"), _po.VoucherNumber, _po.CustomerNameAr, netAmount.ToString("#0.000"), "Pending", _po.VoucherAmount.Value.ToString("#0.000"), _po.Discount.ToString("#0.000"), netAmount.ToString());

                        MailMessage mailMessage = new MailMessage
                        {
                            Subject = $"New Purchase Order #{_po.VoucherNumber} Dated {_po.VoucherDate.ToString("dd/MMM/yyyy")}",
                            Body = htmlString,
                            IsBodyHtml = true,
                            From = new MailAddress("lpo.intlmill@gmail.com"),
                        };

                        mailMessage.To.Add(new MailAddress(setting.GmEmail));

                        if (!string.IsNullOrEmpty(setting.AdditionalCc1))
                            mailMessage.To.Add(new MailAddress(setting.AdditionalCc1));

                        if (!string.IsNullOrEmpty(setting.AdditionalCc2))
                            mailMessage.To.Add(new MailAddress(setting.AdditionalCc2));

                        SmtpClient smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential("lpo.intlmill@gmail.com", "Intlmill2021")
                        };

                        smtp.Send(mailMessage);
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void SendChequeReadyEmail(PurchaseOrder purchaseOrder, ICS_Transaction trans)
        {
            try
            {
                var setting = _report.Settings.FirstOrDefault();
                var lpoStatus = purchaseOrder.LpoStatus == 1 ? "Approved" : purchaseOrder.LpoStatus == 2 ? "Rejected" : "Pending";
                var paymentStatus = purchaseOrder.LpoPaymentStatus == 1 ? "Cheque Ready" : "Pending";
                var customerNameAr = trans.GL_Ledger2 != null ? trans.GL_Ledger2.A_Ldgr_Name : "";

                var startDate = purchaseOrder.VoucherDate.AddDays(-20);
                var purchaseOrderBeforeInvoice = _report.PurchaseOrders.FirstOrDefault(x => x.LdgrCd == purchaseOrder.LdgrCd && x.LpoStatus == 1 && x.VoucherDate >= startDate && x.VoucherDate <= purchaseOrder.VoucherDate); 

                var transDetails = GetDetails(trans.Entry_Id, trans.Voucher_Date);
                var midBodyString = "";

                foreach (var detail in transDetails)
                {
                    midBodyString += $"<tr>" +
                        $"<td style='border: 1px solid #dddddd; text-align: left'>({detail.Part_Number}) {detail.ProductNameAr}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.Package.Value:#0.000}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.PackQty.Value:#0.000}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.SellUnit}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.SellQuantity.Value:#0.000}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.UnitPrice.Value:#0.000}</td>" +
                                $"<td style='border: 1px solid #dddddd; text-align: left'>{detail.Amount.Value:#0.000}</td>" +
                                $"</tr>";
                }

                //var firstBodyString = "<html><body>" +
                //    "<a style='color:#FF33CD' href='http://192.168.250.2:70/ImillReports/PurchaseOrder/Index' target='_blank'>Click Here To Open Purchase Order</a>" +
                //    "<h4>Date of Purchase Order : {0} </h4> " +
                //    "<h4>LPO Number : {1} </h4> " +
                //    "<h4>Supplier Name : {2} </h4> " +
                //    "<h4>Total Amount : {3} </h4> " +
                //    "<h4>LPO Status : <span style='background-color: {9}'>{4}</span></h4> " +
                //    "<h4>Gm Comments : {5} </h4> " +
                //    "<table style='min-width: 820px; width:100%'> <thead> " +
                //    "<tr> " +
                //    "<th style='text-align: left'>Description</th> " +
                //    "<th style='text-align: left'>Pack Unit</th>" +
                //    "<th style='text-align: left'>Pack Qty</th> " +
                //    "<th style='text-align: left'>Unit</th>" +
                //    "<th style='text-align: left'>Qty</th>" +
                //    "<th style='text-align: left'>Price</th>" +
                //    "<th style='text-align: left'>Amount</th> " +
                //    "</tr></thead> <tbody>";

                var netAmount = trans.Voucher_Amt_FC - trans.Discount_Amt_FC;
                var color = lpoStatus == "Approved" ? "#00ff00" : lpoStatus == "Rejected" ? "#ff0000" : "";
                var poDate = purchaseOrderBeforeInvoice != null ? purchaseOrderBeforeInvoice.VoucherDate.ToString("dd/MMM/yyyy") : trans.Voucher_Date.ToString("dd/MMM/yyyy");
                var firstBodyString = "<html><body>" +
                    $"<a style='color:#FF33CD' href='http://192.168.250.2:70/ImillReports/PurchaseOrder/GetDetails?entryId={purchaseOrder.EntryId}&amp;transDate=' target='_blank'>Click Here To Open Purchase Order Details</a>" +
                    $"<h4>Date of Purchase Order : {poDate} </h4> " +
                    $"<h4>LPO Number : {trans.Voucher_No} </h4> " +
                    $"<h4>Supplier Name : {customerNameAr} </h4> " +
                    $"<h4>Total Amount : {netAmount:#0.000} </h4> " +
                    $"<h4>LPO Status : <span style='background-color: {color}'>{lpoStatus}</span></h4> " +
                    $"<h4>Gm Comments : {purchaseOrder.GmComments} </h4> " +
                    $"<h4>Item Arrival Date : {trans.Voucher_Date:dd/MMM/yyyy} </h4> " +
                    $"<h4>QA Approval Date : {purchaseOrder.QAStatusTransDate:dd/MMM/yyyy} </h4> " +
                    $"<h4>QA Remarks : {purchaseOrder.QARemarks} </h4> " +
                    $"<h4>Accountant Remarks : {purchaseOrder.PaymentRemarks} </h4> " +
                    "<table style='min-width: 820px; width:100%'> <thead> " +
                    "<tr> " +
                    "<th style='text-align: left'>Description</th> " +
                    "<th style='text-align: left'>Pack Unit</th>" +
                    "<th style='text-align: left'>Pack Qty</th> " +
                    "<th style='text-align: left'>Unit</th>" +
                    "<th style='text-align: left'>Qty</th>" +
                    "<th style='text-align: left'>Price</th>" +
                    "<th style='text-align: left'>Amount</th> " +
                    "</tr></thead> <tbody>";

                var lastBodyString = 
                    $"<tr><td></td><td></td><td></td><td></td><td></td><td>Total Amount</td><td>{trans.Voucher_Amt_FC:#0.000}</td></tr>" +
                    $"<tr><td></td><td></td><td></td><td></td><td></td><td>Discount</td><td>{trans.Discount_Amt_FC:#0.000}</td></tr>" +
                    $"<tr><td></td><td></td><td></td><td></td><td></td><td>Net Amount</td><td>{netAmount:#0.000}</td></tr></tbody> </table></body></html>";

                // string lastBodyString = "</tbody> </table></body></html>";


                //string htmlString = string.Format(firstBodyString + midBodyString + midBodyString2 + lastBodyString, trans.Voucher_Date.ToString("dd/MMM/yyyy"), 
                //    trans.Voucher_No, customerNameAr, netAmount.ToString("#0.000"), lpoStatus, purchaseOrder.GmComments, trans.Voucher_Amt_FC.ToString("#0.000"), 
                //    trans.Discount_Amt_FC.ToString("#0.000"), netAmount.ToString("#0.000"), color);

                var htmlString = firstBodyString + midBodyString + lastBodyString;
                var mailMessage = new MailMessage
                {
                    Subject = $"Company : {customerNameAr} | {paymentStatus}",
                    Body = htmlString,
                    IsBodyHtml = true,
                    From = new MailAddress("lpo.intlmill@gmail.com")
                };

                mailMessage.To.Add(new MailAddress(setting.PurchaseEmail));
                mailMessage.To.Add(new MailAddress(setting.GmEmail));

                if (!string.IsNullOrEmpty(setting.AdditionalCc1))
                    mailMessage.To.Add(new MailAddress(setting.AdditionalCc1));

                if (!string.IsNullOrEmpty(setting.AdditionalCc2))
                    mailMessage.To.Add(new MailAddress(setting.AdditionalCc2));

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("lpo.intlmill@gmail.com", "Intlmill2021")
                };

                smtp.Send(mailMessage);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}