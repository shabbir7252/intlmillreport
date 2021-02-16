﻿using ImillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImillReports.Contracts
{
    public interface IProductRepository
    {
        ProductViewModel GetAllProducts();
        List<ItemGroup> GetItemGroups();
    }
}
