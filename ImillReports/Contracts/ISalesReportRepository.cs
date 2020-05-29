using ImillReports.Models;
using ImillReports.ViewModels;
using System;
using System.Collections.Generic;

namespace ImillReports.Contracts
{
    public interface ISalesReportRepository
    {
        SalesReportViewModel GetSalesReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray);
        SalesReportViewModel GetSalesDetailReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray);
        SalesPeakHourViewModel GetSalesHourlyReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypeArray);
    }
}
