using ImillReports.Contracts;
using ImillReports.Models;
using ImillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.Repository
{
    public class BankDepositRepo : IBankDepositRepo
    {
        private readonly ImillReportsEntities _context;
        private readonly IDashboardRepository _dashboardRepository;

        public BankDepositRepo(ImillReportsEntities context, IDashboardRepository dashboardRepository)
        {
            _context = context;
            _dashboardRepository = dashboardRepository;
        }

        public List<BankDeposit> GetBankDeposits(DateTime? fromDate, DateTime? toDate)
        {
            return _context.BankDeposits.ToList();
        }

        public bool SaveTransaction(DateTime transDate, decimal salheya, decimal salmiya, decimal nuzha, decimal dahiya,
            decimal khaldiya, decimal qortuba, decimal kaifan, decimal avenues, decimal kpc, decimal yarmouk, decimal zahara,
            decimal mall360, decimal dasma, decimal gate, decimal shahuda, decimal surra, decimal hamra, decimal bnied, decimal jahra,
            decimal kout, decimal av4, decimal fintas, decimal misc, decimal misc2, decimal cash, string depositby, string comments)
        {
            try
            {

                var totalCash = salheya + salmiya + nuzha + dahiya + khaldiya + qortuba + kaifan + avenues + kpc + 
                                yarmouk + zahara + mall360 + dasma + gate + shahuda + surra + hamra + bnied + 
                                jahra + kout + av4 + fintas;

                var fromDate = new DateTime(transDate.Year, transDate.Month, transDate.Day, 03, 00, 00);
                var toDate = new DateTime(transDate.Year, transDate.Month, transDate.Day, 02, 59, 00).AddDays(1);

                var salesOfMonth = _dashboardRepository.GetSalesRecordOfMonth(fromDate, toDate);
                var totalCashSales = salesOfMonth.TotalBranchCash ?? 0;

                var bankDeposit = new BankDeposit
                {
                    TransDate = transDate,
                    Salhiyah = salheya,
                    Av4 = av4,
                    Avenues = avenues,
                    Bnied = bnied,
                    Cash = cash,
                    Comments = comments,
                    Dahiya = dahiya,
                    Dasma = dasma,
                    DepositBy = depositby,
                    Fintas = fintas,
                    Gate = gate,
                    Jahra = jahra,
                    Kaifan = kaifan,
                    Khaldiya = khaldiya,
                    Kout = kout,
                    KPC = kpc,
                    Mall360 = mall360,
                    Misc = misc,
                    Misc2 = misc2,
                    Nuzha = nuzha,
                    Qortuba = qortuba,
                    Salmiya = salmiya,
                    Shahuda = shahuda,
                    Surra = surra,
                    Yarmouk = yarmouk,
                    Zahara = zahara,
                    TotalCash = totalCash,
                    Innova = totalCashSales
                };


                _context.BankDeposits.Add(bankDeposit);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return true;
        }
    }
}