using System;
using System.Linq;
using System.Data.Entity;
using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;

namespace ImillReports.Repository
{
    public class SalesReportRepository : ISalesReportRepository
    {
        private readonly IMILLEntities _context;
        private readonly ILocationRepository _locationRepository;

        public SalesReportRepository(IMILLEntities context, ILocationRepository locationRepository)
        {
            _context = context;
            _locationRepository = locationRepository;
        }

        public SalesReportViewModel GetSalesDetailReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray)
        {
            try
            {
                if (fromDate == null) fromDate = DateTime.Now;
                if (toDate == null) toDate = DateTime.Now;

                var transactions = _context.spICSTrans_GetAll(fromDate, toDate, locationArray, voucherTypesArray).ToList();

                var EntryIdArray = new List<string>();
                EntryIdArray.AddRange(from item in transactions select item.Entry_Id.ToString());

                var EntryIdString = EntryIdArray != null && EntryIdArray.Count > 0 ? string.Join(",", EntryIdArray) : "";
                var transactionDetails = _context.spICSTransDetail_GetAll(EntryIdString, productStringArray).ToList();

                var salesReportItems = new List<SalesReportItem>();

                foreach (var detail in transactionDetails)
                {
                    var trans = transactions.FirstOrDefault(x => x.Entry_Id == detail.Entry_Id);

                    var salesReportItem = new SalesReportItem
                    {
                        Location = trans.L_Short_Name,
                        LocationId = trans.Locat_Cd,
                        InvDateTime = trans.Voucher_Date,
                        Salesman = trans.L_Sman_Name,
                        Voucher = trans.L_Voucher_Name,
                        VoucherId = trans.Voucher_Type,
                        InvoiceNumber = trans.Voucher_No,
                        GroupCD = trans.Group_Cd,
                        CustomerName = trans.L_Ldgr_Name,
                        CustomerNameAr = trans.A_Ldgr_Name,
                        ProdId = detail.ProdId,
                        ProductNameEn = detail.ProdEn,
                        ProductNameAr = detail.ProdAr,
                        BaseQuantity = detail.IUDBaseQty,
                        BaseUnit = detail.BaseUnit,
                        BaseUnitId = detail.BaseUnitId,
                        SellQuantity = detail.Qty,
                        SellUnit = detail.SellUnit,
                        SellUnitId = detail.SellUnitId,
                        Discount = trans.Voucher_Type == 202 ||
                               trans.Voucher_Type == 2023 ||
                               trans.Voucher_Type == 2035 ||
                               trans.Voucher_Type == 2036
                                ? -detail.FC_Prod_Dis
                                : detail.FC_Prod_Dis,
                        Amount = trans.Voucher_Type == 202 ||
                               trans.Voucher_Type == 2023 ||
                               trans.Voucher_Type == 2035 ||
                               trans.Voucher_Type == 2036
                                ? -detail.FC_Amount
                                : detail.FC_Amount
                    };

                    salesReportItems.Add(salesReportItem);
                }

                return new SalesReportViewModel
                {
                    SalesReportItems = salesReportItems
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SalesReportViewModel GetSalesReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            var transactions = _context.ICS_Transaction
                .Include(a => a.SM_Location)
                .Include(a => a.SM_SALESMAN)
                .Include(a => a.ICS_Transaction_Types)
                .Include(a => a.GL_Ledger)
                .Include(a => a.GL_Ledger1)
                .Include(a => a.GL_Ledger2)
                .Where(x => x.Voucher_Date >= fromDate &&
                            x.Voucher_Date <= toDate &&

                            // Sales Voucher Type Id
                            (x.ICS_Transaction_Types.Voucher_Type == 201 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2021 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2022 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2025 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2026 ||

                            // Sales Return Voucher Type Id
                            x.ICS_Transaction_Types.Voucher_Type == 202 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2023 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2035 ||
                            x.ICS_Transaction_Types.Voucher_Type == 2036) &&

                            x.GL_Ledger2.Group_Cd != 329);

            if (!string.IsNullOrEmpty(locationArray))
            {
                var locationIds = new List<short>();
                foreach (var id in locationArray.Split(','))
                {
                    locationIds.Add(short.Parse(id));
                }

                transactions = transactions.Where(x => locationIds.Contains(x.SM_Location.Locat_Cd));
            }

            if (!string.IsNullOrEmpty(voucherTypesArray))
            {
                var voucherIds = new List<short>();
                foreach (var id in voucherTypesArray.Split(','))
                {
                    voucherIds.Add(short.Parse(id));
                }

                transactions = transactions.Where(x => voucherIds.Contains(x.ICS_Transaction_Types.Voucher_Type));
            }

            var salesReportItems = new List<SalesReportItem>();

            foreach (var transaction in transactions)
            {
                var salesReportItem = new SalesReportItem
                {
                    Location = transaction.SM_Location.L_Short_Name,
                    LocationId = transaction.SM_Location.Locat_Cd,
                    Date = transaction.Voucher_Date.Date,
                    InvDateTime = transaction.Voucher_Date,
                    Salesman = transaction.SM_SALESMAN.L_Sman_Name,
                    Voucher = transaction.ICS_Transaction_Types.L_Voucher_Name,
                    VoucherId = transaction.ICS_Transaction_Types.Voucher_Type,
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

                    SalesReturn = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2036
                                    ? -transaction.Net_Amt_FC
                                    : 0,

                    NetAmount = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2036
                                    ? -transaction.Net_Amt_FC
                                    : transaction.Net_Amt_FC,

                    Cash = (transaction.ICS_Transaction_Types.Voucher_Type == 201 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2021 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2022 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2025 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2026) &&
                            string.IsNullOrEmpty(transaction.Credit_Card_Type)
                                ? transaction.Net_Amt_FC
                                : (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2036) &&
                                   string.IsNullOrEmpty(transaction.Credit_Card_Type)
                                        ? -transaction.Net_Amt_FC
                                        : 0,

                    Knet = (transaction.ICS_Transaction_Types.Voucher_Type == 201 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2021 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2022 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2025 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2026) &&
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

                    CreditCard = (transaction.ICS_Transaction_Types.Voucher_Type == 201 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2021 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2022 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2025 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2026) &&
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

        public SalesPeakHourViewModel GetSalesHourlyReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            var salesReportItems = GetSalesReport(fromDate, toDate, locationArray, "").SalesReportItems;
            var salesPeakHourItems = new List<SalesPeakHourItem>();
            var hoursCount = 24;
            

            if(locationArray != "")
            {
                var locationListIds = new List<int>();
                var locationStringArray = locationArray.Split(',');

                foreach (var item in locationStringArray)
                {
                    var id = int.Parse(item);
                    locationListIds.Add(id);
                }

                var locations = _locationRepository.GetLocations().LocationItems.Where(x => locationListIds.Contains(x.LocationId));


                foreach (var location in locations)
                {
                    var currentstartHour = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 6, 00, 00);

                    for (int i = 1; i <= hoursCount; i++)
                    {
                        var salesItems = salesReportItems.Where(x => x.InvDateTime >= currentstartHour &&
                                                                     x.InvDateTime <= currentstartHour.Add(TimeSpan.FromMinutes(59)) &&
                                                                     x.LocationId == location.LocationId);

                        var salesPeakHourItem = new SalesPeakHourItem
                        {
                            LocationId = location.LocationId,
                            Location = location.Name,
                            Hour = currentstartHour,
                            Amount = salesItems.Sum(a => a.NetAmount),
                            TransCount = salesItems.Count()
                        };

                        salesPeakHourItems.Add(salesPeakHourItem);
                        currentstartHour = currentstartHour.Add(TimeSpan.FromHours(1));
                    }
                }
            }

            return new SalesPeakHourViewModel
            {
                SalesPeakHourItems = salesPeakHourItems
            };
        }
    }
}