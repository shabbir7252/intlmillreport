using IntlmillReports.Contracts;
using IntlmillReports.Models;
using IntlmillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace IntlmillReports.Repository
{
    public class SalesReportRepository : ISalesReportRepository
    {
        private readonly IMILLEntities _context;

        public SalesReportRepository(IMILLEntities context)
        {
            _context = context;
        }
        public SalesReportViewModel GetSalesReport(DateTime? fromDate, DateTime? toDate, int? locationId)
        {
            if (fromDate == null)
            {
                fromDate = DateTime.Now;
            }

            if (toDate == null)
            {
                toDate = DateTime.Now;
            }

            var transactions = _context.ICS_Transaction
                .Include(a => a.SM_Location)
                .Include(a => a.SM_SALESMAN)
                .Include(a => a.ICS_Transaction_Types)
                .Include(a => a.GL_Ledger)
                .Include(a => a.GL_Ledger1)
                .Include(a => a.GL_Ledger2)
                .Where(x => x.Voucher_Date >= fromDate &&
                            x.Voucher_Date <= toDate &&
                            (x.ICS_Transaction_Types.Voucher_Type == 201 ||
                            x.ICS_Transaction_Types.Voucher_Type == 202 ||
                            x.ICS_Transaction_Types.Voucher_Type == 421 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2021 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2022 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2023 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2025 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2026 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2035 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2036) &&
                            x.GL_Ledger2.Group_Cd != 329);

            if(locationId != 0 && locationId != null)
            {
                transactions = transactions.Where(x => x.SM_Location.Locat_Cd == locationId);
            }

            var salesReportItems = new List<SalesReportItem>();

            foreach (var transaction in transactions)
            {
                var salesReportItem = new SalesReportItem
                {
                    Location = transaction.SM_Location.L_Short_Name,
                    InvDateTime = transaction.Voucher_Date,
                    Salesman = transaction.SM_SALESMAN.L_Sman_Name,
                    Voucher = transaction.ICS_Transaction_Types.L_Voucher_Name,
                    InvoiceNumber = transaction.Voucher_No,
                    GroupCD = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Group_Cd : 0,
                    CustomerName = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.L_Ldgr_Name : "",
                    CustomerNameAr = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.A_Ldgr_Name : "",

                    AmountRecieved = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2036
                                    ? -transaction.Voucher_Amt_FC
                                    : transaction.Voucher_Amt_FC,

                    Discount = transaction.Discount_Amt_FC,

                    NetAmount = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2036
                                    ? -transaction.Net_Amt_FC
                                    : transaction.Net_Amt_FC,

                    Cash = transaction.ICS_Transaction_Types.Voucher_Type == 201 && string.IsNullOrEmpty(transaction.Credit_Card_Type) ? transaction.Net_Amt_FC :
                           (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                           transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                           transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                           transaction.ICS_Transaction_Types.Voucher_Type == 2036) &&
                           string.IsNullOrEmpty(transaction.Credit_Card_Type)
                                    ? -transaction.Net_Amt_FC
                                    : 0,

                    Knet = transaction.ICS_Transaction_Types.Voucher_Type == 201 &&
                           !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                           (transaction.Credit_Card_Type == "K-Net" || transaction.Credit_Card_Type == "Knet")
                                ? transaction.Net_Amt_FC
                                : (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2036) &&
                                   !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                                   (transaction.Credit_Card_Type == "K-Net" || transaction.Credit_Card_Type == "Knet")
                                             ? -transaction.Net_Amt_FC
                                             : 0,

                    CreditCard = transaction.ICS_Transaction_Types.Voucher_Type == 201 &&
                                 !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                                 transaction.Credit_Card_Type == "CC"
                                    ? transaction.Net_Amt_FC
                                    : (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2036) &&
                                       !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                                       transaction.Credit_Card_Type == "CC"
                                           ? -transaction.Net_Amt_FC
                                           : 0,

                    CreditCardType = transaction.Credit_Card_Type
                };

                salesReportItems.Add(salesReportItem);
            }

            return new SalesReportViewModel
            {
                SalesReportItems = salesReportItems
            };
        }
    }
}