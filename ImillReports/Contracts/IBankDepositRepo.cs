using System;
using ImillReports.Models;
using System.Collections.Generic;

namespace ImillReports.Contracts
{
    public interface IBankDepositRepo
    {
        List<BankDeposit> GetBankDeposits(DateTime? fromDate, DateTime? toDate);
        bool SaveTransaction(DateTime transDate, long oid, decimal misc, decimal misc2, decimal cash, 
            string depositby, string comments);
    }
}
