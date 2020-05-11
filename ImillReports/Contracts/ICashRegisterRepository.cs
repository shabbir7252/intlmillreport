using ImillReports.ViewModels;
using System;

namespace ImillReports.Contracts
{
    public interface ICashRegisterRepository
    {
        CashRegisterViewModel GetCashRegister(DateTime? fromDate, DateTime? toDate, bool takeCount);
        CashRegVsSalesViewModel GetCashRegisterVsSalesRpt(DateTime? fromDate, DateTime? toDate);

    }
}
