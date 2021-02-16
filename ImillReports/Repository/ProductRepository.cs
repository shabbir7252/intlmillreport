using ImillReports.Contracts;
using ImillReports.Models;
using ImillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMILLEntities _context;

        public ProductRepository(IMILLEntities context)
        {
            _context = context;
        }

        public ProductViewModel GetAllProducts()
        {
            var items = _context.ICS_Item.Where(a => !a.Discontinued);

            var prodItems = from item in items
                            let prodItem = new Item
                            {
                                ProductId = item.Prod_Cd,
                                Name = item.L_Prod_Name,
                                NameAr = item.A_Prod_Name,
                                GroupCd = item.Group_Cd
                            }
                            select prodItem;

            return new ProductViewModel
            {
                Items = prodItems.ToList()
            };
        }

        public List<ItemGroup> GetItemGroups()
        {
            var itemGroups = new List<ItemGroup>();
            
            try
            {
                var dbItemGroup = _context.ICS_Item_Group;

                foreach (var item in dbItemGroup)
                {
                    var groupName = "";
                    var groupNameAr = "";

                    if (item.M_Group_Cd != 0)
                    {
                        groupName = dbItemGroup.FirstOrDefault(x => x.Group_Cd == item.M_Group_Cd).L_Group_Name;
                        groupNameAr = dbItemGroup.FirstOrDefault(x => x.Group_Cd == item.M_Group_Cd).A_Group_Name;
                    }

                    itemGroups.Add(new ItemGroup
                    {
                        GroupNumber = item.Group_No,
                        ItemGroupId = item.Group_Cd,
                        ParentGroupId = item.M_Group_Cd,
                        GroupName = groupName,
                        GroupNameAr = groupName,
                        Name = item.L_Group_Name,
                        NameAr = item.A_Group_Name
                    });
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            return itemGroups;
        }
    }
}