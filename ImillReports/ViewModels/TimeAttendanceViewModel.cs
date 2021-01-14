using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.ViewModels
{
    public class TimeAttendanceViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DeviceCode { get; set; }
        public DateTime? TransDate { get; set; }
        public string Location { get; internal set; }
        public DateTime? PunchIn { get; internal set; }
        public DateTime? PunchOut { get; internal set; }
        public int LocationId { get; internal set; }
        public TimeSpan LateIn { get; set; }
        public TimeSpan EarlyOut { get; internal set; }
        public TimeSpan? ShiftEnd { get; internal set; }
        public TimeSpan? ShiftStart { get; internal set; }
        public TimeSpan TotalHoursWorked { get; set; }
        public string PersonId { get; set; }
        public bool IsOpened { get; internal set; }
        public string Temp { get; internal set; }
        public decimal? TotalWorkingHours { get; internal set; }
        public decimal? TemperatureIn { get; internal set; }
        public decimal? TemperatureOut { get; internal set; }
        public int Oid { get; internal set; }
        public string EmployeeNameAr { get; internal set; }
        public string ConvTotalHoursWorked { get; internal set; }
        public string IsLateIn { get; internal set; }
        public string IsLateIn5Min { get; internal set; }
        public string IsEarlyOut { get; internal set; }
        public string IsEarlyOut5Min { get; internal set; }
        public string IsOnLeave { get; internal set; }
    }

    public class AllocationMapViewModel
    {
        public int Oid { get; set; }
        public int EmpId { get; set; }
        public string EmpName { get; set; }
        public string EmpNameAr { get; set; }
        public int LocationOid { get; set; }
        public string Location { get; set; }
        public string LocationAr { get; set; }
        public int ShiftOid { get; set; }
        public string ShiftCode { get; set; }
        public string ShiftName { get; set; }
        public string ShiftNameAr { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime? FromDate { get; internal set; }
        public DateTime? ToDate { get; internal set; }
        public bool? Sat { get; internal set; }
        public bool? Sun { get; internal set; }
        public bool? Mon { get; internal set; }
        public bool? Tues { get; internal set; }
        public bool? Wed { get; internal set; }
        public bool? Thur { get; internal set; }
        public bool? Fri { get; internal set; }
    }

    public class EmployeeViewModel
    {
        public int Oid { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public int EmployeeId { get; set; }
        public string DisplayText { get; set; }
        public bool IsSelected { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}