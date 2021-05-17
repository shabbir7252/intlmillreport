using ImillPda.ViewModels;
using System.Collections.Generic;

namespace ImillPda.Contracts
{
    public interface IProductRepository
    {
        ProductViewModel GetAllProducts();
        List<ItemGroup> GetItemGroups();
    }
}
