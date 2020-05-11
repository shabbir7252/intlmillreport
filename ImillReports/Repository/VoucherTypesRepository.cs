using ImillReports.Contracts;
using ImillReports.Models;
using ImillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.Repository
{
    public class VoucherTypesRepository : IVoucherTypesRepository
    {
        private readonly IMILLEntities _context;

        public VoucherTypesRepository(IMILLEntities context)
        {
            _context = context;
        }
        public VoucherTypeViewModel GetSalesVoucherTypes()
        {
            var voucherTypes = _context.ICS_Transaction_Types
                 .Where(x => x.Voucher_Type == 201 ||
                             x.Voucher_Type == 2021 ||
                             x.Voucher_Type == 2022 ||
                             x.Voucher_Type == 2025 ||
                             x.Voucher_Type == 2026 ||
                             // below are sales return type Id
                             x.Voucher_Type == 202 ||
                             x.Voucher_Type == 2023 ||
                             x.Voucher_Type == 2035 ||
                             x.Voucher_Type == 2036);

            var voucherTypeItems = new List<VoucherTypeItem>();

            foreach (var type in voucherTypes)
            {
                var voucherTypeItem = new VoucherTypeItem
                {
                    Voucher_Type = type.Voucher_Type,
                    L_Voucher_Name = type.L_Voucher_Name,
                    A_Voucher_Name = type.A_Voucher_Name
                };
                voucherTypeItems.Add(voucherTypeItem);
            }

            return new VoucherTypeViewModel
            {
                VoucherTypeItems = voucherTypeItems
            };
        }
    }
}