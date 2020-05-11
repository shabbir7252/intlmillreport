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
        // List<spICSTransDetail2_GetAll_Result> GetSalesDetailReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray);
    }
}
