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
        private readonly ImillReportsEntities _report;

        public SalesReportRepository(IMILLEntities context, ILocationRepository locationRepository, ImillReportsEntities report)
        {
            _context = context;
            _locationRepository = locationRepository;
            _report = report;
        }

        struct DateTimeRange
        {
            public DateTimeRange(DateTime start, DateTime end)
            {
                Start = start;
                End = end;
            }
            public DateTime Start { get; }
            public DateTime End { get; }
        }

        // https://stackoverflow.com/questions/13690617/split-date-range-into-date-range-chunks/13691511
        public static IEnumerable<Tuple<DateTime, DateTime>> SplitDateRange(DateTime start, DateTime end, int dayChunkSize)
        {
            DateTime chunkEnd;
            while ((chunkEnd = start.AddDays(dayChunkSize)) < end)
            {
                yield return Tuple.Create(start, chunkEnd);
                start = chunkEnd;
            }
            yield return Tuple.Create(start, end);
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> entry, int nSize = 30)
        {
            for (int i = 0; i < entry.Count; i += nSize)
            {
                yield return entry.GetRange(i, Math.Min(nSize, entry.Count - i));
            }
        }

        private IEnumerable<DateTimeRange> GetYearsBetweenDates(DateTime startDate, DateTime stopDate)
        {
            var count = 0;
            for (int i = startDate.Year; i <= stopDate.Year; i++)
            {
                yield return new DateTimeRange
                (
                    start: (i == stopDate.Year || count > 0) && stopDate.Year != startDate.Year
                    ? new DateTime(i, 01, 01, startDate.Hour, startDate.Minute, startDate.Second)
                    : new DateTime(i, startDate.Month, startDate.Day, startDate.Hour, startDate.Minute, startDate.Second),
                    end:
                     i == stopDate.Year
                     ? new DateTime(i, stopDate.Month, stopDate.Day, stopDate.Hour, stopDate.Minute, stopDate.Second)
                     : new DateTime(i, 12, 31, stopDate.Hour, stopDate.Minute, stopDate.Second)
                );
                count += 1;
            }
        }

        private IQueryable<ICS_Transaction> GetTransactions(DateTime? fromDate, DateTime? toDate, DateTime fromHoDate, DateTime toHoDate)
        {
            return _context.ICS_Transaction
                            .Include(a => a.SM_Location)
                            .Include(a => a.SM_SALESMAN)
                            .Include(a => a.ICS_Transaction_Types)
                            .Include(a => a.GL_Ledger)
                            .Include(a => a.GL_Ledger1)
                            .Include(a => a.GL_Ledger2)
                            .Where(x => ((x.Voucher_Date >= fromDate &&
                                        x.Voucher_Date <= toDate &&
                                        x.Locat_Cd != 1) ||
                                        (x.Voucher_Date >= fromHoDate &&
                                        x.Voucher_Date <= toHoDate &&
                                        x.Locat_Cd == 1)) &&

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
                                        x.ICS_Transaction_Types.Voucher_Type == 2036)
                                        && x.GL_Ledger2.Group_Cd != 329
                                        );
        }

        public SalesReportViewModel GetSalesTransaction(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            var salesReportItems = new List<SalesReportItem>();

            var dateRange = GetYearsBetweenDates(fromDate.Value, toDate.Value).ToList();

            foreach (var date in dateRange)
            {
                var fromHoDate = date.Start.TimeOfDay >= new TimeSpan(06, 00, 00)
                    ? date.Start.Date.AddDays(1)
                    : date.Start.Date;

                var toHoDate = date.End.TimeOfDay >= new TimeSpan(06, 00, 00)
                    ? new DateTime(date.End.Date.Year, date.End.Date.Month, date.End.Date.Day, 23, 59, 00).AddDays(1)
                    : new DateTime(date.End.Date.Year, date.End.Date.Month, date.End.Date.Day, 23, 59, 00).AddDays(-1);

                if (toHoDate < fromHoDate)
                    toHoDate = fromHoDate.AddDays(1).AddMinutes(-1);

                if (date.Start.Year == 2019)
                {

                    var transactions = _report.Transaction_2019.Where(x => (x.InvDateTime >= date.Start &&
                                                                                  x.InvDateTime <= date.End &&
                                                                                  x.LocationId != 1) ||
                                                                                  (x.InvDateTime >= fromHoDate &&
                                                                                  x.InvDateTime <= toHoDate &&
                                                                                  x.LocationId == 1)).ToList();

                    if (!string.IsNullOrEmpty(locationArray))
                    {
                        var locationIds = (from id in locationArray.Split(',')
                                           select short.Parse(id)).ToList();
                        transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value)).ToList();
                    }

                    if (!string.IsNullOrEmpty(voucherTypesArray))
                    {
                        var voucherIds = (from id in voucherTypesArray.Split(',')
                                          select short.Parse(id)).ToList();
                        transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value)).ToList();
                    }

                    foreach (var transaction in transactions)
                    {
                        var salesReportItem = new SalesReportItem
                        {
                            Amount = transaction.Amount.Value,
                            AmountRecieved = transaction.AmountRecieved,
                            BaseQuantity = transaction.BaseQuantity.Value,
                            BaseUnit = transaction.BaseUnit,
                            BaseUnitId = transaction.BaseUnitId.Value,
                            Cash = transaction.Cash.Value,
                            CreditCard = transaction.CreditCard.Value,
                            CreditCardType = transaction.CreditCardType,
                            CustomerId = transaction.CustomerId.Value,
                            CustomerName = transaction.CustomerName,
                            CustomerNameAr = transaction.CustomerNameAr,
                            Date = transaction.Date.Value,
                            Discount = transaction.Discount.Value,
                            EntryId = transaction.EntryId.Value,
                            GroupCD = transaction.GroupCD.Value,
                            InvDateTime = transaction.InvDateTime.Value,
                            InvoiceNumber = transaction.InvoiceNumber.Value,
                            Knet = transaction.Knet.Value,
                            Location = transaction.Location,
                            LocationId = transaction.LocationId.Value,
                            NetAmount = transaction.NetAmount.Value,
                            ProdId = transaction.ProdId.Value,
                            ProductNameAr = transaction.ProductNameAr,
                            ProductNameEn = transaction.ProductNameEn,
                            Salesman = transaction.Salesman,
                            SalesReturn = transaction.SalesReturn.Value,
                            SellQuantity = transaction.SellQuantity.Value,
                            SellUnit = transaction.SellUnit,
                            SellUnitId = transaction.SellUnitId.Value,
                            UnitPrice = transaction.UnitPrice.Value,
                            Voucher = transaction.Voucher,
                            VoucherId = transaction.VoucherId.Value,
                            Year = transaction.Year.Value
                        };

                        salesReportItems.Add(salesReportItem);
                    }

                }

                if (date.Start.Year == 2020)
                {

                    var transactions = _report.Transaction_2020.Where(x => (x.InvDateTime >= date.Start &&
                                                                                  x.InvDateTime <= date.End &&
                                                                                  x.LocationId != 1) ||
                                                                                  (x.InvDateTime >= fromHoDate &&
                                                                                  x.InvDateTime <= toHoDate &&
                                                                                  x.LocationId == 1)).ToList();

                    if (!string.IsNullOrEmpty(locationArray))
                    {
                        var locationIds = (from id in locationArray.Split(',')
                                           select short.Parse(id)).ToList();
                        transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value)).ToList();
                    }

                    if (!string.IsNullOrEmpty(voucherTypesArray))
                    {
                        var voucherIds = (from id in voucherTypesArray.Split(',')
                                          select short.Parse(id)).ToList();
                        transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value)).ToList();
                    }

                    foreach (var transaction in transactions)
                    {
                        var salesReportItem = new SalesReportItem
                        {
                            Amount = transaction.Amount.Value,
                            AmountRecieved = transaction.AmountRecieved,
                            BaseQuantity = transaction.BaseQuantity.Value,
                            BaseUnit = transaction.BaseUnit,
                            BaseUnitId = transaction.BaseUnitId.Value,
                            Cash = transaction.Cash.Value,
                            CreditCard = transaction.CreditCard.Value,
                            CreditCardType = transaction.CreditCardType,
                            CustomerId = transaction.CustomerId.Value,
                            CustomerName = transaction.CustomerName,
                            CustomerNameAr = transaction.CustomerNameAr,
                            Date = transaction.Date.Value,
                            Discount = transaction.Discount.Value,
                            EntryId = transaction.EntryId.Value,
                            GroupCD = transaction.GroupCD.Value,
                            InvDateTime = transaction.InvDateTime.Value,
                            InvoiceNumber = transaction.InvoiceNumber.Value,
                            Knet = transaction.Knet.Value,
                            Location = transaction.Location,
                            LocationId = transaction.LocationId.Value,
                            NetAmount = transaction.NetAmount.Value,
                            ProdId = transaction.ProdId.Value,
                            ProductNameAr = transaction.ProductNameAr,
                            ProductNameEn = transaction.ProductNameEn,
                            Salesman = transaction.Salesman,
                            SalesReturn = transaction.SalesReturn.Value,
                            SellQuantity = transaction.SellQuantity.Value,
                            SellUnit = transaction.SellUnit,
                            SellUnitId = transaction.SellUnitId.Value,
                            UnitPrice = transaction.UnitPrice.Value,
                            Voucher = transaction.Voucher,
                            VoucherId = transaction.VoucherId.Value,
                            Year = transaction.Year.Value
                        };

                        salesReportItems.Add(salesReportItem);
                    }

                }
            }
            return new SalesReportViewModel
            {
                SalesReportItems = salesReportItems
            };
        }

        public SalesReportDashboard GetSalesDashboardTransaction(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray)
        {
            try
            {
                if (fromDate == null) fromDate = DateTime.Now;
                if (toDate == null) toDate = DateTime.Now;

                //var fromHoDate = fromDate.Value.Date;
                //var toHoDate = toDate.Value.Date;

                var transactions = GetSalesTransaction(fromDate, toDate, locationArray, voucherTypesArray).SalesReportItems;

                var entryIdArray = new List<long>();
                entryIdArray.AddRange(from item in transactions select item.EntryId);

                var dividedEntry = SplitList(entryIdArray, 5000);

                long[] productIdArray = null;
                if (!string.IsNullOrEmpty(productStringArray))
                {
                    var productIdStringArray = productStringArray.Split(',');
                    productIdArray = Array.ConvertAll(productIdStringArray, s => long.Parse(s));
                }

                var transactionDetails = new List<TransDetailsViewModel>();

                foreach (var entries in dividedEntry)
                {
                    if (productIdArray != null && productIdArray.Length > 0)
                    {
                        var transDetails2019 = _report.Trans_Detail_2019.Where(x => entries.Contains(x.EntryId.Value) && productIdArray.Contains(x.ProdId.Value)).ToList();
                        foreach (var detail in transDetails2019)
                        {
                            transactionDetails.Add(new TransDetailsViewModel
                            {
                                Amount = detail.Amount,
                                AmountRecieved = detail.AmountRecieved,
                                BaseQuantity = detail.BaseQuantity,
                                BaseUnit = detail.BaseUnit,
                                BaseUnitId = detail.BaseUnitId,
                                Cash = detail.Cash,
                                CreditCard = detail.CreditCard,
                                CreditCardType = detail.CreditCardType,
                                CustomerId = detail.CustomerId,
                                CustomerName = detail.CustomerName,
                                CustomerNameAr = detail.CustomerNameAr,
                                Date = detail.Date,
                                Discount = detail.Discount,
                                EntryId = detail.EntryId,
                                GroupCD = detail.GroupCD,
                                InvDateTime = detail.InvDateTime,
                                InvoiceNumber = detail.InvoiceNumber,
                                Knet = detail.Knet,
                                Location = detail.Location,
                                LocationId = detail.LocationId,
                                NetAmount = detail.NetAmount,
                                Oid = detail.Oid,
                                ProdId = detail.ProdId,
                                ProductNameAr = detail.ProductNameAr,
                                ProductNameEn = detail.ProductNameEn,
                                Salesman = detail.Salesman,
                                SalesReturn = detail.SalesReturn,
                                SellQuantity = detail.SellQuantity,
                                SellUnit = detail.SellUnit,
                                SellUnitId = detail.SellUnitId,
                                UnitPrice = detail.UnitPrice,
                                Voucher = detail.Voucher,
                                VoucherId = detail.VoucherId,
                                Year = detail.Year
                            });
                        }

                        var transDetails2020 = _report.Trans_Detail_2020.Where(x => entries.Contains(x.EntryId.Value) && productIdArray.Contains(x.ProdId.Value)).ToList();
                        foreach (var detail in transDetails2020)
                        {
                            transactionDetails.Add(new TransDetailsViewModel
                            {
                                Amount = detail.Amount,
                                AmountRecieved = detail.AmountRecieved,
                                BaseQuantity = detail.BaseQuantity,
                                BaseUnit = detail.BaseUnit,
                                BaseUnitId = detail.BaseUnitId,
                                Cash = detail.Cash,
                                CreditCard = detail.CreditCard,
                                CreditCardType = detail.CreditCardType,
                                CustomerId = detail.CustomerId,
                                CustomerName = detail.CustomerName,
                                CustomerNameAr = detail.CustomerNameAr,
                                Date = detail.Date,
                                Discount = detail.Discount,
                                EntryId = detail.EntryId,
                                GroupCD = detail.GroupCD,
                                InvDateTime = detail.InvDateTime,
                                InvoiceNumber = detail.InvoiceNumber,
                                Knet = detail.Knet,
                                Location = detail.Location,
                                LocationId = detail.LocationId,
                                NetAmount = detail.NetAmount,
                                Oid = detail.Oid,
                                ProdId = detail.ProdId,
                                ProductNameAr = detail.ProductNameAr,
                                ProductNameEn = detail.ProductNameEn,
                                Salesman = detail.Salesman,
                                SalesReturn = detail.SalesReturn,
                                SellQuantity = detail.SellQuantity,
                                SellUnit = detail.SellUnit,
                                SellUnitId = detail.SellUnitId,
                                UnitPrice = detail.UnitPrice,
                                Voucher = detail.Voucher,
                                VoucherId = detail.VoucherId,
                                Year = detail.Year
                            });
                        }
                    }
                    else
                    {
                        var transDetails2019 = _report.Trans_Detail_2019.Where(x => entries.Contains(x.EntryId.Value)).ToList();
                        foreach (var detail in transDetails2019)
                        {
                            transactionDetails.Add(new TransDetailsViewModel
                            {
                                Amount = detail.Amount,
                                AmountRecieved = detail.AmountRecieved,
                                BaseQuantity = detail.BaseQuantity,
                                BaseUnit = detail.BaseUnit,
                                BaseUnitId = detail.BaseUnitId,
                                Cash = detail.Cash,
                                CreditCard = detail.CreditCard,
                                CreditCardType = detail.CreditCardType,
                                CustomerId = detail.CustomerId,
                                CustomerName = detail.CustomerName,
                                CustomerNameAr = detail.CustomerNameAr,
                                Date = detail.Date,
                                Discount = detail.Discount,
                                EntryId = detail.EntryId,
                                GroupCD = detail.GroupCD,
                                InvDateTime = detail.InvDateTime,
                                InvoiceNumber = detail.InvoiceNumber,
                                Knet = detail.Knet,
                                Location = detail.Location,
                                LocationId = detail.LocationId,
                                NetAmount = detail.NetAmount,
                                Oid = detail.Oid,
                                ProdId = detail.ProdId,
                                ProductNameAr = detail.ProductNameAr,
                                ProductNameEn = detail.ProductNameEn,
                                Salesman = detail.Salesman,
                                SalesReturn = detail.SalesReturn,
                                SellQuantity = detail.SellQuantity,
                                SellUnit = detail.SellUnit,
                                SellUnitId = detail.SellUnitId,
                                UnitPrice = detail.UnitPrice,
                                Voucher = detail.Voucher,
                                VoucherId = detail.VoucherId,
                                Year = detail.Year
                            });
                        }

                        var transDetails2020 = _report.Trans_Detail_2020.Where(x => entries.Contains(x.EntryId.Value)).ToList();
                        foreach (var detail in transDetails2020)
                        {
                            transactionDetails.Add(new TransDetailsViewModel
                            {
                                Amount = detail.Amount,
                                AmountRecieved = detail.AmountRecieved,
                                BaseQuantity = detail.BaseQuantity,
                                BaseUnit = detail.BaseUnit,
                                BaseUnitId = detail.BaseUnitId,
                                Cash = detail.Cash,
                                CreditCard = detail.CreditCard,
                                CreditCardType = detail.CreditCardType,
                                CustomerId = detail.CustomerId,
                                CustomerName = detail.CustomerName,
                                CustomerNameAr = detail.CustomerNameAr,
                                Date = detail.Date,
                                Discount = detail.Discount,
                                EntryId = detail.EntryId,
                                GroupCD = detail.GroupCD,
                                InvDateTime = detail.InvDateTime,
                                InvoiceNumber = detail.InvoiceNumber,
                                Knet = detail.Knet,
                                Location = detail.Location,
                                LocationId = detail.LocationId,
                                NetAmount = detail.NetAmount,
                                Oid = detail.Oid,
                                ProdId = detail.ProdId,
                                ProductNameAr = detail.ProductNameAr,
                                ProductNameEn = detail.ProductNameEn,
                                Salesman = detail.Salesman,
                                SalesReturn = detail.SalesReturn,
                                SellQuantity = detail.SellQuantity,
                                SellUnit = detail.SellUnit,
                                SellUnitId = detail.SellUnitId,
                                UnitPrice = detail.UnitPrice,
                                Voucher = detail.Voucher,
                                VoucherId = detail.VoucherId,
                                Year = detail.Year
                            });
                        }
                    }
                }

                return new SalesReportDashboard
                {
                    SRItemsTrans = transactions,
                    SRItemsTransDetails = transactionDetails.OrderBy(x => x.InvDateTime.Value).ToList()
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<TransDetailsViewModel> GetSalesDetailTransaction(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray)
        {
            try
            {
                //if (fromDate == null) fromDate = DateTime.Now;
                //if (toDate == null) toDate = DateTime.Now;

                //var fromHoDate = fromDate.Value.Date;
                //var toHoDate = toDate.Value.Date;

                //var transactions = GetSalesTransaction(fromDate, toDate, locationArray, voucherTypesArray).SalesReportItems;

                //var entryIdArray = new List<long>();
                //entryIdArray.AddRange(from item in transactions select item.EntryId);

                //var dividedEntry = SplitList(entryIdArray, 5000);

                //long[] productIdArray = null;
                //if (!string.IsNullOrEmpty(productStringArray))
                //{
                //    var productIdStringArray = productStringArray.Split(',');
                //    productIdArray = Array.ConvertAll(productIdStringArray, s => long.Parse(s));
                //}

                //var transactionDetails = new List<Trans_Detail_2020>();

                //foreach (var entries in dividedEntry)
                //{
                //    if (productIdArray != null && productIdArray.Length > 0)
                //        transactionDetails.AddRange(_report.Trans_Detail_2020.Where(x => entries.Contains(x.EntryId.Value) && productIdArray.Contains(x.ProdId.Value)).ToList());
                //    else
                //        transactionDetails.AddRange(_report.Trans_Detail_2020.Where(x => entries.Contains(x.EntryId.Value)).ToList());
                //}

                var result = GetSalesDashboardTransaction(fromDate, toDate, locationArray, voucherTypesArray, productStringArray);
                return result.SRItemsTransDetails;
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

            var salesReportItems = new List<SalesReportItem>();

            var fromHoDate = fromDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? fromDate.Value.Date.AddDays(1)
                : fromDate.Value.Date;

            var toHoDate = toDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(1)
                : new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(-1);

            if (toHoDate < fromHoDate)
                toHoDate = fromHoDate.AddDays(1).AddMinutes(-1);

            var transactions = GetTransactions(fromDate, toDate, fromHoDate, toHoDate);

            if (!string.IsNullOrEmpty(locationArray))
            {
                var locationIds = (from id in locationArray.Split(',')
                                   select short.Parse(id)).ToList();
                transactions = transactions.Where(x => locationIds.Contains(x.SM_Location.Locat_Cd));
            }

            if (!string.IsNullOrEmpty(voucherTypesArray))
            {
                var voucherIds = (from id in voucherTypesArray.Split(',')
                                  select short.Parse(id)).ToList();
                transactions = transactions.Where(x => voucherIds.Contains(x.ICS_Transaction_Types.Voucher_Type));
            }

            foreach (var transaction in transactions)
            {
                var salesReportItem = new SalesReportItem
                {
                    EntryId = transaction.Entry_Id,
                    Location = transaction.SM_Location.L_Short_Name,
                    LocationId = transaction.SM_Location.Locat_Cd,
                    Year = transaction.Voucher_Date.Year,
                    Date = transaction.Voucher_Date.Date,
                    InvDateTime = transaction.Voucher_Date,
                    Salesman = transaction.SM_SALESMAN.L_Sman_Name,
                    Voucher = transaction.ICS_Transaction_Types.L_Voucher_Name,
                    VoucherId = transaction.ICS_Transaction_Types.Voucher_Type,
                    InvoiceNumber = transaction.Voucher_No,
                    GroupCD = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Group_Cd : 0,
                    CustomerId = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Ldgr_Cd : 0,
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

            List<Transaction_2020> imillTrans2020 = null;
            var newTrans200 = new List<Transaction_2020>();
            foreach (var items in salesReportItems.GroupBy(x => x.Year).OrderBy(x => x.Key))
            {
                var startDate = items.OrderBy(x => x.Date).FirstOrDefault().Date;
                var endDate = items.OrderByDescending(x => x.Date).FirstOrDefault().Date;
                if (items.Key == 2020)
                {
                    foreach (var item in items)
                    {
                        imillTrans2020 = _report.Transaction_2020.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        if (!imillTrans2020.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                        {
                            var iTrans = new Transaction_2020
                            {
                                Amount = item.Amount,
                                AmountRecieved = item.AmountRecieved,
                                BaseQuantity = item.BaseQuantity,
                                BaseUnit = item.BaseUnit,
                                BaseUnitId = item.BaseUnitId,
                                Cash = item.Cash,
                                CreditCard = item.CreditCard,
                                CreditCardType = item.CreditCardType,
                                CustomerId = item.CustomerId,
                                CustomerName = item.CustomerName,
                                CustomerNameAr = item.CustomerNameAr,
                                Date = item.Date,
                                Discount = item.Discount,
                                EntryId = item.EntryId,
                                GroupCD = item.GroupCD,
                                InvDateTime = item.InvDateTime,
                                InvoiceNumber = item.InvoiceNumber,
                                Knet = item.Knet,
                                Location = item.Location,
                                LocationId = item.LocationId,
                                NetAmount = item.NetAmount,
                                ProdId = item.ProdId,
                                ProductNameAr = item.ProductNameAr,
                                ProductNameEn = item.ProductNameEn,
                                Salesman = item.Salesman,
                                SalesReturn = item.SalesReturn,
                                SellQuantity = item.SellQuantity,
                                SellUnit = item.SellUnit,
                                SellUnitId = item.SellUnitId,
                                UnitPrice = item.UnitPrice,
                                Voucher = item.Voucher,
                                VoucherId = item.VoucherId,
                                Year = item.Year

                            };

                            newTrans200.Add(iTrans);
                        }
                    }
                    if (newTrans200.Any())
                    {
                        _report.Transaction_2020.AddRange(newTrans200);
                        _report.SaveChanges();
                    }
                }
            }

            return new SalesReportViewModel
            {
                SalesReportItems = salesReportItems
            };
        }

        public SalesReportViewModel GetSalesDetailReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray)
        {
            long entryId = 0;
            long prodId = 0;
            try
            {
                if (fromDate == null) fromDate = DateTime.Now;
                if (toDate == null) toDate = DateTime.Now;

                var fromHoDate = fromDate.Value.Date;
                var toHoDate = toDate.Value.Date;

                // var transactions = _context.spICSTrans_GetAll(fromDate, toDate, locationArray, voucherTypesArray, fromHoDate, toHoDate).ToList();
                var transactions = GetSalesTransaction(fromDate, toDate, locationArray, voucherTypesArray).SalesReportItems;

                var entryIdArray = new List<string>();
                entryIdArray.AddRange(from item in transactions select item.EntryId.ToString());

                var entryIdString = entryIdArray != null && entryIdArray.Count > 0 ? string.Join(",", entryIdArray) : "";
                var transactionDetails = _context.spICSTransDetail_GetAll(entryIdString, productStringArray).ToList();

                var salesReportItems = new List<SalesReportItem>();

                foreach (var detail in transactionDetails)
                {
                    prodId = detail.ProdId;
                    var trans = transactions.FirstOrDefault(x => x.EntryId == detail.Entry_Id);

                    var salesReportItem = new SalesReportItem
                    {
                        //Location = trans.L_Short_Name,
                        //LocationId = trans.Locat_Cd,
                        //InvDateTime = trans.Voucher_Date,
                        //Salesman = trans.L_Sman_Name,
                        //Voucher = trans.L_Voucher_Name,
                        //VoucherId = trans.Voucher_Type,
                        //InvoiceNumber = trans.Voucher_No,
                        //GroupCD = trans.Group_Cd,
                        //CustomerName = trans.L_Ldgr_Name,
                        //CustomerNameAr = trans.A_Ldgr_Name,
                        EntryId = detail.Entry_Id,
                        Location = trans.Location,
                        LocationId = trans.LocationId,
                        InvDateTime = trans.InvDateTime,
                        Salesman = trans.Salesman,
                        Voucher = trans.Voucher,
                        VoucherId = trans.VoucherId,
                        InvoiceNumber = trans.InvoiceNumber,
                        GroupCD = trans.GroupCD,
                        CustomerId = trans.CustomerId,
                        CustomerName = trans.CustomerName,
                        CustomerNameAr = trans.CustomerNameAr,
                        ProdId = detail.ProdId,
                        ProductNameEn = detail.ProdEn,
                        ProductNameAr = detail.ProdAr,
                        BaseQuantity = detail.IUDBaseQty,
                        BaseUnit = detail.BaseUnit,
                        BaseUnitId = detail.BaseUnitId,
                        SellQuantity = detail.Qty,
                        SellUnit = detail.SellUnit,
                        SellUnitId = detail.SellUnitId,
                        Discount = trans.VoucherId == 202 ||
                               trans.VoucherId == 2023 ||
                               trans.VoucherId == 2035 ||
                               trans.VoucherId == 2036
                                ? -detail.FC_Prod_Dis
                                : detail.FC_Prod_Dis,
                        Amount = trans.VoucherId == 202 ||
                               trans.VoucherId == 2023 ||
                               trans.VoucherId == 2035 ||
                               trans.VoucherId == 2036
                                ? -detail.FC_Amount
                                : detail.FC_Amount,
                        Year = trans.InvDateTime.Year,
                        Date = trans.InvDateTime.Date
                    };

                    salesReportItems.Add(salesReportItem);
                }


                List<Trans_Detail_2020> trans_detail_2020 = null;
                var newTransDetail2020 = new List<Trans_Detail_2020>();

                foreach (var items in salesReportItems.GroupBy(x => x.Year).OrderBy(x => x.Key))
                {
                    var startDate = items.OrderBy(x => x.InvDateTime).FirstOrDefault().Date;
                    var endDate = items.OrderByDescending(x => x.InvDateTime).FirstOrDefault().Date;

                    if (items.Key == 2020)
                    {
                        foreach (var item in items)
                        {
                            entryId = item.EntryId;
                            trans_detail_2020 = _report.Trans_Detail_2020.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                            if (!trans_detail_2020.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.ProdId == item.ProdId && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                var iTrans = new Trans_Detail_2020
                                {
                                    Amount = item.Amount,
                                    AmountRecieved = item.AmountRecieved,
                                    BaseQuantity = item.BaseQuantity,
                                    BaseUnit = item.BaseUnit,
                                    BaseUnitId = item.BaseUnitId,
                                    Cash = item.Cash,
                                    CreditCard = item.CreditCard,
                                    CreditCardType = item.CreditCardType,
                                    CustomerId = item.CustomerId,
                                    CustomerName = item.CustomerName,
                                    CustomerNameAr = item.CustomerNameAr,
                                    Date = item.Date,
                                    Discount = item.Discount,
                                    EntryId = item.EntryId,
                                    GroupCD = item.GroupCD,
                                    InvDateTime = item.InvDateTime,
                                    InvoiceNumber = item.InvoiceNumber,
                                    Knet = item.Knet,
                                    Location = item.Location,
                                    LocationId = item.LocationId,
                                    NetAmount = item.NetAmount,
                                    ProdId = item.ProdId,
                                    ProductNameAr = item.ProductNameAr,
                                    ProductNameEn = item.ProductNameEn,
                                    Salesman = item.Salesman,
                                    SalesReturn = item.SalesReturn,
                                    SellQuantity = item.SellQuantity,
                                    SellUnit = item.SellUnit,
                                    SellUnitId = item.SellUnitId,
                                    UnitPrice = item.UnitPrice,
                                    Voucher = item.Voucher,
                                    VoucherId = item.VoucherId,
                                    Year = item.Year

                                };

                                newTransDetail2020.Add(iTrans);
                            }
                        }
                        if (newTransDetail2020.Any())
                        {
                            _report.Trans_Detail_2020.AddRange(newTransDetail2020);
                            _report.SaveChanges();
                        }
                    }
                }



                return new SalesReportViewModel
                {
                    SalesReportItems = salesReportItems
                };
            }
            catch (Exception ex)
            {
                var test = entryId;
                var test2 = prodId;
                throw ex;
            }
        }

        public SalesPeakHourViewModel GetSalesHourlyReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            var salesReportItems = GetSalesTransaction(fromDate, toDate, locationArray, "").SalesReportItems;
            var salesPeakHourItems = new List<SalesPeakHourItem>();
            var hoursCount = 24;

            if (locationArray != "")
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

                        if (salesPeakHourItem.Amount > 0)
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

        public string GetSales(int days)
        {
            try
            {
                var today = DateTime.Now;
                var fromDate = new DateTime(today.Year, today.Month, today.Day, 00, 00, 00).AddDays(-days);
                var toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);

                var salesReportItems = new List<SalesReportItem>();

                var transactions = GetTransactions(fromDate, toDate, fromDate, toDate);

                foreach (var transaction in transactions)
                {
                    var salesReportItem = new SalesReportItem
                    {
                        EntryId = transaction.Entry_Id,
                        Location = transaction.SM_Location.L_Short_Name,
                        LocationId = transaction.SM_Location.Locat_Cd,
                        Year = transaction.Voucher_Date.Year,
                        Date = transaction.Voucher_Date.Date,
                        InvDateTime = transaction.Voucher_Date,
                        Salesman = transaction.SM_SALESMAN.L_Sman_Name,
                        Voucher = transaction.ICS_Transaction_Types.L_Voucher_Name,
                        VoucherId = transaction.ICS_Transaction_Types.Voucher_Type,
                        InvoiceNumber = transaction.Voucher_No,
                        GroupCD = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Group_Cd : 0,
                        CustomerId = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Ldgr_Cd : 0,
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

                List<Transaction_2020> imillTrans2020 = null;
                var newTrans200 = new List<Transaction_2020>();
                foreach (var items in salesReportItems.GroupBy(x => x.Year).OrderBy(x => x.Key))
                {
                    var startDate = items.OrderBy(x => x.Date).FirstOrDefault().Date;
                    var endDate = items.OrderByDescending(x => x.Date).FirstOrDefault().Date;
                    if (items.Key == 2020)
                    {
                        imillTrans2020 = _report.Transaction_2020.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2020.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans200.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2020
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans200.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans200.Any())
                        {
                            _report.Transaction_2020.AddRange(newTrans200);
                            _report.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
                throw;
            }

            return "true";
        }

        public string GetSalesDetail(int days)
        {
            try
            {
                var today = DateTime.Now;
                var fromDate = new DateTime(today.Year, today.Month, today.Day, 00, 00, 00).AddDays(-days);
                var toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);

                var transactions = GetSalesTransaction(fromDate, toDate, "", "").SalesReportItems;

                var entryIdArray = new List<string>();
                entryIdArray.AddRange(from item in transactions select item.EntryId.ToString());

                var entryIdString = entryIdArray != null && entryIdArray.Count > 0 ? string.Join(",", entryIdArray) : "";
                var transactionDetails = _context.spICSTransDetail_GetAll(entryIdString, "").ToList();

                var salesReportItems = new List<SalesReportItem>();

                foreach (var detail in transactionDetails)
                {
                    var trans = transactions.FirstOrDefault(x => x.EntryId == detail.Entry_Id);

                    var salesReportItem = new SalesReportItem
                    {
                        EntryId = detail.Entry_Id,
                        Location = trans.Location,
                        LocationId = trans.LocationId,
                        InvDateTime = trans.InvDateTime,
                        Salesman = trans.Salesman,
                        Voucher = trans.Voucher,
                        VoucherId = trans.VoucherId,
                        InvoiceNumber = trans.InvoiceNumber,
                        GroupCD = trans.GroupCD,
                        CustomerId = trans.CustomerId,
                        CustomerName = trans.CustomerName,
                        CustomerNameAr = trans.CustomerNameAr,
                        ProdId = detail.ProdId,
                        ProductNameEn = detail.ProdEn,
                        ProductNameAr = detail.ProdAr,
                        BaseQuantity = detail.IUDBaseQty,
                        BaseUnit = detail.BaseUnit,
                        BaseUnitId = detail.BaseUnitId,
                        SellQuantity = detail.Qty,
                        SellUnit = detail.SellUnit,
                        SellUnitId = detail.SellUnitId,
                        Discount = trans.VoucherId == 202 ||
                               trans.VoucherId == 2023 ||
                               trans.VoucherId == 2035 ||
                               trans.VoucherId == 2036
                                ? -detail.FC_Prod_Dis
                                : detail.FC_Prod_Dis,
                        Amount = trans.VoucherId == 202 ||
                               trans.VoucherId == 2023 ||
                               trans.VoucherId == 2035 ||
                               trans.VoucherId == 2036
                                ? -detail.FC_Amount
                                : detail.FC_Amount,
                        Year = trans.InvDateTime.Year,
                        Date = trans.InvDateTime.Date
                    };

                    salesReportItems.Add(salesReportItem);
                }


                List<Trans_Detail_2020> trans_detail_2020 = null;
                var newTransDetail2020 = new List<Trans_Detail_2020>();

                foreach (var items in salesReportItems.GroupBy(x => x.Year).OrderBy(x => x.Key))
                {
                    var startDate = items.OrderBy(x => x.InvDateTime).FirstOrDefault().Date;
                    var endDate = items.OrderByDescending(x => x.InvDateTime).FirstOrDefault().Date;

                    if (items.Key == 2020)
                    {
                        trans_detail_2020 = _report.Trans_Detail_2020.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2020.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.ProdId == item.ProdId && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTransDetail2020.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.ProdId == item.ProdId && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Trans_Detail_2020
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTransDetail2020.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2020.Any())
                        {
                            _report.Trans_Detail_2020.AddRange(newTransDetail2020);
                            _report.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "true";
        }

        public SalesReportDashboard GetSalesRecordDashboardTrans(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            try
            {
                if (fromDate == null) fromDate = DateTime.Now;
                if (toDate == null) toDate = DateTime.Now;

                var fromHoDate = fromDate.Value.Date;
                var toHoDate = toDate.Value.Date;

                var transactions = GetSalesTransaction(fromDate, toDate, locationArray, voucherTypesArray).SalesReportItems;

                return new SalesReportDashboard
                {
                    SRItemsTrans = transactions
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetSalesDashboardTransTest(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray)
        {
            string dividedEntryCount = "";
            var count = 0;
            try
            {
                if (fromDate == null) fromDate = DateTime.Now;
                if (toDate == null) toDate = DateTime.Now;

                var fromHoDate = fromDate.Value.Date;
                var toHoDate = toDate.Value.Date;

                var transactions = GetSalesTransaction(fromDate, toDate, locationArray, voucherTypesArray).SalesReportItems;

                var entryIdArray = new List<long>();
                entryIdArray.AddRange(from item in transactions select item.EntryId);

                var dividedEntry = SplitList(entryIdArray, 5000);

                dividedEntryCount = dividedEntry.Count().ToString();

                long[] productIdArray = null;
                if (!string.IsNullOrEmpty(productStringArray))
                {
                    var productIdStringArray = productStringArray.Split(',');
                    productIdArray = Array.ConvertAll(productIdStringArray, s => long.Parse(s));
                }

                var transactionDetails = new List<Trans_Detail_2020>();


                foreach (var entries in dividedEntry)
                {
                    if (productIdArray != null && productIdArray.Length > 0)
                        transactionDetails.AddRange(_report.Trans_Detail_2020.Where(x => entries.Contains(x.EntryId.Value) && productIdArray.Contains(x.ProdId.Value)).ToList());
                    else
                        transactionDetails.AddRange(_report.Trans_Detail_2020.Where(x => entries.Contains(x.EntryId.Value)).ToList());
                    count += 1;
                }

                return "transaction was passed";

            }
            catch (Exception ex)
            {
                return ex.Message + "DEntry Count : " + dividedEntryCount + " and Count : " + count.ToString();
            }
        }

        public string GetSalesMonth(int year, int month, int from, int to)
        {
            try
            {
                // var daysInMonth = DateTime.DaysInMonth(2020, month);
                var fromDate = new DateTime(year, month, from, 00, 00, 00);
                var toDate = new DateTime(year, month, to, 23, 59, 59);

                var salesReportItems = new List<SalesReportItem>();

                var transactions = GetTransactions(fromDate, toDate, fromDate, toDate);

                foreach (var transaction in transactions)
                {
                    var salesReportItem = new SalesReportItem
                    {
                        EntryId = transaction.Entry_Id,
                        Location = transaction.SM_Location.L_Short_Name,
                        LocationId = transaction.SM_Location.Locat_Cd,
                        Year = transaction.Voucher_Date.Year,
                        Date = transaction.Voucher_Date.Date,
                        InvDateTime = transaction.Voucher_Date,
                        Salesman = transaction.SM_SALESMAN.L_Sman_Name,
                        Voucher = transaction.ICS_Transaction_Types.L_Voucher_Name,
                        VoucherId = transaction.ICS_Transaction_Types.Voucher_Type,
                        InvoiceNumber = transaction.Voucher_No,
                        GroupCD = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Group_Cd : 0,
                        CustomerId = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Ldgr_Cd : 0,
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

                foreach (var items in salesReportItems.GroupBy(x => x.Year).OrderBy(x => x.Key))
                {
                    var startDate = items.OrderBy(x => x.Date).FirstOrDefault().Date;
                    var endDate = items.OrderByDescending(x => x.Date).FirstOrDefault().Date;
                    if (items.Key == 2020)
                    {
                        List<Transaction_2020> imillTrans2020 = null;
                        var newTrans200 = new List<Transaction_2020>();

                        imillTrans2020 = _report.Transaction_2020.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2020.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans200.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2020
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans200.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans200.Any())
                        {
                            _report.Transaction_2020.AddRange(newTrans200);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2019)
                    {
                        List<Transaction_2019> imillTrans2019 = null;
                        var newTrans2019 = new List<Transaction_2019>();

                        imillTrans2019 = _report.Transaction_2019.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2019.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2019.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2019
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2019.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2019.Any())
                        {
                            _report.Transaction_2019.AddRange(newTrans2019);
                            _report.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
                throw;
            }

            return "true";
        }

        public string GetSalesDetailMonth(int year, int month, int from, int to)
        {
            try
            {
                var fromDate = new DateTime(year, month, from, 00, 00, 00);
                var toDate = new DateTime(year, month, to, 23, 59, 59);

                var transactions = year == 2020
                                        ? GetSalesTransaction(fromDate, toDate, "", "").SalesReportItems
                                        : GetSalesTransaction2019(fromDate, toDate, "", "").SalesReportItems;

                var entryIdArray = new List<string>();
                entryIdArray.AddRange(from item in transactions select item.EntryId.ToString());

                var entryIdString = entryIdArray != null && entryIdArray.Count > 0 ? string.Join(",", entryIdArray) : "";
                var transactionDetails = _context.spICSTransDetail_GetAll(entryIdString, "").ToList();

                var salesReportItems = new List<SalesReportItem>();

                foreach (var detail in transactionDetails)
                {
                    var trans = transactions.FirstOrDefault(x => x.EntryId == detail.Entry_Id);

                    var salesReportItem = new SalesReportItem
                    {
                        EntryId = detail.Entry_Id,
                        Location = trans.Location,
                        LocationId = trans.LocationId,
                        InvDateTime = trans.InvDateTime,
                        Salesman = trans.Salesman,
                        Voucher = trans.Voucher,
                        VoucherId = trans.VoucherId,
                        InvoiceNumber = trans.InvoiceNumber,
                        GroupCD = trans.GroupCD,
                        CustomerId = trans.CustomerId,
                        CustomerName = trans.CustomerName,
                        CustomerNameAr = trans.CustomerNameAr,
                        ProdId = detail.ProdId,
                        ProductNameEn = detail.ProdEn,
                        ProductNameAr = detail.ProdAr,
                        BaseQuantity = detail.IUDBaseQty,
                        BaseUnit = detail.BaseUnit,
                        BaseUnitId = detail.BaseUnitId,
                        SellQuantity = detail.Qty,
                        SellUnit = detail.SellUnit,
                        SellUnitId = detail.SellUnitId,
                        Discount = trans.VoucherId == 202 ||
                               trans.VoucherId == 2023 ||
                               trans.VoucherId == 2035 ||
                               trans.VoucherId == 2036
                                ? -detail.FC_Prod_Dis
                                : detail.FC_Prod_Dis,
                        Amount = trans.VoucherId == 202 ||
                               trans.VoucherId == 2023 ||
                               trans.VoucherId == 2035 ||
                               trans.VoucherId == 2036
                                ? -detail.FC_Amount
                                : detail.FC_Amount,
                        Year = trans.InvDateTime.Year,
                        Date = trans.InvDateTime.Date
                    };

                    salesReportItems.Add(salesReportItem);
                }

                foreach (var items in salesReportItems.GroupBy(x => x.Year).OrderBy(x => x.Key))
                {
                    var startDate = items.OrderBy(x => x.InvDateTime).FirstOrDefault().Date;
                    var endDate = items.OrderByDescending(x => x.InvDateTime).FirstOrDefault().Date;

                    if (items.Key == 2020)
                    {
                        List<Trans_Detail_2020> trans_detail_2020 = null;
                        var newTransDetail2020 = new List<Trans_Detail_2020>();

                        trans_detail_2020 = _report.Trans_Detail_2020.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2020.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.ProdId == item.ProdId && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTransDetail2020.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.ProdId == item.ProdId && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Trans_Detail_2020
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTransDetail2020.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2020.Any())
                        {
                            _report.Trans_Detail_2020.AddRange(newTransDetail2020);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2019)
                    {
                        List<Trans_Detail_2019> trans_detail_2019 = null;
                        var newTransDetail2019 = new List<Trans_Detail_2019>();

                        trans_detail_2019 = _report.Trans_Detail_2019.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2019.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.ProdId == item.ProdId && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTransDetail2019.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.ProdId == item.ProdId && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Trans_Detail_2019
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTransDetail2019.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2019.Any())
                        {
                            _report.Trans_Detail_2019.AddRange(newTransDetail2019);
                            _report.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "true";
        }

        public SalesReportViewModel GetSalesTransaction2019(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            var salesReportItems = new List<SalesReportItem>();

            var fromHoDate = fromDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? fromDate.Value.Date.AddDays(1)
                : fromDate.Value.Date;

            var toHoDate = toDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(1)
                : new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(-1);

            if (toHoDate < fromHoDate)
                toHoDate = fromHoDate.AddDays(1).AddMinutes(-1);


            var transactions = _report.Transaction_2019.Where(x => (x.InvDateTime >= fromDate &&
                                                                          x.InvDateTime <= toDate &&
                                                                          x.LocationId != 1) ||
                                                                          (x.InvDateTime >= fromHoDate &&
                                                                          x.InvDateTime <= toHoDate &&
                                                                          x.LocationId == 1)).ToList();

            if (!string.IsNullOrEmpty(locationArray))
            {
                var locationIds = (from id in locationArray.Split(',')
                                   select short.Parse(id)).ToList();
                transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value)).ToList();
            }

            if (!string.IsNullOrEmpty(voucherTypesArray))
            {
                var voucherIds = (from id in voucherTypesArray.Split(',')
                                  select short.Parse(id)).ToList();
                transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value)).ToList();
            }

            foreach (var transaction in transactions)
            {
                var salesReportItem = new SalesReportItem
                {
                    Amount = transaction.Amount.Value,
                    AmountRecieved = transaction.AmountRecieved,
                    BaseQuantity = transaction.BaseQuantity.Value,
                    BaseUnit = transaction.BaseUnit,
                    BaseUnitId = transaction.BaseUnitId.Value,
                    Cash = transaction.Cash.Value,
                    CreditCard = transaction.CreditCard.Value,
                    CreditCardType = transaction.CreditCardType,
                    CustomerId = transaction.CustomerId.Value,
                    CustomerName = transaction.CustomerName,
                    CustomerNameAr = transaction.CustomerNameAr,
                    Date = transaction.Date.Value,
                    Discount = transaction.Discount.Value,
                    EntryId = transaction.EntryId.Value,
                    GroupCD = transaction.GroupCD.Value,
                    InvDateTime = transaction.InvDateTime.Value,
                    InvoiceNumber = transaction.InvoiceNumber.Value,
                    Knet = transaction.Knet.Value,
                    Location = transaction.Location,
                    LocationId = transaction.LocationId.Value,
                    NetAmount = transaction.NetAmount.Value,
                    ProdId = transaction.ProdId.Value,
                    ProductNameAr = transaction.ProductNameAr,
                    ProductNameEn = transaction.ProductNameEn,
                    Salesman = transaction.Salesman,
                    SalesReturn = transaction.SalesReturn.Value,
                    SellQuantity = transaction.SellQuantity.Value,
                    SellUnit = transaction.SellUnit,
                    SellUnitId = transaction.SellUnitId.Value,
                    UnitPrice = transaction.UnitPrice.Value,
                    Voucher = transaction.Voucher,
                    VoucherId = transaction.VoucherId.Value,
                    Year = transaction.Year.Value
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