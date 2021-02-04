using ImillReports.ViewModels;
using System;

namespace ImillReports.Contracts
{
    public interface IDashboardRepository
    {
        SalesOfMonthViewModel GetSalesOfMonth(DateTime? fromDate, DateTime? toDate);
        //string GetSalesOfMonthTest(DateTime? fromDate, DateTime? toDate);
        SalesOfMonthViewModel GetSalesRecordOfMonth(DateTime fromDate, DateTime toDate);
        SalesOfMonthViewModel GetSalesRecordDetailOfMonth(DateTime? fromDate, DateTime? toDate);
        SendEmailAsReport GetLastEmailSettings();
        void SetWeeklyRptEmailDate();
    }
}
