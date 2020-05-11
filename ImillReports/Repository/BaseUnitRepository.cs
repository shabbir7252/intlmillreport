using ImillReports.Contracts;
using ImillReports.Models;
using ImillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.Repository
{
    public class BaseUnitRepository : IBaseUnitRepository
    {
        private readonly IMILLEntities _context;

        public BaseUnitRepository(IMILLEntities context)
        {
            _context = context;
        }
        public BaseUnitViewModel GetBaseUnits()
        {
            var baseUnits = _context.ICS_Unit;

            var baseUnitItems = new List<BaseUnitItem>();

            foreach(var unit in baseUnits)
            {
                var baseUnitItem = new BaseUnitItem
                {
                    Unit_Cd = unit.Unit_Cd,
                    L_Unit_Name = unit.L_Unit_Name,
                    A_Unit_Name = unit.A_Unit_Name
                };

                baseUnitItems.Add(baseUnitItem);
            }

            return new BaseUnitViewModel
            {
                BaseUnitItems = baseUnitItems
            };
        }
    }
}