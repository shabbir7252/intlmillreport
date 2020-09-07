using Cash_Register.ViewModels;
using System.Collections.Generic;

namespace Cash_Register.Contracts
{
    public interface ICashRegisterRepository
    {
        int SaveCashRegister(CRegister cRegister);
        int UpdateCashRegister(CRegister cRegister);
        int GetShiftCount(ShiftCount shiftCount);
        int GetOid();
        CRegister GetCashRegister(int oid);
        List<CRegister> GetSalesmanCashRegister(short oid);
        bool DeleteCashRegRecord(int oid);
    }
}
