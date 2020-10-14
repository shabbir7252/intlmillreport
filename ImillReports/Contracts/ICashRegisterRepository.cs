using ImillReports.ViewModels;
using System;
using System.Collections.Generic;

namespace ImillReports.Contracts
{
    public interface ICashRegisterRepository
    {
        CashRegisterViewModel GetCashRegister(DateTime? fromDate, DateTime? toDate, bool takeCount);
        CashRegVsSalesViewModel GetCashRegisterVsSalesRpt(DateTime? fromDate, DateTime? toDate);

        bool UpdateVerifiedIds(List<int> verifiedIds, List<int> deVerifiedIds);
        string Update(CashRegUpdateVM cashRegUpdateVM);
    }
}
