using ImillPda.Contracts;
using ImillPda.Models;
using ImillPda.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImillPda.Repository
{
    public class ItemRepo : IItemRepo
    {
        private readonly IMILLEntities _context = new IMILLEntities();

        public ItemRepo(IMILLEntities context)
        {
            _context = context;
        }

        public IQueryable<ItemVm> GetItems()
        {
            IQueryable<ItemVm> dbItems = null;
            try
            {
                dbItems = _context.ICS_Item.Select(x => new ItemVm
                {
                    Part_No = x.Part_No,
                    Prod_Cd = x.Prod_Cd,
                    L_Prod_Name = x.L_Prod_Name,
                    A_Prod_Name = x.A_Prod_Name,
                    Item_Type_Cd = x.Item_Type_Cd,
                    GroupCd = x.Group_Cd
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return dbItems;
        }
    }
}