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
        List<TimeAttendanceViewModel> GetTAReport(DateTime? fromDate, DateTime? toDate, int[] employees, string type);
        string SyncTAReport(int year, int month, int from, int toYear, int toMonth, int to);
        List<ShiftViewModel> GetShifts();
        List<TaLocation> GetLocations();
        List<AllocationMapViewModel> GetEmpAllocations();
        string UpdateEmpAllocations(string locations, string shifts, List<int> verifiedIds, DateTime fromDate, DateTime? toDate,
            bool sun, bool mon, bool tues, bool wed, bool thur, bool fri, bool sat);
        List<EmployeeViewModel> GetEmployees();
        int AddEmployee(int empId, string nameEn, string nameAr);
        string UpdateEmployee(int oid, int empId, string nameEn, string nameAr);
        void DeleteEmployees(List<int> verifiedIds);
        void AddLocations(string deviceCode, string nameEn, string nameAr);
        void DeleteLocations(List<int> verifiedIds);
        void AddShift(string code, TimeSpan startTime, TimeSpan endTime);
        void DeleteShifts(List<int> verifiedIds);
        string AddAllocation(DateTime fromDate, DateTime? toDate, List<int> employees, string shift, string location, 
            bool sun, bool mon, bool tues, bool wed, bool thur, bool fri, bool sat);
        void DeleteAllocations(List<int> verifiedIds);
        void DeleteTransactions(List<int> verifiedIds);
        string UpdateLocation(int oid, string deviceCode, string nameEn, string nameAr);
        string SendShiftEmail();
        string AddEmployeeLeaves(DateTime from, DateTime to, List<int> employees);
        List<EmployeeViewModel> GetEmployeeLeaves();
        void DeleteEmployeeLeaves(List<int> verifiedIds);
        string SendShiftStartDetailReport();
        string SendShiftEndDetailReport();
        string SyncHoDevice(DateTime? fromDate, DateTime? toDate, string ipAddress);
    }
}
