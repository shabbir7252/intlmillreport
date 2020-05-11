using ImillReports.ViewModels;
using System;

namespace ImillReports.Contracts
{
    public interface IDashboardRepository
    {
        SalesOfMonthViewModel GetSalesOfMonth(DateTime? fromDate, DateTime? toDate);
    }
}
