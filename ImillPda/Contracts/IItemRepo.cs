using System.Linq;
using ImillPda.ViewModels;

namespace ImillPda.Contracts
{
    public interface IItemRepo
    {
        IQueryable<ItemVm> GetItems();
    }
}
