using ImillReports.ViewModels;
using System;
using System.Collections.Generic;

namespace ImillReports.Contracts
{
    public interface IDashboardRepository
    {
        SalesOfMonthViewModel GetSalesOfMonth(DateTime? fromDate, DateTime? toDate);
        //string GetSalesOfMonthTest(DateTime? fromDate, DateTime? toDate);
        SalesOfMonthViewModel GetSalesRecordOfMonth(DateTime fromDate, DateTime toDate);
        SalesOfMonthViewModel GetSalesRecordDetailOfMonth(DateTime? fromDate, DateTime? toDate);
        SendEmailAsReport GetLastEmailSettings(bool isWeekly, bool isMonthly);
        void UpdateWeeklyRptTransactions(int oid);
        bool GetSettings();
        List<ReportEmailsSettings> GetEmails();
    }
}
