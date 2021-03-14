using System;
using ImillReports.Models;
using System.Collections.Generic;

namespace ImillReports.Contracts
{
    public interface IBankDepositRepo
    {
        List<BankDeposit> GetBankDeposits(DateTime? fromDate, DateTime? toDate);
        bool SaveTransaction(DateTime transDate, decimal salheya, decimal salmiya, decimal nuzha, decimal dahiya, 
            decimal khaldiya, decimal qortuba, decimal kaifan, decimal avenues, decimal kpc, decimal yarmouk, 
            decimal zahara, decimal mall360, decimal dasma, decimal gate, decimal shahuda, decimal surra, decimal hamra, decimal bnied, 
            decimal jahra, decimal kout, decimal av4, decimal fintas, decimal misc, decimal misc2, decimal cash, 
            string depositby, string comments);
    }
}
