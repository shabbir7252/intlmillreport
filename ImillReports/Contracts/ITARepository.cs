using ImillReports.Models;
using ImillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImillReports.Contracts
{
    public interface ITARepository
    {
        List<TimeAttendanceViewModel> GetTAReport(DateTime? fromDate, DateTime? toDate);
        string SyncTAReport(int year, int month, int from, int toYear, int toMonth, int to);
        List<ShiftViewModel> GetShifts();
        List<TaLocation> GetLocations();
        List<AllocationMapViewModel> GetEmpAllocations();
        void UpdateEmpAllocations(string locations, string shifts, List<int> verifiedIds);
        List<EmployeeViewModel> GetEmployees();
        int AddEmployee(int empId, string nameEn, string nameAr);
        void DeleteEmployees(List<int> verifiedIds);
        void AddLocations(string deviceCode, string nameEn, string nameAr);
        void DeleteLocations(List<int> verifiedIds);
        void AddShift(string code, TimeSpan startTime, TimeSpan endTime);
        void DeleteShifts(List<int> verifiedIds);
        int AddAllocation(DateTime fromDate, DateTime? toDate, List<int> employees, string shift, string location);
        void DeleteAllocations(List<int> verifiedIds);
        void DeleteTransactions(List<int> verifiedIds);
    }
}
