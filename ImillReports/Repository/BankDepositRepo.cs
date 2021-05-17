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
        private readonly ICashRegisterRepository _cashRegisterRepository;

        public BankDepositRepo(ImillReportsEntities context,
            IDashboardRepository dashboardRepository,
            ICashRegisterRepository cashRegisterRepository)
        {
            _context = context;
            _dashboardRepository = dashboardRepository;
            _cashRegisterRepository = cashRegisterRepository;
        }

        public List<BankDeposit> GetBankDeposits(DateTime? fromDate, DateTime? toDate)
        {

            var bankDeposits = new List<BankDeposit>();
            var dbBankDeposits = _context.BankDeposits.ToList();

            for (var i = fromDate; i <= toDate;)
            {
                var from = new DateTime(i.Value.Year, i.Value.Month, i.Value.Day, 00, 00, 00);
                var to = new DateTime(i.Value.Year, i.Value.Month, i.Value.Day, 23, 59, 00);
                var cashReg = _cashRegisterRepository.GetCashRegister(from, to, false).CashRegisterItems;

                var cashRegByLocation = cashReg.GroupBy(x => x.LocationId);
                var bankDeposit = new BankDeposit();

                foreach (var location in cashRegByLocation)
                {
                    var locationAmTotal = location.FirstOrDefault(x => x.ShiftType == "am") != null ?
                        location.FirstOrDefault(x => x.ShiftType == "am").NetAmount ?? 0
                        : 0;
                    var locationPmTotal = location.FirstOrDefault(x => x.ShiftType == "pm") != null
                        ? location.FirstOrDefault(x => x.ShiftType == "pm").NetAmount ?? 0
                        : 0;
                    var locationTotal = location.Sum(x => x.NetAmount);

                    switch (location.Key)
                    {
                        case 54:
                            bankDeposit.Avenues = locationTotal.Value;
                            bankDeposit.AvenuesAm = locationAmTotal;
                            bankDeposit.AvenuesPm = locationPmTotal;
                            break;
                        case 55:
                            bankDeposit.Dasma = locationTotal.Value;
                            bankDeposit.DasmaAm = locationAmTotal;
                            bankDeposit.DasmaPm = locationPmTotal;
                            break;
                        case 56:
                            bankDeposit.Salhiyah = locationTotal.Value;
                            bankDeposit.SalhiyahAm = locationAmTotal;
                            bankDeposit.SalhiyahPm = locationPmTotal;
                            break;
                        case 57:
                            bankDeposit.Dahiya = locationTotal.Value;
                            bankDeposit.DahiyaAm = locationAmTotal;
                            bankDeposit.DahiyaPm = locationPmTotal;
                            break;
                        case 58:
                            bankDeposit.Kaifan = locationTotal.Value;
                            bankDeposit.KaifanAm = locationAmTotal;
                            bankDeposit.KaifanPm = locationPmTotal;
                            break;
                        case 59:
                            bankDeposit.Nuzha = locationTotal.Value;
                            bankDeposit.NuzhaAm = locationAmTotal;
                            bankDeposit.NuzhaPm = locationPmTotal;
                            break;
                        case 60:
                            bankDeposit.Khaldiya = locationTotal.Value;
                            bankDeposit.KhaldiyaAm = locationAmTotal;
                            bankDeposit.KhaldiyaPm = locationPmTotal;
                            break;
                        case 61:
                            bankDeposit.Qortuba = locationTotal.Value;
                            bankDeposit.QortubaAm = locationAmTotal;
                            bankDeposit.QortubaPm = locationPmTotal;
                            break;
                        case 62:
                            bankDeposit.Yarmouk = locationTotal.Value;
                            bankDeposit.YarmoukAm = locationAmTotal;
                            bankDeposit.YarmoukPm = locationPmTotal;
                            break;
                        case 64:
                            bankDeposit.Zahara = locationTotal.Value;
                            bankDeposit.ZaharaAm = locationAmTotal;
                            bankDeposit.ZaharaPm = locationPmTotal;
                            break;
                        case 65:
                            bankDeposit.Mall360 = locationTotal.Value;
                            bankDeposit.Mall360Am = locationAmTotal;
                            bankDeposit.Mall360Pm = locationPmTotal;
                            break;
                        case 66:
                            bankDeposit.Gate = locationTotal.Value;
                            bankDeposit.GateAm = locationAmTotal;
                            bankDeposit.GatePm = locationPmTotal;
                            break;
                        case 68:
                            bankDeposit.KPC = locationTotal.Value;
                            bankDeposit.KPCAm = locationAmTotal;
                            bankDeposit.KPCPm = locationPmTotal;
                            break;
                        case 74:
                            bankDeposit.Shahuda = locationTotal.Value;
                            bankDeposit.ShahudaAm = locationAmTotal;
                            bankDeposit.ShahudaPm = locationPmTotal;
                            break;
                        case 76:
                            bankDeposit.Salmiya = locationTotal.Value;
                            bankDeposit.SalmiyaAm = locationAmTotal;
                            bankDeposit.SalmiyaPm = locationPmTotal;
                            break;
                        case 77:
                            bankDeposit.Surra = locationTotal.Value;
                            bankDeposit.SurraAm = locationAmTotal;
                            bankDeposit.SurraPm = locationPmTotal;
                            break;
                        case 78:
                            bankDeposit.Hamra = locationTotal.Value;
                            bankDeposit.HamraAm = locationAmTotal;
                            bankDeposit.HamraPm = locationPmTotal;
                            break;
                        case 80:
                            bankDeposit.Bnied = locationTotal.Value;
                            bankDeposit.BniedAm = locationAmTotal;
                            bankDeposit.BniedPm = locationPmTotal;
                            break;
                        case 81:
                            bankDeposit.Jahra = locationTotal.Value;
                            bankDeposit.JahraAm = locationAmTotal;
                            bankDeposit.JahraPm = locationPmTotal;
                            break;
                        case 82:
                            bankDeposit.Kout = locationTotal.Value;
                            bankDeposit.KoutAm = locationAmTotal;
                            bankDeposit.KoutPm = locationPmTotal;
                            break;
                        case 83:
                            bankDeposit.Av4 = locationTotal.Value;
                            bankDeposit.Av4Am = locationAmTotal;
                            bankDeposit.Av4Pm = locationPmTotal;
                            break;
                        case 85:
                            bankDeposit.Fintas = locationTotal.Value;
                            bankDeposit.FintasAm = locationAmTotal;
                            bankDeposit.FintasPm = locationPmTotal;
                            break;
                    }
                }

                bankDeposit.TransDate = i.Value;

                var salesOfMonth = _dashboardRepository.GetSalesRecordOfMonth(from, to);
                var totalCashSales = salesOfMonth.TotalBranchCash ?? 0;

                bankDeposit.Innova = totalCashSales;

                var totalCash = bankDeposit.Avenues + bankDeposit.Dasma + bankDeposit.Salhiyah + bankDeposit.Dahiya +
                    bankDeposit.Kaifan + bankDeposit.Nuzha + bankDeposit.Khaldiya + bankDeposit.Qortuba + bankDeposit.Yarmouk +
                    bankDeposit.Zahara + bankDeposit.Mall360 + bankDeposit.Gate + bankDeposit.KPC + bankDeposit.Shahuda +
                    bankDeposit.Salmiya + bankDeposit.Surra + bankDeposit.Hamra + bankDeposit.Bnied + bankDeposit.Jahra +
                    bankDeposit.Kout + bankDeposit.Avenues + bankDeposit.Fintas;

                bankDeposit.TotalCash = totalCash;

                var dbBankDeposit = dbBankDeposits.Any(x => x.TransDate == i.Value) ? dbBankDeposits.FirstOrDefault(x => x.TransDate == i.Value) : null;

                if (dbBankDeposit != null)
                {
                    bankDeposit.Oid = dbBankDeposit.Oid;
                    bankDeposit.Misc = dbBankDeposit.Misc;
                    bankDeposit.Misc2 = dbBankDeposit.Misc2;
                    bankDeposit.DepositBy = dbBankDeposit.DepositBy;
                    bankDeposit.DepositBy = dbBankDeposit.DepositBy;
                    bankDeposit.Cash = dbBankDeposit.Cash;
                    bankDeposit.Comments = dbBankDeposit.Comments;
                }

                bankDeposits.Add(bankDeposit);

                i = i.Value.AddDays(1);
            }

            return bankDeposits;
        }

        public bool SaveTransaction(DateTime transDate, long oid, decimal misc, decimal misc2, decimal cash, string depositby, string comments)
        {
            try
            {
                if (oid <= 0)
                {
                    var bankDeposit = new BankDeposit
                    {
                        TransDate = transDate,
                        Misc = misc,
                        Misc2 = misc2,
                        DepositBy = depositby,
                        Cash = cash,
                        Comments = comments
                    };


                    _context.BankDeposits.Add(bankDeposit);
                    _context.SaveChanges();
                }
                else
                {
                    var dbBankDeposit = _context.BankDeposits.FirstOrDefault(x => x.Oid == oid);

                    dbBankDeposit.Misc = misc;
                    dbBankDeposit.Misc2 = misc2;
                    dbBankDeposit.Cash = cash;
                    dbBankDeposit.DepositBy = depositby;
                    dbBankDeposit.Comments = comments;

                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        private void SyncCashRegAndInnova(DateTime? fromDate, DateTime? toDate)
        {
            var bankDeposits = new List<BankDeposit>();
            var dbBankDeposits = _context.BankDeposits.ToList();

            for (var i = fromDate; i <= toDate;)
            {
                var dbBankDeposit = dbBankDeposits.Any(x => x.TransDate == i.Value)
                    ? dbBankDeposits.FirstOrDefault(x => x.TransDate == i.Value)
                    : null;

                if (dbBankDeposit == null)
                {
                    var from = new DateTime(i.Value.Year, i.Value.Month, i.Value.Day, 00, 00, 00);
                    var to = new DateTime(i.Value.Year, i.Value.Month, i.Value.Day, 23, 59, 00);
                    var cashReg = _cashRegisterRepository.GetCashRegister(from, to, false).CashRegisterItems;

                    var cashRegByLocation = cashReg.GroupBy(x => x.LocationId);
                    var bankDeposit = new BankDeposit();

                    foreach (var location in cashRegByLocation)
                    {
                        var locationAmTotal = location.FirstOrDefault(x => x.ShiftType == "am") != null ?
                            location.FirstOrDefault(x => x.ShiftType == "am").NetAmount ?? 0
                            : 0;
                        var locationPmTotal = location.FirstOrDefault(x => x.ShiftType == "pm") != null
                            ? location.FirstOrDefault(x => x.ShiftType == "pm").NetAmount ?? 0
                            : 0;
                        var locationTotal = location.Sum(x => x.NetAmount);

                        switch (location.Key)
                        {
                            case 54:
                                bankDeposit.Avenues = locationTotal.Value;
                                bankDeposit.AvenuesAm = locationAmTotal;
                                bankDeposit.AvenuesPm = locationPmTotal;
                                break;
                            case 55:
                                bankDeposit.Dasma = locationTotal.Value;
                                bankDeposit.DasmaAm = locationAmTotal;
                                bankDeposit.DasmaPm = locationPmTotal;
                                break;
                            case 56:
                                bankDeposit.Salhiyah = locationTotal.Value;
                                bankDeposit.SalhiyahAm = locationAmTotal;
                                bankDeposit.SalhiyahPm = locationPmTotal;
                                break;
                            case 57:
                                bankDeposit.Dahiya = locationTotal.Value;
                                bankDeposit.DahiyaAm = locationAmTotal;
                                bankDeposit.DahiyaPm = locationPmTotal;
                                break;
                            case 58:
                                bankDeposit.Kaifan = locationTotal.Value;
                                bankDeposit.KaifanAm = locationAmTotal;
                                bankDeposit.KaifanPm = locationPmTotal;
                                break;
                            case 59:
                                bankDeposit.Nuzha = locationTotal.Value;
                                bankDeposit.NuzhaAm = locationAmTotal;
                                bankDeposit.NuzhaPm = locationPmTotal;
                                break;
                            case 60:
                                bankDeposit.Khaldiya = locationTotal.Value;
                                bankDeposit.KhaldiyaAm = locationAmTotal;
                                bankDeposit.KhaldiyaPm = locationPmTotal;
                                break;
                            case 61:
                                bankDeposit.Qortuba = locationTotal.Value;
                                bankDeposit.QortubaAm = locationAmTotal;
                                bankDeposit.QortubaPm = locationPmTotal;
                                break;
                            case 62:
                                bankDeposit.Yarmouk = locationTotal.Value;
                                bankDeposit.YarmoukAm = locationAmTotal;
                                bankDeposit.YarmoukPm = locationPmTotal;
                                break;
                            case 64:
                                bankDeposit.Zahara = locationTotal.Value;
                                bankDeposit.ZaharaAm = locationAmTotal;
                                bankDeposit.ZaharaPm = locationPmTotal;
                                break;
                            case 65:
                                bankDeposit.Mall360 = locationTotal.Value;
                                bankDeposit.Mall360Am = locationAmTotal;
                                bankDeposit.Mall360Pm = locationPmTotal;
                                break;
                            case 66:
                                bankDeposit.Gate = locationTotal.Value;
                                bankDeposit.GateAm = locationAmTotal;
                                bankDeposit.GatePm = locationPmTotal;
                                break;
                            case 68:
                                bankDeposit.KPC = locationTotal.Value;
                                bankDeposit.KPCAm = locationAmTotal;
                                bankDeposit.KPCPm = locationPmTotal;
                                break;
                            case 74:
                                bankDeposit.Shahuda = locationTotal.Value;
                                bankDeposit.ShahudaAm = locationAmTotal;
                                bankDeposit.ShahudaPm = locationPmTotal;
                                break;
                            case 76:
                                bankDeposit.Salmiya = locationTotal.Value;
                                bankDeposit.SalmiyaAm = locationAmTotal;
                                bankDeposit.SalmiyaPm = locationPmTotal;
                                break;
                            case 77:
                                bankDeposit.Surra = locationTotal.Value;
                                bankDeposit.SurraAm = locationAmTotal;
                                bankDeposit.SurraPm = locationPmTotal;
                                break;
                            case 78:
                                bankDeposit.Hamra = locationTotal.Value;
                                bankDeposit.HamraAm = locationAmTotal;
                                bankDeposit.HamraPm = locationPmTotal;
                                break;
                            case 80:
                                bankDeposit.Bnied = locationTotal.Value;
                                bankDeposit.BniedAm = locationAmTotal;
                                bankDeposit.BniedPm = locationPmTotal;
                                break;
                            case 81:
                                bankDeposit.Jahra = locationTotal.Value;
                                bankDeposit.JahraAm = locationAmTotal;
                                bankDeposit.JahraPm = locationPmTotal;
                                break;
                            case 82:
                                bankDeposit.Kout = locationTotal.Value;
                                bankDeposit.KoutAm = locationAmTotal;
                                bankDeposit.KoutPm = locationPmTotal;
                                break;
                            case 83:
                                bankDeposit.Av4 = locationTotal.Value;
                                bankDeposit.Av4Am = locationAmTotal;
                                bankDeposit.Av4Pm = locationPmTotal;
                                break;
                            case 85:
                                bankDeposit.Fintas = locationTotal.Value;
                                bankDeposit.FintasAm = locationAmTotal;
                                bankDeposit.FintasPm = locationPmTotal;
                                break;
                        }
                    }

                    bankDeposit.TransDate = i.Value;

                    var salesOfMonth = _dashboardRepository.GetSalesRecordOfMonth(from, to);
                    var totalCashSales = salesOfMonth.TotalBranchCash ?? 0;

                    bankDeposit.Innova = totalCashSales;

                    var totalCash = bankDeposit.Avenues + bankDeposit.Dasma + bankDeposit.Salhiyah + bankDeposit.Dahiya +
                        bankDeposit.Kaifan + bankDeposit.Nuzha + bankDeposit.Khaldiya + bankDeposit.Qortuba + bankDeposit.Yarmouk +
                        bankDeposit.Zahara + bankDeposit.Mall360 + bankDeposit.Gate + bankDeposit.KPC + bankDeposit.Shahuda +
                        bankDeposit.Salmiya + bankDeposit.Surra + bankDeposit.Hamra + bankDeposit.Bnied + bankDeposit.Jahra +
                        bankDeposit.Kout + bankDeposit.Avenues + bankDeposit.Fintas;

                    bankDeposit.TotalCash = totalCash;


                    bankDeposit.Oid = dbBankDeposit.Oid;
                    bankDeposit.Misc = dbBankDeposit.Misc;
                    bankDeposit.Misc2 = dbBankDeposit.Misc2;
                    bankDeposit.DepositBy = dbBankDeposit.DepositBy;
                    bankDeposit.DepositBy = dbBankDeposit.DepositBy;
                    bankDeposit.Cash = dbBankDeposit.Cash;
                    bankDeposit.Comments = dbBankDeposit.Comments;
                    
                    bankDeposits.Add(bankDeposit);
                }


                i = i.Value.AddDays(1);
            }
        }
    }
}