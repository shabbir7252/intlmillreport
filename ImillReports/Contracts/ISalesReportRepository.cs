using System;
using ImillReports.ViewModels;
using System.Collections.Generic;

namespace ImillReports.Contracts
{
    public interface ISalesReportRepository
    {
        SalesReportViewModel GetSalesReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray);
        SalesReportViewModel GetSalesTransaction(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray);
        SalesReportViewModel GetSalesDetailReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray);
        List<TransDetailsViewModel> GetSalesDetailTransaction(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray);
        SalesReportDashboard GetSalesDashboardTransaction(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray, bool showGroupCD);
        SalesPeakHourViewModel GetSalesHourlyReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypeArray);
        string GetSales(int days);
        string GetSalesDetail(int days);

        SalesReportDashboard GetSalesRecordDashboardTrans(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray);



        string GetSalesDashboardTransTest(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray);

        string GetSalesMonth(int year, int month, int from, int to);
        string GetSalesDetailMonth(int year, int month, int from, int to);
        List<DailyConsumptionVM> GetDailyConsumptionTrans(DateTime? fromDate, DateTime? toDate);
        void GetReportByItemGroup(DateTime? fromDate, DateTime? toDate);
        SalesLocationTrendsViewModel GetMonthlyReportLocationWise(string locationsString, int year);
        SalesLocationTrendsViewModel GetYearlyReportLocationWise(string locationsString);
        SalesLocationTrendsViewModel GetMonthYearReportLocationWise(string locationsString, int month);
        SalesLocationTrendsViewModel GetWeeklyReportLocationWise(string locationsString, DateTime? fromDate, DateTime? toDate);
        SalesLocationTrendsViewModel GetMonthlyReportItemWise(string productString, int year);
        SalesLocationTrendsViewModel GetWeeklyReportItemWise(string productString, DateTime? fromDate, DateTime? toDate);
        SalesLocationTrendsViewModel GetYearlyReportItemWise(string productString);
        SalesLocationTrendsViewModel GetMonthYearReportItemWise(string productString, int month);
    }
}
