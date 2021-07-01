using System;
using System.Net;
using System.Web;
using System.Linq;
using System.Data;
using System.Net.Mail;
using ClosedXML.Excel;
using Newtonsoft.Json;
using System.Data.Entity;
using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;

namespace ImillReports.Repository
{
    public class TARepository : ITARepository
    {
        private readonly FCCCentralizedDBEntities1 _context;

        public TARepository(FCCCentralizedDBEntities1 context)
        {
            _context = context;
        }

        public string SyncTAReport(int year, int month, int from, int toYear, int toMonth, int to)
        {
            var fromDate = new DateTime(year, month, from, 00, 00, 00).AddDays(-1);
            var toDate = new DateTime(toYear, toMonth, to, 23, 59, 59).AddHours(12);

            var fetchedTrans = _context.tbl_Transactions.Where(a => a.TransDate >= fromDate && a.TransDate <= toDate);
            _context.tbl_Transactions.RemoveRange(fetchedTrans);
            _context.SaveChanges();

            // var fDate = fromDate;
            // var tDate = toDate.AddDays(1);

            var deviceCodes = _context.tbl_DeviceCode.ToList();
            var empTransactions = _context.tbl_EmpTransaction.Where(x => x.TransactionDateTime >= fromDate &&
                                                                         x.TransactionDateTime <= toDate)
                                                             .GroupBy(a => a.EmployeeID).ToList();

            var employeeLeaves = _context.tbl_EmployeeLeaves.ToList();

            // var resultFlag = await _context.StableAllotment.AnyAsync(x =>
            // x.StableRoom == stableRoom && x.StartDate <= model.StartDate && x.EndDate >= model.StartDate ||
            // x.StartDate <= model.EndDate && x.EndDate >= model.EndDate);

            var taModels = new List<TimeAttendanceViewModel>();

            var locations = _context.tbl_Location;

            foreach (var item in empTransactions)
            {
                // var flag = false;
                for (var i = fromDate.Date; i <= toDate.Date;)
                {
                    var iFirstTime = new DateTime(i.Year, i.Month, i.Day, 00, 00, 00);
                    var iLastTime = new DateTime(i.Year, i.Month, i.Day, 23, 59, 59);
                    var selectedDateTime = item.Where(x => x.TransactionDateTime >= iFirstTime && x.TransactionDateTime <= iLastTime).ToList();

                    if (selectedDateTime.Count > 0)
                    {
                        var firstRecord = selectedDateTime.OrderBy(x => x.TransactionDateTime).FirstOrDefault();
                        var storedFirstRecord = firstRecord;
                        if (firstRecord.EmployeeID != "")
                        {
                            foreach (var rec in selectedDateTime)
                            {
                                firstRecord = storedFirstRecord;
                                var empId = int.TryParse(firstRecord.EmployeeID, out int val) == true ? int.Parse(firstRecord.EmployeeID) : 0;
                                var employeeName = "";
                                var employeeNameAr = "";
                                tbl_Shift shift = null;
                                TimeSpan? shiftStart = null;
                                TimeSpan? shiftEnd = null;
                                var locationOid = 0;
                                var locationNameAr = "";

                                if (_context.tbl_Employees.Any(x => x.EmployeeID == empId))
                                {
                                    var employee = _context.tbl_Employees.FirstOrDefault(x => x.EmployeeID == empId);
                                    var empOid = employee.Oid;
                                    employeeName = employee.NameEn;
                                    employeeNameAr = employee.NameAr;

                                    tbl_EmpLocShiftMap empLocShiftMap = null;
                                    var mapCount = _context.tbl_EmpLocShiftMap.Count(x => x.EmpId == empOid);

                                    if (mapCount == 1)
                                    {

                                        var punchIn = firstRecord.TransactionDateTime;

                                        var currentDay = new DateTime(i.Year, i.Month, i.Day).DayOfWeek;
                                        var sat = false;
                                        var sun = false;
                                        var mon = false;
                                        var tues = false;
                                        var wed = false;
                                        var thur = false;

                                        if (currentDay == DayOfWeek.Saturday)
                                            sat = true;
                                        if (currentDay == DayOfWeek.Sunday)
                                            sun = true;
                                        if (currentDay == DayOfWeek.Monday)
                                            mon = true;
                                        if (currentDay == DayOfWeek.Tuesday)
                                            tues = true;
                                        if (currentDay == DayOfWeek.Wednesday)
                                            wed = true;
                                        if (currentDay == DayOfWeek.Thursday)
                                            thur = true;

                                        // empLocShiftMap = _context.tbl_EmpLocShiftMap.FirstOrDefault(x => x.EmpId == empOid);

                                        empLocShiftMap = sat == true
                                            ? _context.tbl_EmpLocShiftMap.FirstOrDefault(x => x.EmpId == empOid && x.IsSat.Value == true)
                                            : sun == true
                                            ? _context.tbl_EmpLocShiftMap.FirstOrDefault(x => x.EmpId == empOid && x.IsSun.Value == true)
                                            : mon == true
                                            ? _context.tbl_EmpLocShiftMap.FirstOrDefault(x => x.EmpId == empOid && x.IsMon.Value == true)
                                            : tues == true
                                            ? _context.tbl_EmpLocShiftMap.FirstOrDefault(x => x.EmpId == empOid && x.IsTues.Value == true)
                                            : wed == true
                                            ? _context.tbl_EmpLocShiftMap.FirstOrDefault(x => x.EmpId == empOid && x.IsWed.Value == true)
                                            : thur == true
                                            ? _context.tbl_EmpLocShiftMap.FirstOrDefault(x => x.EmpId == empOid && x.IsThur.Value == true)
                                            : _context.tbl_EmpLocShiftMap.FirstOrDefault(x => x.EmpId == empOid && x.IsFri.Value == true);

                                        var skipFlag = false;
                                        if (empLocShiftMap != null)
                                        {
                                            shift = _context.tbl_Shift.FirstOrDefault(x => x.Oid == empLocShiftMap.ShiftOid);
                                            shiftStart = shift.StartTime;
                                            shiftEnd = shift.EndTime;

                                            var nightShiftFlag = false;

                                            if (shiftStart >= new TimeSpan(21, 00, 00) && shiftStart <= new TimeSpan(23, 59, 59))
                                            {
                                                iFirstTime = new DateTime(i.Year, i.Month, i.Day, shiftStart.Value.Hours, shiftStart.Value.Minutes, shiftStart.Value.Seconds).AddHours(-1).AddMinutes(-30);
                                                iLastTime = new DateTime(i.Year, i.Month, i.Day, shiftEnd.Value.Hours, shiftEnd.Value.Minutes, shiftEnd.Value.Seconds).AddDays(1).AddHours(3);

                                                if (iLastTime.Date > toDate.Date)
                                                {
                                                    continue;
                                                }
                                                selectedDateTime = item.Where(x => x.TransactionDateTime >= iFirstTime && x.TransactionDateTime <= iLastTime).ToList();
                                                firstRecord = selectedDateTime.OrderBy(x => x.TransactionDateTime).FirstOrDefault();
                                                if (firstRecord != null)
                                                {
                                                    punchIn = firstRecord.TransactionDateTime;
                                                    skipFlag = true;
                                                    nightShiftFlag = true;
                                                }
                                            }

                                            var location = locations.FirstOrDefault(x => x.Oid == empLocShiftMap.LocationOid);
                                            locationOid = location.Oid;
                                            locationNameAr = location.NameAr;


                                            // var lastRecord = selectedDateTime.OrderByDescending(x => x.TransactionDateTime).FirstOrDefault();
                                            var shiftEndFirstRange = shiftEnd.Value.Subtract(new TimeSpan(1, 0, 0));
                                            var shiftEndSecondRange = shiftEnd.Value.Add(new TimeSpan(1, 0, 0));

                                            if (selectedDateTime.Any())
                                            {
                                                var lastRecord = selectedDateTime.OrderBy(x => x.TransactionDateTime)
                                                    .FirstOrDefault(x => x.TransactionDateTime.Value.TimeOfDay >= shiftEndFirstRange &&
                                                    x.TransactionDateTime.Value.TimeOfDay <= shiftEndSecondRange);
                                                lastRecord = lastRecord ?? selectedDateTime.OrderByDescending(x => x.TransactionDateTime).FirstOrDefault();
                                                var punchOut = firstRecord.TransactionDateTime != lastRecord.TransactionDateTime
                                                                                               ? lastRecord.TransactionDateTime
                                                                                               : firstRecord.TransactionDateTime;

                                                if (nightShiftFlag && punchOut == punchIn) skipFlag = false;

                                                if (empId != 0)
                                                {
                                                    if (!skipFlag)
                                                    {
                                                        var taModel = new TimeAttendanceViewModel()
                                                        {
                                                            TransDate = firstRecord.TransactionDateTime.Value.Date,
                                                            DeviceCode = deviceCodes.FirstOrDefault(x => x.DeviceCode == firstRecord.DeviceCode).NameAr,
                                                            EmployeeId = empId,
                                                            EmployeeName = employeeName,
                                                            EmployeeNameAr = employeeNameAr,
                                                            Location = locationNameAr,
                                                            LocationId = locationOid,
                                                            TemperatureIn = firstRecord.Temperature,
                                                            TemperatureOut = lastRecord.Temperature,
                                                            PunchIn = punchIn,
                                                            PunchOut = punchOut,
                                                            ShiftStart = shiftStart,
                                                            ShiftEnd = shiftEnd,
                                                            LateIn = shiftStart != null && punchIn.Value.TimeOfDay > shiftStart ? (punchIn.Value.TimeOfDay - shiftStart).Value : new TimeSpan(),
                                                            EarlyOut = shiftEnd != null && shiftEnd > punchOut.Value.TimeOfDay ? (shiftEnd - punchOut.Value.TimeOfDay).Value : new TimeSpan(),
                                                            TotalHoursWorked = punchOut.Value - punchIn.Value,
                                                            IsOpened = shift == null
                                                        };

                                                        taModels.Add(taModel);
                                                    }
                                                    else
                                                    {
                                                        if (punchOut != punchIn)
                                                        {
                                                            var taModel = new TimeAttendanceViewModel()
                                                            {
                                                                TransDate = firstRecord.TransactionDateTime.Value.Date,
                                                                DeviceCode = deviceCodes.FirstOrDefault(x => x.DeviceCode == firstRecord.DeviceCode).NameAr,
                                                                EmployeeId = empId,
                                                                EmployeeName = employeeName,
                                                                EmployeeNameAr = employeeNameAr,
                                                                Location = locationNameAr,
                                                                LocationId = locationOid,
                                                                TemperatureIn = firstRecord.Temperature,
                                                                TemperatureOut = lastRecord.Temperature,
                                                                PunchIn = punchIn,
                                                                PunchOut = punchOut,
                                                                ShiftStart = shiftStart,
                                                                ShiftEnd = shiftEnd,
                                                                LateIn = shiftStart != null && punchIn.Value.TimeOfDay > shiftStart ? (punchIn.Value.TimeOfDay - shiftStart).Value : new TimeSpan(),
                                                                EarlyOut = shiftEnd != null && shiftEnd > punchOut.Value.TimeOfDay ? (shiftEnd - punchOut.Value.TimeOfDay).Value : new TimeSpan(),
                                                                TotalHoursWorked = punchOut.Value - punchIn.Value,
                                                                IsOpened = shift == null
                                                            };

                                                            taModels.Add(taModel);
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var currentDay = new DateTime(i.Year, i.Month, i.Day).DayOfWeek;
                                        var sat = false;
                                        var sun = false;
                                        var mon = false;
                                        var tues = false;
                                        var wed = false;
                                        var thur = false;

                                        if (currentDay == DayOfWeek.Saturday)
                                            sat = true;
                                        if (currentDay == DayOfWeek.Sunday)
                                            sun = true;
                                        if (currentDay == DayOfWeek.Monday)
                                            mon = true;
                                        if (currentDay == DayOfWeek.Tuesday)
                                            tues = true;
                                        if (currentDay == DayOfWeek.Wednesday)
                                            wed = true;
                                        if (currentDay == DayOfWeek.Thursday)
                                            thur = true;

                                        var mappingList = sat == true
                                            ? _context.tbl_EmpLocShiftMap.Where(x => x.EmpId == empOid && x.IsSat.Value).OrderByDescending(x => x.fromDate).ToList()
                                            : sun == true
                                            ? _context.tbl_EmpLocShiftMap.Where(x => x.EmpId == empOid && x.IsSun.Value).OrderByDescending(x => x.fromDate).ToList()
                                            : mon == true
                                            ? _context.tbl_EmpLocShiftMap.Where(x => x.EmpId == empOid && x.IsMon.Value).OrderByDescending(x => x.fromDate).ToList()
                                            : tues == true
                                            ? _context.tbl_EmpLocShiftMap.Where(x => x.EmpId == empOid && x.IsTues.Value).OrderByDescending(x => x.fromDate).ToList()
                                            : wed == true
                                            ? _context.tbl_EmpLocShiftMap.Where(x => x.EmpId == empOid && x.IsWed.Value).OrderByDescending(x => x.fromDate).ToList()
                                            : thur == true
                                            ? _context.tbl_EmpLocShiftMap.Where(x => x.EmpId == empOid && x.IsThur.Value).OrderByDescending(x => x.fromDate).ToList()
                                            : _context.tbl_EmpLocShiftMap.Where(x => x.EmpId == empOid && x.IsFri.Value == true).OrderByDescending(x => x.fromDate).ToList();

                                        var dbShifts = _context.tbl_Shift;

                                        foreach (var mapShift in mappingList)
                                        {
                                            var dbShift = dbShifts.FirstOrDefault(x => x.Oid == mapShift.ShiftOid);

                                            iFirstTime = new DateTime(i.Year, i.Month, i.Day, dbShift.StartTime.Value.Hours, dbShift.StartTime.Value.Minutes, dbShift.StartTime.Value.Seconds).AddHours(-2);
                                            iLastTime = new DateTime(i.Year, i.Month, i.Day, dbShift.EndTime.Value.Hours, dbShift.EndTime.Value.Minutes, dbShift.EndTime.Value.Seconds).AddHours(2);
                                            if (dbShift.EndTime.Value > new TimeSpan(22, 00, 00))
                                            {
                                                iLastTime = new DateTime(i.Year, i.Month, i.Day, 1, 0, 0).AddDays(1);
                                            }

                                            var transaction = item.Where(x => x.TransactionDateTime >= iFirstTime && x.TransactionDateTime <= iLastTime).ToList();
                                            if (transaction.Count > 0)
                                            {
                                                var mFirstRecord = transaction.OrderBy(x => x.TransactionDateTime).FirstOrDefault();
                                                var mLastRecord = transaction.Where(a => a.DeviceCode == firstRecord.DeviceCode).OrderByDescending(x => x.TransactionDateTime).FirstOrDefault();
                                                var punchIn = mFirstRecord.TransactionDateTime;
                                                var punchOut = mFirstRecord.TransactionDateTime != mLastRecord.TransactionDateTime
                                                                                                        ? mLastRecord.TransactionDateTime
                                                                                                        : mFirstRecord.TransactionDateTime;

                                                var location = mappingList.FirstOrDefault(x => x.ShiftOid == mapShift.ShiftOid).tbl_Location;

                                                shiftStart = dbShift.StartTime;
                                                shiftEnd = dbShift.EndTime;
                                                locationOid = location.Oid;
                                                locationNameAr = location.NameAr;

                                                var taModel = new TimeAttendanceViewModel()
                                                {
                                                    TransDate = mFirstRecord.TransactionDateTime.Value.Date,
                                                    DeviceCode = deviceCodes.FirstOrDefault(x => x.DeviceCode == firstRecord.DeviceCode).NameAr,
                                                    EmployeeId = empId,
                                                    EmployeeName = employeeName,
                                                    EmployeeNameAr = employeeNameAr,
                                                    Location = locationNameAr,
                                                    LocationId = locationOid,
                                                    TemperatureIn = mFirstRecord.Temperature,
                                                    TemperatureOut = mLastRecord.Temperature,
                                                    PunchIn = punchIn,
                                                    PunchOut = punchOut,
                                                    ShiftStart = shiftStart,
                                                    ShiftEnd = shiftEnd,
                                                    LateIn = shiftStart != null && punchIn.Value.TimeOfDay > shiftStart ? (punchIn.Value.TimeOfDay - shiftStart).Value : new TimeSpan(),
                                                    EarlyOut = shiftEnd != null && shiftEnd > punchOut.Value.TimeOfDay ? (shiftEnd - punchOut.Value.TimeOfDay).Value : new TimeSpan(),
                                                    TotalHoursWorked = punchOut.Value.TimeOfDay - punchIn.Value.TimeOfDay,
                                                    IsOpened = shift == null
                                                };

                                                taModels.Add(taModel);
                                            }
                                        }

                                        //var tbl_Shifts = new List<tbl_Shift>();
                                        //var selectedShift = new tbl_Shift();


                                        //foreach (var mapShift in mappingList) {
                                        //    tbl_Shifts.Add(dbShifts.FirstOrDefault(x => x.Oid == mapShift.ShiftOid));
                                        //}

                                        //var firstRecOfPunch = selectedDateTime.Where(a => a.DeviceCode == firstRecord.DeviceCode).OrderBy(x => x.TransactionDateTime).FirstOrDefault();

                                        //foreach(var tshift in tbl_Shifts)
                                        //{
                                        //    var startTime = tshift.StartTime.Value.Add(-new TimeSpan(1, 0, 0));
                                        //    var endTime = tshift.EndTime.Value.Add(new TimeSpan(1, 0, 0));
                                        //    var recTime = firstRecOfPunch.TransactionDateTime.Value.TimeOfDay;

                                        //    if (startTime <= recTime && endTime >= recTime)
                                        //    {
                                        //        selectedShift = tshift;
                                        //        break;
                                        //    }
                                        //}

                                        //if(selectedShift != null)
                                        //{
                                        //    var location = mappingList.FirstOrDefault(x => x.ShiftOid == selectedShift.Oid).tbl_Location;

                                        //    shiftStart = selectedShift.StartTime;
                                        //    shiftEnd = selectedShift.EndTime;
                                        //    locationOid = location.Oid;
                                        //    locationNameAr = location.NameAr;

                                        //    var punchIn = firstRecord.TransactionDateTime;
                                        //    var punchOut = firstRecord.TransactionDateTime != firstRecOfPunch.TransactionDateTime
                                        //                                                            ? firstRecOfPunch.TransactionDateTime
                                        //                                                            : firstRecord.TransactionDateTime;

                                        //    var taModel = new TimeAttendanceViewModel()
                                        //    {
                                        //        TransDate = firstRecord.TransactionDateTime.Value.Date,
                                        //        DeviceCode = firstRecord.DeviceCode,
                                        //        EmployeeId = empId,
                                        //        EmployeeName = employeeName,
                                        //        Location = locationNameAr,
                                        //        LocationId = locationOid,
                                        //        TemperatureIn = firstRecord.Temperature,
                                        //        TemperatureOut = firstRecOfPunch.Temperature,
                                        //        PunchIn = punchIn,
                                        //        PunchOut = punchOut,
                                        //        ShiftStart = shiftStart,
                                        //        ShiftEnd = shiftEnd,
                                        //        LateIn = shiftStart != null && punchIn.Value.TimeOfDay > shiftStart ? (punchIn.Value.TimeOfDay - shiftStart).Value : new TimeSpan(),
                                        //        EarlyOut = shiftEnd != null && shiftEnd > punchOut.Value.TimeOfDay ? (shiftEnd - punchOut.Value.TimeOfDay).Value : new TimeSpan(),
                                        //        TotalHoursWorked = punchOut.Value.TimeOfDay - punchIn.Value.TimeOfDay,
                                        //        IsOpened = shift == null
                                        //    };

                                        //    taModels.Add(taModel);
                                        //}

                                        //foreach (var mapShift in mappingList)
                                        //{
                                        //    var innerShift = _context.tbl_Shift.FirstOrDefault(x => x.Oid == mapShift.ShiftOid);

                                        //    var lastRecord = selectedDateTime.Where(a => a.DeviceCode == firstRecord.DeviceCode).OrderBy(x => x.TransactionDateTime).FirstOrDefault();

                                        //    var timeOfDay = lastRecord.TransactionDateTime.Value;

                                        //    var startTime = innerShift.StartTime.Value.Add(-new TimeSpan(1, 0, 0));
                                        //    var endTime = innerShift.EndTime.Value.Add(new TimeSpan(1, 0, 0));

                                        //    var startDateTime = new DateTime(timeOfDay.Year, timeOfDay.Month, timeOfDay.Day, startTime.Hours, startTime.Minutes, startTime.Seconds);
                                        //    var endDateTime = new DateTime(timeOfDay.Year, timeOfDay.Month, timeOfDay.Day, 23,59,59).AddHours(2);

                                        //    if (timeOfDay.TimeOfDay >= new TimeSpan(0, 01, 00) && timeOfDay.TimeOfDay <= new TimeSpan(2, 00, 00))
                                        //        startDateTime = startDateTime.AddDays(1);

                                        //    if (timeOfDay >= startDateTime && timeOfDay <= endDateTime)
                                        //    {
                                        //        var punchIn = firstRecord.TransactionDateTime;
                                        //        var punchOut = firstRecord.TransactionDateTime != lastRecord.TransactionDateTime
                                        //                                                                ? lastRecord.TransactionDateTime
                                        //                                                                : firstRecord.TransactionDateTime;

                                        //        if (empLocShiftMap != null)
                                        //        {
                                        //            // shift = _context.tbl_Shift.FirstOrDefault(x => x.Oid == empLocShiftMap.ShiftOid);
                                        //            shiftStart = shift.StartTime;
                                        //            shiftEnd = shift.EndTime;
                                        //            var location = locations.FirstOrDefault(x => x.Oid == empLocShiftMap.LocationOid);
                                        //            locationOid = location.Oid;
                                        //            locationNameAr = location.NameAr;
                                        //        }

                                        //        if (empId != 0)
                                        //        {
                                        //            var taModel = new TimeAttendanceViewModel()
                                        //            {
                                        //                TransDate = firstRecord.TransactionDateTime.Value.Date,
                                        //                DeviceCode = firstRecord.DeviceCode,
                                        //                EmployeeId = empId,
                                        //                EmployeeName = employeeName,
                                        //                Location = locationNameAr,
                                        //                LocationId = locationOid,
                                        //                TemperatureIn = firstRecord.Temperature,
                                        //                TemperatureOut = lastRecord.Temperature,
                                        //                PunchIn = punchIn,
                                        //                PunchOut = punchOut,
                                        //                ShiftStart = shiftStart,
                                        //                ShiftEnd = shiftEnd,
                                        //                LateIn = shiftStart != null && punchIn.Value.TimeOfDay > shiftStart ? (punchIn.Value.TimeOfDay - shiftStart).Value : new TimeSpan(),
                                        //                EarlyOut = shiftEnd != null && shiftEnd > punchOut.Value.TimeOfDay ? (shiftEnd - punchOut.Value.TimeOfDay).Value : new TimeSpan(),
                                        //                TotalHoursWorked = punchOut.Value.TimeOfDay - punchIn.Value.TimeOfDay,
                                        //                IsOpened = shift == null
                                        //            };

                                        //            taModels.Add(taModel);
                                        //        }

                                        //        firstRecord = selectedDateTime.Where(x => x.TransactionDateTime.Value.TimeOfDay >= punchOut.Value.TimeOfDay)
                                        //                                      .OrderBy(x => x.TransactionDateTime).FirstOrDefault();
                                        //    }

                                        //    // a.TransactionDateTime.Value.TimeOfDay >= startTime && a.TransactionDateTime.Value.TimeOfDay <= endTime

                                        //}
                                    }
                                }
                            }
                        }
                    }

                    i = i.AddDays(1);
                }
            }

            var fromDateObj = fromDate.Date;
            var toDateObj = toDate.Date;

            var dbTransactions = new List<tbl_Transactions>();
            var transactions = _context.tbl_Transactions.Where(x => x.TransDate >= fromDateObj && x.TransDate <= toDateObj).ToList();
            foreach (var item in taModels.OrderBy(x => x.PunchIn.Value))
            {
                if (!transactions.Any(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId && x.PunchIn == item.PunchIn && x.PunchOut == item.PunchOut))
                {
                    if (!dbTransactions.Any(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId && x.PunchIn == item.PunchIn && x.PunchOut == item.PunchOut))
                    {
                        dbTransactions.Add(new tbl_Transactions
                        {
                            DeviceCode = item.DeviceCode,
                            EarlyOut = item.EarlyOut,
                            Employee = item.EmployeeName,
                            EmployeeAr = item.EmployeeNameAr,
                            EmployeeId = item.EmployeeId,
                            LateIn = item.LateIn,
                            LocationName = item.Location,
                            LocationOid = item.LocationId,
                            PunchIn = item.PunchIn.Value,
                            PunchOut = item.PunchOut.Value,
                            TemperatureIn = item.TemperatureIn.ToString(),
                            TemperatureOut = item.TemperatureOut.ToString(),
                            TotalHoursWorked = decimal.Parse(item.TotalHoursWorked.TotalHours.ToString()),
                            TransDate = item.TransDate.Value,
                            ShiftStart = item.ShiftStart,
                            ShiftEnd = item.ShiftEnd,
                            IsOpened = item.IsOpened
                        });
                    }
                }
                else if (transactions.Any(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId && x.IsOpened && x.PunchIn == item.PunchIn && x.PunchOut == item.PunchOut))
                {
                    var trans = transactions.FirstOrDefault(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId && x.IsOpened && x.PunchIn == item.PunchIn && x.PunchOut == item.PunchOut);
                    trans.DeviceCode = item.DeviceCode;
                    trans.EarlyOut = item.EarlyOut;
                    trans.Employee = item.EmployeeName;
                    trans.EmployeeAr = item.EmployeeNameAr;
                    trans.EmployeeId = item.EmployeeId;
                    trans.LateIn = item.LateIn;
                    trans.LocationName = item.Location;
                    trans.LocationOid = item.LocationId;
                    trans.PunchIn = item.PunchIn.Value;
                    trans.PunchOut = item.PunchOut.Value;
                    trans.TemperatureIn = item.TemperatureIn.ToString();
                    trans.TemperatureOut = item.TemperatureOut.ToString();
                    trans.TotalHoursWorked = decimal.Parse(item.TotalHoursWorked.TotalHours.ToString());
                    trans.TransDate = item.TransDate.Value;
                    trans.ShiftStart = item.ShiftStart;
                    trans.ShiftEnd = item.ShiftEnd;
                    trans.IsOpened = item.IsOpened;

                    if (dbTransactions.Any(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId && x.IsOpened && x.PunchIn == item.PunchIn && x.PunchOut == item.PunchOut))
                    {
                        dbTransactions.FirstOrDefault(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId && x.IsOpened && x.PunchIn == item.PunchIn && x.PunchOut == item.PunchOut).IsOpened = item.IsOpened;
                    }
                }
            }

            _context.tbl_Transactions.AddRange(dbTransactions);
            _context.SaveChanges();

            return "True";
        }

        public List<TimeAttendanceViewModel> GetTAReport(DateTime? fromDate, DateTime? toDate, int[] employees, string type)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            toDate = toDate.Value.AddHours(12);

            var tAModels = new List<TimeAttendanceViewModel>();
            var transactions = new List<tbl_Transactions>();

            if (employees == null || employees.Length <= 0)
                transactions = _context.tbl_Transactions.Where(x => x.TransDate >= fromDate && x.TransDate <= toDate).ToList();
            else
            {
                transactions = _context.tbl_Transactions.Where(x => x.TransDate >= fromDate && x.TransDate <= toDate && employees.Contains(x.EmployeeId)).ToList();
            }

            foreach (var item in transactions)
            {
                if (!tAModels.Any(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId && x.EmployeeId == item.EmployeeId && x.PunchIn == item.PunchIn && x.PunchOut == item.PunchOut))
                {
                    var onLeave = false;
                    var empLeaves = _context.tbl_EmployeeLeaves.Where(x => x.StartDate <= item.TransDate && x.EndDate >= item.TransDate || x.StartDate <= item.TransDate && x.EndDate >= item.TransDate).ToList();
                    if (empLeaves.Any(x => x.EmployeeId == item.EmployeeId))
                        onLeave = true;

                    if (string.IsNullOrEmpty(type) || type == "default")
                    {
                        if (item.PunchOut > item.PunchIn.AddMinutes(5))
                        {
                            var ttlHrsWorked = double.Parse(item.TotalHoursWorked.ToString());

                            var isLateIn5Min = (item.PunchIn.TimeOfDay - item.ShiftStart >= new TimeSpan(0, 5, 0)) &&
                                           (item.PunchIn.TimeOfDay - item.ShiftStart <= new TimeSpan(0, 15, 0)) &&
                                           item.TotalHoursWorked < 8;

                            var isEarlyOut5Min = (item.ShiftEnd - item.PunchOut.TimeOfDay >= new TimeSpan(0, 5, 0)) &&
                                                 (item.ShiftEnd - item.PunchOut.TimeOfDay <= new TimeSpan(0, 15, 0)) &&
                                                 item.TotalHoursWorked < 8;

                            var isLateIn = item.PunchIn.TimeOfDay - item.ShiftStart >= new TimeSpan(0, 15, 1) && item.TotalHoursWorked < 8;
                            var isEarlyOut = item.ShiftEnd - item.PunchOut.TimeOfDay >= new TimeSpan(0, 15, 1) && item.TotalHoursWorked < 8;
                            if (onLeave)
                            {
                                tAModels.Add(new TimeAttendanceViewModel
                                {
                                    Oid = item.Oid,
                                    DeviceCode = "",
                                    IsOnLeave = "true",
                                    EarlyOut = new TimeSpan(0),
                                    IsEarlyOut = "false",
                                    IsEarlyOut5Min = "false",
                                    EmployeeName = item.Employee,
                                    EmployeeNameAr = item.EmployeeAr,
                                    EmployeeId = item.EmployeeId,
                                    LateIn = new TimeSpan(0),
                                    IsLateIn = "false",
                                    IsLateIn5Min = "false",
                                    Location = "",
                                    LocationId = 0,
                                    PunchIn = new DateTime(0),
                                    PunchOut = new DateTime(0),
                                    TemperatureIn = 0,
                                    TemperatureOut = 0,
                                    TotalWorkingHours = 0,
                                    ConvTotalHoursWorked = "0",
                                    TransDate = item.TransDate,
                                    ShiftStart = new TimeSpan(0),
                                    ShiftEnd = new TimeSpan(0),
                                    IsOpened = false
                                });
                            }
                            else
                            {
                                tAModels.Add(new TimeAttendanceViewModel
                                {
                                    Oid = item.Oid,
                                    DeviceCode = item.DeviceCode,
                                    IsOnLeave = onLeave ? "true" : "false",
                                    EarlyOut = item.EarlyOut,
                                    IsEarlyOut = isEarlyOut ? "true" : "false",
                                    IsEarlyOut5Min = isEarlyOut5Min ? "true" : "false",
                                    EmployeeName = item.Employee,
                                    EmployeeNameAr = item.EmployeeAr,
                                    EmployeeId = item.EmployeeId,
                                    LateIn = item.LateIn,
                                    IsLateIn = isLateIn ? "true" : "false",
                                    IsLateIn5Min = isLateIn5Min ? "true" : "false",
                                    Location = item.LocationName,
                                    LocationId = item.LocationOid,
                                    PunchIn = item.PunchIn,
                                    PunchOut = item.PunchOut,
                                    TemperatureIn = decimal.Parse(item.TemperatureIn),
                                    TemperatureOut = decimal.Parse(item.TemperatureOut),
                                    TotalWorkingHours = item.TotalHoursWorked,
                                    ConvTotalHoursWorked = TimeSpan.FromHours(ttlHrsWorked).ToString("hh\\:mm\\:ss"),
                                    TransDate = item.TransDate,
                                    ShiftStart = item.ShiftStart,
                                    ShiftEnd = item.ShiftEnd,
                                    IsOpened = item.IsOpened
                                });
                            }
                        }
                    }
                    else
                    {
                        var ttlHrsWorked = double.Parse(item.TotalHoursWorked.ToString());

                        var isLateIn5Min = (item.PunchIn.TimeOfDay - item.ShiftStart >= new TimeSpan(0, 5, 0)) &&
                                           (item.PunchIn.TimeOfDay - item.ShiftStart <= new TimeSpan(0, 15, 0)) &&
                                           item.TotalHoursWorked < 8;

                        var isEarlyOut5Min = (item.ShiftEnd - item.PunchOut.TimeOfDay >= new TimeSpan(0, 5, 0)) &&
                                             (item.ShiftEnd - item.PunchOut.TimeOfDay <= new TimeSpan(0, 15, 0)) &&
                                             item.TotalHoursWorked < 8;

                        var isLateIn = item.PunchIn.TimeOfDay - item.ShiftStart >= new TimeSpan(0, 15, 1) && item.TotalHoursWorked < 8;
                        var isEarlyOut = item.ShiftEnd - item.PunchOut.TimeOfDay >= new TimeSpan(0, 15, 1) && item.TotalHoursWorked < 8;


                        if (onLeave)
                        {
                            tAModels.Add(new TimeAttendanceViewModel
                            {
                                Oid = item.Oid,
                                DeviceCode = "",
                                IsOnLeave = "true",
                                EarlyOut = new TimeSpan(0),
                                IsEarlyOut = "false",
                                IsEarlyOut5Min = "false",
                                EmployeeName = item.Employee,
                                EmployeeNameAr = item.EmployeeAr,
                                EmployeeId = item.EmployeeId,
                                LateIn = new TimeSpan(0),
                                IsLateIn = "false",
                                IsLateIn5Min = "false",
                                Location = "",
                                LocationId = 0,
                                PunchIn = new DateTime(0),
                                PunchOut = new DateTime(0),
                                TemperatureIn = 0,
                                TemperatureOut = 0,
                                TotalWorkingHours = 0,
                                ConvTotalHoursWorked = "0",
                                TransDate = item.TransDate,
                                ShiftStart = new TimeSpan(0),
                                ShiftEnd = new TimeSpan(0),
                                IsOpened = false
                            });
                        }
                        else
                        {
                            tAModels.Add(new TimeAttendanceViewModel
                            {
                                Oid = item.Oid,
                                DeviceCode = item.DeviceCode,
                                IsOnLeave = onLeave ? "true" : "false",
                                EarlyOut = item.EarlyOut,
                                IsEarlyOut = isEarlyOut ? "true" : "false",
                                IsEarlyOut5Min = isEarlyOut5Min ? "true" : "false",
                                EmployeeName = item.Employee,
                                EmployeeNameAr = item.EmployeeAr,
                                EmployeeId = item.EmployeeId,
                                LateIn = item.LateIn,
                                IsLateIn = isLateIn ? "true" : "false",
                                IsLateIn5Min = isLateIn5Min ? "true" : "false",
                                Location = item.LocationName,
                                LocationId = item.LocationOid,
                                PunchIn = item.PunchIn,
                                PunchOut = item.PunchOut,
                                TemperatureIn = decimal.Parse(item.TemperatureIn),
                                TemperatureOut = decimal.Parse(item.TemperatureOut),
                                TotalWorkingHours = item.TotalHoursWorked,
                                ConvTotalHoursWorked = TimeSpan.FromHours(ttlHrsWorked).ToString("hh\\:mm\\:ss"),
                                TransDate = item.TransDate,
                                ShiftStart = item.ShiftStart,
                                ShiftEnd = item.ShiftEnd,
                                IsOpened = item.IsOpened
                            });
                        }
                    }
                }
            }

            if (type == "showall")
            {
                var employeeList = employees != null
                    ? _context.tbl_Employees.Where(x => employees.Contains(x.EmployeeID.Value)).ToList()
                    : _context.tbl_Employees.ToList();

                foreach (var employee in employeeList)
                {
                    var empIdsModel = tAModels.Select(a => a.EmployeeId).ToList();
                    var empLeaves = _context.tbl_EmployeeLeaves.Where(x => x.EmployeeId == employee.EmployeeID &&
                                                                           (x.StartDate <= fromDate && x.EndDate >= fromDate ||
                                                                           x.StartDate <= toDate && x.EndDate >= toDate) &&
                                                                           !empIdsModel.Contains(x.EmployeeId)).ToList();

                    if (empLeaves.Any())
                    {
                        tAModels.Add(new TimeAttendanceViewModel
                        {
                            Oid = 0,
                            DeviceCode = "",
                            IsOnLeave = "true",
                            EarlyOut = new TimeSpan(0),
                            IsEarlyOut = "false",
                            IsEarlyOut5Min = "false",
                            EmployeeName = employee.NameEn,
                            EmployeeNameAr = employee.NameAr,
                            EmployeeId = employee.EmployeeID.Value,
                            LateIn = new TimeSpan(0),
                            IsLateIn = "false",
                            IsLateIn5Min = "false",
                            Location = "",
                            LocationId = 0,
                            PunchIn = new DateTime(0),
                            PunchOut = new DateTime(0),
                            TemperatureIn = 0,
                            TemperatureOut = 0,
                            TotalWorkingHours = 0,
                            ConvTotalHoursWorked = "0",
                            TransDate = fromDate,
                            ShiftStart = new TimeSpan(0),
                            ShiftEnd = new TimeSpan(0),
                            IsOpened = false
                        });
                    }

                    //if (!tAModels.Any(x => x.EmployeeId == employee.EmployeeID.Value))
                    //{
                    //    var map = _context.tbl_EmpLocShiftMap.FirstOrDefault(x => x.tbl_Employees.EmployeeID == employee.EmployeeID.Value);
                    //    var shiftStart = new TimeSpan(1, 1, 1);
                    //    var shiftEnd = new TimeSpan(1, 1, 1);

                    //    if (map != null)
                    //    {
                    //        var shift = _context.tbl_Shift.FirstOrDefault(x => x.Oid == map.ShiftOid);
                    //        shiftStart = shift.StartTime.Value;
                    //        shiftEnd = shift.EndTime.Value;
                    //    }

                    //    tAModels.Add(new TimeAttendanceViewModel
                    //    {
                    //        EmployeeName = employee.NameEn,
                    //        EmployeeNameAr = employee.NameAr,
                    //        EmployeeId = employee.EmployeeID.Value,
                    //        Location = map != null ? map.tbl_Location.NameEn : "",
                    //        LocationId = map != null ? map.tbl_Location.Oid : 0,
                    //        ShiftStart = shiftStart,
                    //        ShiftEnd = shiftEnd
                    //    });
                    //}

                }
            }

            if (type == "notScanned")
            {
                toDate = toDate.Value.AddHours(-12);
                var taAbsentModels = new List<TimeAttendanceViewModel>();
                var devices = _context.tbl_Device.Select(x => x.DeviceCode).ToList();
                var locationOids = _context.tbl_Location.Where(x => x.IsActive.Value).Select(a => a.Oid).ToList();
                var employeeList = employees != null
                    ? _context.tbl_Employees.Include(x => x.tbl_EmpLocShiftMap).Where(a => employees.Contains(a.EmployeeID.Value)).ToList()
                    : _context.tbl_Employees.Include(x => x.tbl_EmpLocShiftMap).ToList();
                var empIdsModel = tAModels.Where(x => devices.Contains(x.DeviceCode)).Select(a => a.EmployeeId).ToList();
                var _empLeaves = _context.tbl_EmployeeLeaves.ToList();

                for (var i = fromDate; i <= toDate;)
                {
                    foreach (var employee in employeeList)
                    {
                        if (employee.tbl_EmpLocShiftMap.Any(x => locationOids.Contains(x.LocationOid)))
                        {

                            //var nightShiftFlag = false;

                            //foreach(var idd in employee.tbl_EmpLocShiftMap)
                            //{
                            //    var startTime = idd.tbl_Shift.StartTime;
                            //    var endTime = idd.tbl_Shift.EndTime;

                            //    var iFirstTime = new TimeSpan(20,30,00);
                            //    var iLastTime = new TimeSpan(09, 00, 00);

                            //    if (startTime >= iFirstTime && endTime <= iLastTime) nightShiftFlag = true;
                            //}

                            var empLeaves = _empLeaves.Where(x => x.EmployeeId == employee.EmployeeID &&
                                                                  (x.StartDate <= fromDate && x.EndDate >= fromDate ||
                                                                   x.StartDate <= toDate && x.EndDate >= toDate) &&
                                                                  !empIdsModel.Contains(x.EmployeeId)).ToList();

                            if (!tAModels.Any(x => x.EmployeeId == employee.EmployeeID.Value && x.TransDate == i) &&
                                !empLeaves.Any(x => x.EmployeeId == employee.EmployeeID.Value))
                            {
                                var map = _context.tbl_EmpLocShiftMap.FirstOrDefault(x => x.tbl_Employees.EmployeeID == employee.EmployeeID.Value);

                                var currentDay = new DateTime(i.Value.Year, i.Value.Month, i.Value.Day).DayOfWeek;
                                var sat = false;
                                var sun = false;
                                var mon = false;
                                var tues = false;
                                var wed = false;
                                var thur = false;
                                var fri = false;

                                if (currentDay == DayOfWeek.Saturday)
                                    sat = true;
                                if (currentDay == DayOfWeek.Sunday)
                                    sun = true;
                                if (currentDay == DayOfWeek.Monday)
                                    mon = true;
                                if (currentDay == DayOfWeek.Tuesday)
                                    tues = true;
                                if (currentDay == DayOfWeek.Wednesday)
                                    wed = true;
                                if (currentDay == DayOfWeek.Thursday)
                                    thur = true;
                                if (currentDay == DayOfWeek.Friday)
                                    fri = true;

                                if ((map.IsFri.Value && fri) || (map.IsSat.Value && sat) || (map.IsSun.Value && sun) ||
                                    (map.IsMon.Value && mon) || (map.IsTues.Value && tues) || (map.IsWed.Value && wed) ||
                                    (map.IsThur.Value && thur))
                                {
                                    var shiftStart = new TimeSpan(1, 1, 1);
                                    var shiftEnd = new TimeSpan(1, 1, 1);

                                    if (map != null)
                                    {
                                        var shift = _context.tbl_Shift.FirstOrDefault(x => x.Oid == map.ShiftOid);
                                        shiftStart = shift.StartTime.Value;
                                        shiftEnd = shift.EndTime.Value;
                                    }

                                    taAbsentModels.Add(new TimeAttendanceViewModel
                                    {
                                        TransDate = i.Value,
                                        EmployeeName = employee.NameEn,
                                        EmployeeNameAr = employee.NameAr,
                                        EmployeeId = employee.EmployeeID.Value,
                                        Location = map != null ? map.tbl_Location.NameEn : "",
                                        LocationId = map != null ? map.tbl_Location.Oid : 0,
                                        ShiftStart = shiftStart,
                                        ShiftEnd = shiftEnd
                                    });
                                }
                            }
                        }
                    }

                    i = i.Value.AddDays(1);
                }

                return taAbsentModels;
            }

            return tAModels;
        }

        public string AddAllocation(DateTime fromDate, DateTime? toDate, List<int> employees, string shift, string location,
            bool sun, bool mon, bool tues, bool wed, bool thur, bool fri, bool sat)
        {
            //var flag = false;
            //var flagCount = 0;
            //var nonFlagCount = 0;


            if (employees.Count() > 0)
            {
                var innerShift = _context.tbl_Shift;
                var employeeList = _context.tbl_Employees;
                var empLocShift = _context.tbl_EmpLocShiftMap;

                foreach (var oid in employees)
                {
                    var empObj = employeeList.FirstOrDefault(x => x.Oid == oid);
                    var shiftOid = int.Parse(shift);

                    var recShift = innerShift.FirstOrDefault(x => x.Oid == shiftOid);

                    var locationOid = int.Parse(location);

                    var empLocShiftMap = empLocShift.Where(x => x.EmpId == empObj.Oid);

                    if (empLocShiftMap.Any())
                    {
                        if (empLocShiftMap.Any(x => x.LocationOid == locationOid))
                        {
                            //if (empLocShiftMap.Any(x => x.ShiftOid == shiftOid && x.fromDate <= fromDate && x.toDate >= toDate
                            //&& x.IsSun == sun && x.IsSat == sat && x.IsMon == mon && x.IsTues == tues && x.IsWed == wed
                            //&& x.IsThur == thur && x.IsFri == fri))
                            //{
                            //    return "Employee Record Already Present With Same Location Shift and Dates";
                            //}

                            if (empLocShiftMap.Any(x => x.ShiftOid == shiftOid &&
                           (x.fromDate <= fromDate && x.toDate >= fromDate ||
                           x.fromDate <= toDate && x.toDate >= toDate)))
                            {
                                if (empLocShiftMap.Any(x => x.IsSun == sun || x.IsSat == sat || x.IsMon == mon || x.IsTues == tues || x.IsWed == wed || x.IsThur == thur || x.IsFri == fri))
                                {
                                    return "Employee Record Already Present With Same Location Shift and Dates";
                                }
                            }

                            foreach (var item in empLocShiftMap.Where(x => x.fromDate <= fromDate && x.toDate >= fromDate || x.fromDate <= toDate && x.toDate >= toDate))
                            {
                                if (item.IsFri.Value && fri)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                if (item.IsSat.Value && sat)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                if (item.IsSun.Value && sun)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                if (item.IsMon.Value && mon)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                if (item.IsTues.Value && tues)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                if (item.IsWed.Value && wed)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                if (item.IsThur.Value && thur)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }
                            }

                            //foreach (var item in empLocShiftMap)
                            //{
                            //    if (item.IsFri != fri) break;
                            //    if (item.IsSat != sat) break;
                            //    if (item.IsSun != sun) break;
                            //    if (item.IsMon != mon) break;
                            //    if (item.IsTues != tues) break;
                            //    if (item.IsWed != wed) break;
                            //    if (item.IsThur != thur) break;

                            //    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                            //    var updateShift = innerShift.FirstOrDefault(x => x.Oid == shiftOid);

                            //    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                            //        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                            //    {
                            //        return "Timings Are In Confilict";
                            //    }
                            //}
                        }
                        else
                        {
                            //if (empLocShiftMap.Any(x => x.ShiftOid == shiftOid && x.fromDate <= fromDate && x.toDate >= toDate
                            //&& x.IsSun == sun && x.IsSat == sat && x.IsMon == mon && x.IsTues == tues && x.IsWed == wed
                            //&& x.IsThur == thur && x.IsFri == fri))
                            //{
                            //    return "Please Change the Shift or Days and Try Again!";
                            //}

                            if (empLocShiftMap.Any(x => x.ShiftOid == recShift.Oid &&
                            (x.fromDate <= fromDate && x.toDate >= fromDate ||
                            x.fromDate <= toDate && x.toDate >= toDate)))
                            {
                                // check this
                                if (empLocShiftMap.Any(x => x.IsSun == sun || x.IsSat == sat || x.IsMon == mon || x.IsTues == tues || x.IsWed == wed || x.IsThur == thur || x.IsFri == fri))
                                {
                                    return "Please Change the Shift or Days and Try Again!";
                                }
                            }
                            else
                            {
                                foreach (var item in empLocShiftMap)
                                {
                                    if (locationOid == item.LocationOid)
                                    {
                                        var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                        var updateShift = innerShift.FirstOrDefault(x => x.Oid == shiftOid);

                                        if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                            actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                        {
                                            return "Timings Are In Confilict";
                                        }
                                    }
                                }
                            }
                        }

                        // flag = empLocShiftMap.Any(x => );
                    }


                    //if (!flag)
                    //{
                    var shiftObj = _context.tbl_Shift.FirstOrDefault(x => x.Oid == shiftOid);
                    var locationObj = _context.tbl_Location.FirstOrDefault(x => x.Oid == locationOid);

                    if (empObj != null && shiftObj != null && locationObj != null)
                    {
                        var mapping = new tbl_EmpLocShiftMap
                        {
                            EmpId = empObj.Oid,
                            LocationOid = locationObj.Oid,
                            ShiftOid = shiftObj.Oid,
                            fromDate = fromDate,
                            toDate = toDate,
                            IsFri = fri,
                            IsMon = mon,
                            IsSat = sat,
                            IsSun = sun,
                            IsThur = thur,
                            IsTues = tues,
                            IsWed = wed
                        };

                        _context.tbl_EmpLocShiftMap.Add(mapping);
                        _context.SaveChanges();
                    }

                    // nonFlagCount += 1;
                    //}
                    //else
                    //{
                    //    flagCount += 1;
                    //}
                }
            }
            //if (flagCount <= 0 && nonFlagCount > 0)
            //    return 1;

            //if (flagCount > 0 && nonFlagCount > 0)
            //    return 2;

            return "Record Updated Successfully!";
        }

        public int AddEmployee(int empId, string nameEn, string nameAr)
        {
            if (empId > 0 && !string.IsNullOrEmpty(nameEn) && !string.IsNullOrEmpty(nameAr))
            {
                var flag = _context.tbl_Employees.Any(x => x.EmployeeID == empId);
                if (!flag)
                {
                    var employee = new tbl_Employees
                    {
                        EmployeeID = empId,
                        NameAr = nameAr,
                        NameEn = nameEn
                    };

                    _context.tbl_Employees.Add(employee);
                    _context.SaveChanges();
                    return 1;
                }
            }

            return 0;
        }

        public void AddLocations(string deviceCode, string nameEn, string nameAr)
        {
            if (!string.IsNullOrEmpty(nameEn) && !string.IsNullOrEmpty(nameAr))
            {
                var location = new tbl_Location
                {
                    NameAr = nameAr,
                    NameEn = nameEn,
                    DeviceCode = deviceCode
                };

                _context.tbl_Location.Add(location);
                _context.SaveChanges();
            }
        }

        public void AddShift(string code, TimeSpan startTime, TimeSpan endTime)
        {
            var start = (DateTime.Today + startTime).ToString("hh:mm tt").ToLower();
            var end = (DateTime.Today + endTime).ToString("hh:mm tt").ToLower();

            if (startTime != null || endTime != null)
            {
                var shift = new tbl_Shift
                {
                    Code = code,
                    StartTime = startTime,
                    EndTime = endTime,
                    NameEn = $"{start} : {end}",
                    NameAr = $"{start} : {end}"
                };

                _context.tbl_Shift.Add(shift);
                _context.SaveChanges();
            }
        }

        public void DeleteAllocations(List<int> verifiedIds)
        {
            if (verifiedIds.Count() > 0)
                foreach (var id in verifiedIds)
                {
                    var record = _context.tbl_EmpLocShiftMap.FirstOrDefault(x => x.Oid == id);

                    if (record != null)
                    {
                        _context.tbl_EmpLocShiftMap.Remove(record);
                        _context.SaveChanges();
                    }
                }
        }

        public void DeleteEmployees(List<int> verifiedIds)
        {
            if (verifiedIds.Count() > 0)
                foreach (var id in verifiedIds)
                {
                    var record = _context.tbl_Employees.FirstOrDefault(x => x.Oid == id);

                    if (record != null)
                    {
                        _context.tbl_Employees.Remove(record);
                        _context.SaveChanges();
                    }
                }
        }

        public void DeleteLocations(List<int> verifiedIds)
        {
            if (verifiedIds.Count() > 0)
                foreach (var id in verifiedIds)
                {
                    var record = _context.tbl_Location.FirstOrDefault(x => x.Oid == id);

                    if (record != null)
                    {
                        _context.tbl_Location.Remove(record);
                        _context.SaveChanges();
                    }
                }
        }

        public void DeleteShifts(List<int> verifiedIds)
        {
            if (verifiedIds.Count() > 0)
                foreach (var id in verifiedIds)
                {
                    var record = _context.tbl_Shift.FirstOrDefault(x => x.Oid == id);

                    if (record != null)
                    {
                        _context.tbl_Shift.Remove(record);
                        _context.SaveChanges();
                    }
                }
        }

        public List<AllocationMapViewModel> GetEmpAllocations()
        {
            var allocations = _context.tbl_EmpLocShiftMap.Include("tbl_Shift").Include("tbl_Location").Include("tbl_Employees").ToList();
            var model = new List<AllocationMapViewModel>();

            foreach (var item in allocations)
                model.Add(new AllocationMapViewModel
                {
                    Oid = item.Oid,
                    EmpId = item.tbl_Employees.EmployeeID.Value,
                    EmpName = item.tbl_Employees.NameEn,
                    EmpNameAr = item.tbl_Employees.NameAr,
                    LocationOid = item.tbl_Location.Oid,
                    Location = item.tbl_Location.NameEn,
                    LocationAr = item.tbl_Location.NameAr,
                    ShiftOid = item.tbl_Shift.Oid,
                    ShiftCode = item.tbl_Shift.Code,
                    ShiftName = item.tbl_Shift.NameEn,
                    ShiftNameAr = item.tbl_Shift.NameAr,
                    StartTime = item.tbl_Shift.StartTime.Value,
                    EndTime = item.tbl_Shift.EndTime.Value,
                    FromDate = item.fromDate,
                    ToDate = item.toDate,
                    Sat = item.IsSat,
                    Sun = item.IsSun,
                    Mon = item.IsMon,
                    Tues = item.IsTues,
                    Wed = item.IsWed,
                    Thur = item.IsThur,
                    Fri = item.IsFri
                });

            return model;
        }

        public List<EmployeeViewModel> GetEmployees()
        {
            var employees = _context.tbl_Employees.ToList();
            var model = new List<EmployeeViewModel>();

            foreach (var emp in employees)
                model.Add(new EmployeeViewModel
                {
                    Oid = emp.Oid,
                    NameEn = emp.NameEn,
                    NameAr = emp.NameAr,
                    EmployeeId = emp.EmployeeID.Value,
                    DisplayText = $"{emp.EmployeeID.Value} - {emp.NameAr}"
                });

            return model;
        }

        public List<TaLocation> GetLocations()
        {
            var locations = _context.tbl_Location.ToList();
            var model = new List<TaLocation>();

            foreach (var location in locations)
                model.Add(new TaLocation
                {
                    Oid = location.Oid,
                    NameEn = location.NameEn,
                    NameAr = location.NameAr,
                    DeviceCode = location.DeviceCode
                });

            return model;
        }

        public List<ShiftViewModel> GetShifts()
        {
            var shifts = _context.tbl_Shift.ToList();
            var model = new List<ShiftViewModel>();

            foreach (var shift in shifts)
                model.Add(new ShiftViewModel
                {
                    Oid = shift.Oid,
                    Code = shift.Code,
                    ShiftCode = int.Parse(shift.Code),
                    NameEn = shift.NameEn,
                    NameAr = shift.NameAr,
                    StartTime = shift.StartTime,
                    EndTime = shift.EndTime,
                    DisplayText = $"({shift.Code}) - {shift.NameEn}"
                });

            return model.OrderBy(x => x.ShiftCode).ToList();

        }

        public string UpdateEmpAllocations(string location, string shift, List<int> verifiedIds, DateTime fromDate, DateTime? toDate,
            bool sun, bool mon, bool tues, bool wed, bool thur, bool fri, bool sat)
        {
            var innerShift = _context.tbl_Shift;
            var _shift = int.Parse(shift);
            var recShift = innerShift.FirstOrDefault(x => x.Oid == _shift);

            var innerLocation = _context.tbl_Location;
            var _location = int.Parse(location);
            var recLocation = innerLocation.FirstOrDefault(x => x.Oid == _location);

            if (verifiedIds.Count() > 0)
                foreach (var id in verifiedIds)
                {
                    var record = _context.tbl_EmpLocShiftMap.Include("tbl_Employees").FirstOrDefault(x => x.Oid == id);

                    var empLocShiftMap = _context.tbl_EmpLocShiftMap.Where(x => x.EmpId == record.EmpId && x.Oid != record.Oid).ToList();

                    if (empLocShiftMap.Any())
                    {
                        if (empLocShiftMap.Any(x => x.LocationOid == recLocation.Oid))
                        {
                            if (empLocShiftMap.Any(x => x.ShiftOid == recShift.Oid &&
                            (x.fromDate <= fromDate && x.toDate >= fromDate ||
                            x.fromDate <= toDate && x.toDate >= toDate)))
                            {
                                if (empLocShiftMap.Any(x => x.IsSun == sun || x.IsSat == sat || x.IsMon == mon || x.IsTues == tues || x.IsWed == wed || x.IsThur == thur || x.IsFri == fri))
                                {
                                    return "Employee Record Already Present With Same Location Shift and Dates";
                                }
                            }

                            foreach (var item in empLocShiftMap)
                            {

                                if (item.IsFri.Value && fri)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                if (item.IsSat.Value && sat)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                if (item.IsSun.Value && sun)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                if (item.IsMon.Value && mon)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                if (item.IsTues.Value && tues)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                if (item.IsWed.Value && wed)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                if (item.IsThur.Value && thur)
                                {
                                    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                    var updateShift = recShift;

                                    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                    {
                                        return "Timings Are In Confilict";
                                    }
                                }

                                // if (!flag && !item.IsFri.Value && !fri) { flag = false; } else;
                                // if (!flag && item.IsSat != sat) { flag = true; };
                                // if (!flag && item.IsSun != sun) { flag = true; };
                                // if (!flag && item.IsMon != mon) { flag = true; };
                                // if (!flag && item.IsTues != tues) { flag = true; };
                                // if (!flag && item.IsWed != wed) { flag = true; };
                                // if (!flag && item.IsThur != thur) { flag = true; };

                                //if (flag)
                                //{
                                //    var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                //    var updateShift = recShift;

                                //    if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                //        actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                //    {
                                //        return "Timings Are In Confilict";
                                //    }
                                //}
                            }
                        }
                        else
                        {
                            if (empLocShiftMap.Any(x => x.ShiftOid == recShift.Oid &&
                            (x.fromDate <= fromDate && x.toDate >= fromDate ||
                            x.fromDate <= toDate && x.toDate >= toDate)))
                            {
                                // check this
                                if (empLocShiftMap.Any(x => x.IsSun == sun || x.IsSat == sat || x.IsMon == mon || x.IsTues == tues || x.IsWed == wed || x.IsThur == thur || x.IsFri == fri))
                                {
                                    return "Please Change the Shift or Days and Try Again!";
                                }
                            }

                            //if (empLocShiftMap.Any(x => x.ShiftOid == recShift.Oid && x.fromDate <= fromDate && x.toDate >= toDate
                            //&& x.IsSun == sun && x.IsSat == sat && x.IsMon == mon && x.IsTues == tues && x.IsWed == wed
                            //&& x.IsThur == thur && x.IsFri == fri))
                            //{
                            //    return "Please Change the Shift or Days and Try Again!";
                            //}
                            else
                            {

                                foreach (var item in empLocShiftMap)
                                {
                                    if (recLocation.Oid == item.LocationOid)
                                    {
                                        if (item.IsFri.Value && fri)
                                        {
                                            var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                            var updateShift = recShift;

                                            if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                                actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                            {
                                                return "Timings Are In Confilict";
                                            }
                                        }

                                        if (item.IsSat.Value && sat)
                                        {
                                            var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                            var updateShift = recShift;

                                            if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                                actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                            {
                                                return "Timings Are In Confilict";
                                            }
                                        }

                                        if (item.IsSun.Value && sun)
                                        {
                                            var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                            var updateShift = recShift;

                                            if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                                actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                            {
                                                return "Timings Are In Confilict";
                                            }
                                        }

                                        if (item.IsMon.Value && mon)
                                        {
                                            var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                            var updateShift = recShift;

                                            if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                                actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                            {
                                                return "Timings Are In Confilict";
                                            }
                                        }

                                        if (item.IsTues.Value && tues)
                                        {
                                            var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                            var updateShift = recShift;

                                            if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                                actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                            {
                                                return "Timings Are In Confilict";
                                            }
                                        }

                                        if (item.IsWed.Value && wed)
                                        {
                                            var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                            var updateShift = recShift;

                                            if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                                actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                            {
                                                return "Timings Are In Confilict";
                                            }
                                        }

                                        if (item.IsThur.Value && thur)
                                        {
                                            var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                            var updateShift = recShift;

                                            if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                                actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                            {
                                                return "Timings Are In Confilict";
                                            }
                                        }

                                        //if (item.IsFri != fri) break;
                                        //if (item.IsSat != sat) break;
                                        //if (item.IsSun != sun) break;
                                        //if (item.IsMon != mon) break;
                                        //if (item.IsTues != tues) break;
                                        //if (item.IsWed != wed) break;
                                        //if (item.IsThur != thur) break;

                                        //var actualShift = innerShift.FirstOrDefault(x => x.Oid == item.ShiftOid);
                                        //var updateShift = recShift;

                                        //if (actualShift.StartTime <= updateShift.StartTime && actualShift.EndTime >= updateShift.StartTime ||
                                        //    actualShift.StartTime <= updateShift.EndTime && actualShift.EndTime >= updateShift.EndTime)
                                        //{
                                        //    return "Timings Are In Confilict";
                                        //}
                                    }
                                }
                            }
                        }

                        // flag = empLocShiftMap.Any(x => );
                    }

                    if (record != null)
                    {
                        if (!string.IsNullOrEmpty(location))
                            record.LocationOid = recLocation.Oid;

                        if (!string.IsNullOrEmpty(shift))
                            record.ShiftOid = recShift.Oid;

                        record.fromDate = fromDate;
                        record.toDate = toDate;
                        record.IsFri = fri;
                        record.IsMon = mon;
                        record.IsSat = sat;
                        record.IsSun = sun;
                        record.IsThur = thur;
                        record.IsTues = tues;
                        record.IsWed = wed;

                        _context.SaveChanges();
                    }
                }

            return "true";
        }

        public void DeleteTransactions(List<int> verifiedIds)
        {
            if (verifiedIds.Count() > 0)
                foreach (var id in verifiedIds)
                {
                    var record = _context.tbl_Transactions.FirstOrDefault(x => x.Oid == id);

                    if (record != null)
                    {
                        _context.tbl_Transactions.Remove(record);
                        _context.SaveChanges();
                    }
                }
        }

        public string UpdateEmployee(int oid, int empId, string nameEn, string nameAr)
        {
            if (empId > 0 && !string.IsNullOrEmpty(nameEn) && !string.IsNullOrEmpty(nameAr))
            {
                var employees = _context.tbl_Employees;

                var employee = employees.FirstOrDefault(x => x.Oid == oid);
                if (employee != null)
                {
                    try
                    {
                        if (!employees.Any(x => x.EmployeeID == empId))
                            employee.EmployeeID = empId;

                        employee.NameEn = nameEn;
                        employee.NameAr = nameAr;

                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
            }

            return "true";
        }

        public string UpdateLocation(int oid, string deviceCode, string nameEn, string nameAr)
        {
            if (oid > 0 && !string.IsNullOrEmpty(nameEn) && !string.IsNullOrEmpty(nameAr))
            {
                var locations = _context.tbl_Location;

                var location = locations.FirstOrDefault(x => x.Oid == oid);
                if (location != null)
                {
                    try
                    {
                        if (!locations.Any(x => x.DeviceCode == deviceCode) || string.IsNullOrEmpty(deviceCode))
                            location.DeviceCode = deviceCode;

                        location.NameEn = nameEn;
                        location.NameAr = nameAr;

                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
            }

            return "true";
        }

        public string SendShiftEmail()
        {
            var notPunched = "";
            var showallemp = "";
            var currentTime = DateTime.Now.TimeOfDay;
            TimeSpan shiftTimeFirstPhase = new TimeSpan(0, 0, 0);
            TimeSpan shiftTimeSecondPhase = new TimeSpan(0, 0, 0);
            var shiftnum = HttpContext.Current.Request["ShiftCount"];
            var shiftCount = shiftnum != null ? int.Parse(shiftnum.ToString()) : 0;

            try
            {
                var shifts = _context.tbl_Shift.ToList();

                foreach (var shift in shifts)
                {
                    shiftTimeFirstPhase = shift.StartTime.Value.Add(new TimeSpan(1, 0, 0));
                    shiftTimeSecondPhase = shift.StartTime.Value.Add(new TimeSpan(1, 30, 0));

                    // if (shift.StartTime == new TimeSpan(15,00,00))
                    if (currentTime >= shiftTimeFirstPhase && currentTime <= shiftTimeSecondPhase)
                    {
                        var taAbsentModels = new List<TimeAttendanceViewModel>();
                        var shiftMap = _context.tbl_EmpLocShiftMap
                            .Include(a => a.tbl_Employees)
                            .Include(a => a.tbl_Shift)
                            .Include(a => a.tbl_Location)
                            .Where(x => x.ShiftOid == shift.Oid
                            //&& (x.LocationOid == 84 || x.LocationOid == 67 ||
                            //x.LocationOid == 83 || x.LocationOid == 54)
                            ).ToList();

                        foreach (var item in shiftMap)
                        {
                            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

                            var empLeavesEmpIds = _context.tbl_EmployeeLeaves.Where(x => x.StartDate <= startDate && x.EndDate >= startDate ||
                                                                                   x.StartDate <= endDate && x.EndDate >= endDate)
                                                                       .Select(a => a.EmployeeId).ToList();

                            var taModelsEmpId = GetTAReport(startDate, endDate, null, "showall").Select(x => x.EmployeeId).ToList();
                            showallemp = string.Join(",", taModelsEmpId);

                            taAbsentModels = GetTAReport(startDate, endDate, null, "notScanned").ToList();
                            notPunched = string.Join(",", taAbsentModels.Select(x => x.EmployeeId));

                            taAbsentModels = taAbsentModels.Where(x => x.ShiftStart == item.tbl_Shift.StartTime &&
                                                                       x.ShiftEnd == item.tbl_Shift.EndTime).ToList();

                            foreach (var id in taModelsEmpId)
                            {
                                if (taAbsentModels.Any(x => x.EmployeeId == id))
                                {
                                    var rec = taAbsentModels.FirstOrDefault(x => x.EmployeeId == id);
                                    taAbsentModels.Remove(rec);
                                }
                            }

                            foreach (var id in empLeavesEmpIds)
                            {
                                if (taAbsentModels.Any(x => x.EmployeeId == id))
                                {
                                    var rec = taAbsentModels.FirstOrDefault(x => x.EmployeeId == id);
                                    taAbsentModels.Remove(rec);
                                }
                            }
                        }

                        var pathName = "~/Content/";
                        var path = HttpContext.Current.Server.MapPath(pathName);

                        if (taAbsentModels.Any())
                        {
                            using (var workbook = new XLWorkbook())
                            {
                                var worksheet = workbook.Worksheets.Add("Sheet1");
                                // worksheet.Cell("A1").Value = "Employee Id";
                                worksheet.Cell(1, 1).Value = "Employee Id";
                                worksheet.Cell(1, 2).Value = "Employee Name";
                                worksheet.Cell(1, 3).Value = "Location";
                                worksheet.Cell(1, 4).Value = "Shift Start";
                                worksheet.Cell(1, 5).Value = "Shift End";

                                var count = 2;

                                foreach (var emp in taAbsentModels)
                                {
                                    worksheet.Cell(count, 1).Value = emp.EmployeeId;
                                    worksheet.Cell(count, 2).Value = emp.EmployeeName;
                                    worksheet.Cell(count, 3).Value = emp.Location;
                                    worksheet.Cell(count, 4).Value = emp.ShiftStart.Value.ToString();
                                    worksheet.Cell(count, 5).Value = emp.ShiftEnd.Value.ToString();
                                    count += 1;
                                }

                                workbook.SaveAs(path + "shift.xlsx");
                            }

                            //Application app = new Application();
                            //Workbook wb = app.Workbooks.Add(XlSheetType.xlWorksheet);
                            //Worksheet ws = (Worksheet)app.ActiveSheet;
                            //app.Visible = false;
                            //ws.Cells[1, 1] = "Employee Id";
                            //ws.Cells[1, 2] = "Employee Name";
                            //ws.Cells[1, 3] = "Location";
                            //ws.Cells[1, 4] = "Shift Start";
                            //ws.Cells[1, 5] = "Shift End";

                            //var count = 1;

                            //foreach (var emp in taAbsentModels)
                            //{
                            //    ws.Cells[count, 1] = emp.EmployeeId;
                            //    ws.Cells[count, 2] = emp.EmployeeName;
                            //    ws.Cells[count, 3] = emp.Location;
                            //    ws.Cells[count, 4] = emp.ShiftStart.Value.ToString();
                            //    ws.Cells[count, 5] = emp.ShiftEnd.Value.ToString();
                            //    count += 1;
                            //}

                            //wb.SaveAs(path, XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing, true, false, XlSaveAsAccessMode.xlNoChange,
                            //    XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing);

                            //wb.Close();

                            //app.Quit();

                            var shiftId = HttpContext.Current.Request["ShiftOid"];
                            var shiftOid = shiftId != null ? int.Parse(shiftId.ToString()) : 0;

                            if (shift.Oid != shiftOid)
                            {
                                var shiftCountInfo = new HttpCookie("ShiftCount")
                                {
                                    Value = (shiftCount + 1).ToString()
                                };
                                shiftCountInfo.Expires.Add(new TimeSpan(1, 0, 0));
                                HttpContext.Current.Response.Cookies.Add(shiftCountInfo);
                            }


                            if (shift.Oid != shiftOid && shiftCount >= 20)
                            {
                                //var empLocations = "";
                                //foreach (var loc in taAbsentModels.GroupBy(x => x.LocationId))
                                //    empLocations += loc.FirstOrDefault().Location.ToString();

                                MailMessage mailMessage = new MailMessage
                                {
                                    Subject = $"{shift.NameEn} - Employee List (Not Punched)",
                                    Body = "Employee List",
                                    From = new MailAddress("imillmaterialreq@gmail.com"),
                                };

                                var newAttachment = new Attachment(path + "shift.xlsx");
                                mailMessage.Attachments.Add(newAttachment);
                                mailMessage.To.Add(new MailAddress("shabbir.i@intlmill.com"));
                                // mailMessage.To.Add(new MailAddress("maysara@intlmill.com"));

                                SmtpClient smtp = new SmtpClient
                                {
                                    Host = "smtp.gmail.com",
                                    Port = 587,
                                    EnableSsl = true,
                                    DeliveryMethod = SmtpDeliveryMethod.Network,
                                    UseDefaultCredentials = false,
                                    Credentials = new NetworkCredential("imillmaterialreq@gmail.com", "M@ter!alReq$t")
                                };

                                smtp.Send(mailMessage);

                                newAttachment.Dispose();

                                HttpCookie shiftInfo = new HttpCookie("ShiftOid")
                                {
                                    Value = shift.Oid.ToString()
                                };

                                shiftInfo.Expires.Add(new TimeSpan(1, 0, 0));
                                HttpContext.Current.Response.Cookies.Add(shiftInfo);

                                var shiftCountInfo = new HttpCookie("ShiftCount")
                                {
                                    Value = "0"
                                };

                                shiftCountInfo.Expires.Add(new TimeSpan(1, 0, 0));
                                HttpContext.Current.Response.Cookies.Add(shiftCountInfo);

                                return $"Complete : {currentTime} | {shiftTimeFirstPhase} | {shiftTimeSecondPhase} | ShowAll :{showallemp} | NotPunched : {notPunched}";
                            }

                            return $"Mail Not Sent! : {currentTime} | {shiftTimeFirstPhase} | {shiftTimeSecondPhase} | ShowAll :{showallemp} | NotPunched : {notPunched}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error on {currentTime} = {ex.Message}";
            }

            return $"True : {currentTime} | Shift Count : {shiftCount}";
        }

        public string AddEmployeeLeaves(DateTime from, DateTime to, List<int> employees)
        {
            var empLeaves = _context.tbl_EmployeeLeaves.ToList();
            var empLeavesList = new List<tbl_EmployeeLeaves>();

            if (employees.Count() > 0)
            {
                foreach (var oid in employees)
                {
                    var empObj = _context.tbl_Employees.FirstOrDefault(x => x.Oid == oid);

                    if (empLeaves.Any(x => x.EmployeeId == empObj.EmployeeID && x.StartDate <= from && x.EndDate >= to))
                        return "Record Already Present! Kindly Check The Date/Time";

                    empLeavesList.Add(new tbl_EmployeeLeaves
                    {
                        CreatedOn = DateTime.Now,
                        EmployeeId = empObj.EmployeeID.Value,
                        EndDate = to,
                        StartDate = from
                    });
                }

                _context.tbl_EmployeeLeaves.AddRange(empLeavesList);
                _context.SaveChanges();

                return "Record Added Successfully!";
            }

            return "Record was not added!";

        }

        public List<EmployeeViewModel> GetEmployeeLeaves()
        {
            var empLeaves = _context.tbl_EmployeeLeaves.ToList();
            var employees = _context.tbl_Employees.ToList();

            var employeesViewModel = new List<EmployeeViewModel>();

            foreach (var emp in empLeaves)
            {
                var empObj = employees.FirstOrDefault(x => x.EmployeeID == emp.EmployeeId);
                employeesViewModel.Add(new EmployeeViewModel
                {
                    EmployeeId = emp.EmployeeId,
                    NameEn = empObj.NameEn,
                    NameAr = empObj.NameAr,
                    StartDate = emp.StartDate,
                    EndDate = emp.EndDate,
                    Oid = emp.Oid
                });
            }

            return employeesViewModel;
        }

        public void DeleteEmployeeLeaves(List<int> verifiedIds)
        {
            if (verifiedIds.Count() > 0)
                foreach (var id in verifiedIds)
                {
                    var record = _context.tbl_EmployeeLeaves.FirstOrDefault(x => x.Oid == id);

                    if (record != null)
                    {
                        _context.tbl_EmployeeLeaves.Remove(record);
                        _context.SaveChanges();
                    }
                }
        }

        /// <summary>
        /// Get list of all the employees who has punched in after 15 Min 
        /// or more and email the list in excel send email.
        /// </summary>
        /// 09-Jan-2021 - Shabbir Ismail
        /// In Development
        public string SendShiftStartDetailReport()
        {
            // var today = DateTime.Now;
            var today = new DateTime(2021, 06, 29, 23, 00, 00);

            try
            {
                SyncTAReport(today.Year, today.Month, today.Day, today.Year, today.Month, today.Day);

                var currentTime = today.TimeOfDay;
                var shiftTimeFirstPhase = new TimeSpan(0, 0, 0);
                var shiftTimeSecondPhase = new TimeSpan(0, 0, 0);

                var settings = _context.tbl_ShiftEmailSettings.Where(x => x.IsRegForEmail).ToList();
                var shiftEmailMap = _context.tbl_ShiftEmailMap.Where(x => x.TransDate >= today.Date && x.TransDate <= today.Date).ToList();

                foreach (var setting in settings)
                {
                    var shifts = _context.tbl_Shift.ToList();

                    foreach (var shift in shifts)
                    {
                        if (!shiftEmailMap.Any(x => x.ShiftOid == shift.Oid && x.IsEmailSent && x.IsForStart && x.ShiftEmailSettingOid == setting.Oid))
                        {
                            shiftTimeFirstPhase = shift.StartTime.Value.Add(setting.EmailStartRange);
                            shiftTimeSecondPhase = shift.StartTime.Value.Add(setting.EmailEndRange);

                            if (currentTime >= shiftTimeFirstPhase && currentTime <= shiftTimeSecondPhase)
                            {
                                var taViewModels = new List<TimeAttendanceViewModel>();

                                var shiftMap = _context.tbl_EmpLocShiftMap
                                        .Include(a => a.tbl_Employees)
                                        .Include(a => a.tbl_Shift)
                                        .Include(a => a.tbl_Location)
                                        .Where(x => x.ShiftOid == shift.Oid)
                                        .ToList();

                                if (shiftMap.Any())
                                {
                                    List<TimeAttendanceViewModel> taShowAllEmpRec = null;
                                    foreach (var item in shiftMap)
                                    {

                                        var startDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
                                        var endDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);

                                        var empLeavesEmpIds = _context.tbl_EmployeeLeaves.Where(x => x.StartDate <= startDate &&
                                                                                                     x.EndDate >= startDate ||
                                                                                                     x.StartDate <= endDate &&
                                                                                                     x.EndDate >= endDate)
                                                                                         .Select(a => a.EmployeeId)
                                                                                         .ToList();

                                        taShowAllEmpRec = GetTAReport(startDate, endDate, null, "showall").Where(x => x.ShiftStart == item.tbl_Shift.StartTime &&
                                                                                                                          x.ShiftEnd == item.tbl_Shift.EndTime &&
                                                                                                                          x.LateIn >= setting.LateInRange).ToList();

                                        var taAbsentModels = GetTAReport(startDate, endDate, null, "notScanned").Where(x => x.ShiftStart == item.tbl_Shift.StartTime &&
                                                                                                                            x.ShiftEnd == item.tbl_Shift.EndTime).ToList();

                                        foreach (var id in empLeavesEmpIds)
                                        {
                                            if (taAbsentModels.Any(x => x.EmployeeId == id))
                                            {
                                                var rec = taAbsentModels.FirstOrDefault(x => x.EmployeeId == id);
                                                taAbsentModels.Remove(rec);
                                            }

                                            if (taShowAllEmpRec.Any(x => x.EmployeeId == id))
                                            {
                                                var rec = taShowAllEmpRec.FirstOrDefault(x => x.EmployeeId == id);
                                                taShowAllEmpRec.Remove(rec);
                                            }
                                        }


                                        foreach (var rec in taAbsentModels)
                                            if (!taShowAllEmpRec.Any(x => x.EmployeeId == rec.EmployeeId))
                                                taShowAllEmpRec.Add(rec);
                                    }

                                    var emailFirstPart = "<html lang='en'><head> <meta charset='UTF-8'> <meta name='viewport' content='width=device-width, initial-scale=1.0'> <title>Employee Shift PunchIn/PunchOut Rec</title> <style>table{font-family: arial, sans-serif; border-collapse: collapse; width: 100%; font-size: 12px;}td, th{border: 1px solid #dddddd; text-align: left; padding: 8px;}tr:nth-child(even){background-color: #dddddd;}</style></head><body> <table style='min-width: 820px; width:100%;'> <thead> <tr> <th>Employee Id</th> <th>Employee Name</th> <th>Location</th> <th>Device</th> <th>Shift Start</th> <th>Shift End</th> <th>Punch In</th> <th>Late In</th> </tr></thead> <tbody>";
                                    var emailMidPart = "";
                                    var emailSecondPart = "</tbody> </table></body></html>";


                                    if (taShowAllEmpRec.Any())
                                    {
                                        var pathName = "~/Content/";
                                        var path = HttpContext.Current.Server.MapPath(pathName);
                                        using (var workbook = new XLWorkbook())
                                        {
                                            var worksheet = workbook.Worksheets.Add("Sheet1");
                                            worksheet.Cell(1, 1).Value = "Employee Id";
                                            worksheet.Cell(1, 2).Value = "Employee Name";
                                            worksheet.Cell(1, 3).Value = "Location";
                                            worksheet.Cell(1, 4).Value = "Device";
                                            worksheet.Cell(1, 5).Value = "Shift Start";
                                            worksheet.Cell(1, 6).Value = "Shift End";
                                            worksheet.Cell(1, 7).Value = "Punch In";
                                            worksheet.Cell(1, 8).Value = "Late In";

                                            var count = 2;

                                            foreach (var rec in taShowAllEmpRec)
                                            {
                                                var punchIn = rec.PunchIn.HasValue ? rec.PunchIn.Value.ToString("hh:mm:ss") : "Not Punched";
                                                worksheet.Cell(count, 1).Value = rec.EmployeeId;
                                                worksheet.Cell(count, 2).Value = rec.EmployeeName;
                                                worksheet.Cell(count, 3).Value = rec.Location;
                                                worksheet.Cell(count, 4).Value = rec.DeviceCode;
                                                worksheet.Cell(count, 5).Value = rec.ShiftStart.Value.ToString();
                                                worksheet.Cell(count, 6).Value = rec.ShiftEnd.Value.ToString();
                                                worksheet.Cell(count, 7).Value = punchIn;
                                                worksheet.Cell(count, 8).Value = rec.LateIn.ToString();
                                                count += 1;

                                                emailMidPart +=
                                                    $"<tr><td>{rec.EmployeeId}</td><td>{rec.EmployeeName}</td><td>{rec.Location}</td><td>{rec.DeviceCode}</td>" +
                                                    $"<td>{rec.ShiftStart.Value}</td><td>{rec.ShiftEnd.Value}</td><td>{punchIn}</td>" +
                                                    $"<td>{rec.LateIn}</td></tr>";
                                            }

                                            workbook.SaveAs(path + "shift.xlsx");
                                        }

                                        var empLocations = "";
                                        var empLocCount = 0;
                                        foreach (var loc in taShowAllEmpRec.GroupBy(x => x.LocationId))
                                        {
                                            if (empLocCount == 0)
                                                empLocations += loc.FirstOrDefault().Location.ToString();
                                            else
                                                empLocations += $", {loc.FirstOrDefault().Location}";

                                            empLocCount += 1;
                                        }

                                        var htmlString = emailFirstPart + emailMidPart + emailSecondPart;

                                        MailMessage mailMessage = new MailMessage
                                        {
                                            Subject = $"Shift Start : {shift.NameEn} - ({empLocations})",
                                            Body = htmlString,
                                            IsBodyHtml = true,
                                            From = new MailAddress("imillmaterialreq@gmail.com"),
                                        };

                                        var newAttachment = new Attachment(path + "shift.xlsx");
                                        mailMessage.Attachments.Add(newAttachment);


                                        mailMessage.To.Add(new MailAddress(setting.Email));

                                        SmtpClient smtp = new SmtpClient
                                        {
                                            Host = "smtp.gmail.com",
                                            Port = 587,
                                            EnableSsl = true,
                                            DeliveryMethod = SmtpDeliveryMethod.Network,
                                            UseDefaultCredentials = false,
                                            Credentials = new NetworkCredential("imillmaterialreq@gmail.com", "M@ter!alReq$t")
                                        };

                                        smtp.Send(mailMessage);

                                        newAttachment.Dispose();

                                        var newShiftMap = new tbl_ShiftEmailMap
                                        {
                                            IsEmailSent = true,
                                            IsForStart = true,
                                            ShiftOid = shift.Oid,
                                            TransDate = today.Date,
                                            ShiftEmailSettingOid = setting.Oid
                                        };
                                        _context.tbl_ShiftEmailMap.Add(newShiftMap);
                                        _context.SaveChanges();

                                        return $"Email Sent (Shift Start) {shift.NameEn}";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error Occurred! {ex.InnerException.Message}";
                throw;
            }

            return $"True : {today.Date} {today.TimeOfDay}";
        }

        /// <summary>
        /// Get list of all the employees who is early out 15 Min 
        /// or before & have not completed 8 hours. Create the 
        /// list in excel send email.
        /// </summary>
        /// 10-Jan-2021 - Shabbir Ismail
        /// In Development
        public string SendShiftEndDetailReport()
        {
            var today = DateTime.Now;
            // var today = new DateTime(2021, 02, 09, 16, 08, 00);

            try
            {
                SyncTAReport(today.Year, today.Month, today.Day, today.Year, today.Month, today.Day);

                var currentTime = today.TimeOfDay;
                var shiftTimeFirstPhase = new TimeSpan(0, 0, 0);
                var shiftTimeSecondPhase = new TimeSpan(0, 0, 0);

                var settings = _context.tbl_ShiftEmailSettings.Where(x => x.IsRegForEmail).ToList();
                var shiftEmailMap = _context.tbl_ShiftEmailMap.Where(x => x.TransDate >= today.Date && x.TransDate <= today.Date).ToList();

                foreach (var setting in settings)
                {
                    var shifts = _context.tbl_Shift.ToList();

                    foreach (var shift in shifts)
                    {
                        if (!shiftEmailMap.Any(x => x.ShiftOid == shift.Oid && x.IsEmailSent && !x.IsForStart && x.ShiftEmailSettingOid == setting.Oid))
                        {
                            shiftTimeFirstPhase = shift.EndTime.Value.Add(setting.EmailShiftEndStartRange);
                            shiftTimeSecondPhase = shift.EndTime.Value.Add(setting.EmailShiftEndLastRange);

                            if (currentTime >= shiftTimeFirstPhase && currentTime <= shiftTimeSecondPhase)
                            {
                                var taViewModels = new List<TimeAttendanceViewModel>();

                                var shiftMap = _context.tbl_EmpLocShiftMap
                                        .Include(a => a.tbl_Employees)
                                        .Include(a => a.tbl_Shift)
                                        .Include(a => a.tbl_Location)
                                        .Where(x => x.ShiftOid == shift.Oid)
                                        .ToList();

                                if (shiftMap.Any())
                                {
                                    List<TimeAttendanceViewModel> taShowAllEmpRec = null;
                                    foreach (var item in shiftMap)
                                    {
                                        var startDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
                                        var endDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);

                                        var empLeavesEmpIds = _context.tbl_EmployeeLeaves.Where(x => x.StartDate <= startDate &&
                                                                                                     x.EndDate >= startDate ||
                                                                                                     x.StartDate <= endDate &&
                                                                                                     x.EndDate >= endDate)
                                                                                         .Select(a => a.EmployeeId)
                                                                                         .ToList();

                                        var totalWorkingHour = item.tbl_Shift.StartTime.Value.Subtract(item.tbl_Shift.EndTime.Value).Hours;

                                        taShowAllEmpRec = GetTAReport(startDate, endDate, null, "showall").Where(x => x.ShiftStart == item.tbl_Shift.StartTime &&
                                                                                                                          x.ShiftEnd == item.tbl_Shift.EndTime &&
                                                                                                                          (x.EarlyOut >= setting.EarlyOutRange ||
                                                                                                                          x.TotalWorkingHours < totalWorkingHour)).ToList();

                                        //var taAbsentModels = GetTAReport(startDate, endDate, null, "notScanned").Where(x => x.ShiftStart == item.tbl_Shift.StartTime &&
                                        //                                                                                    x.ShiftEnd == item.tbl_Shift.EndTime).ToList();

                                        foreach (var id in empLeavesEmpIds)
                                        {
                                            //if (taAbsentModels.Any(x => x.EmployeeId == id))
                                            //{
                                            //    var rec = taAbsentModels.FirstOrDefault(x => x.EmployeeId == id);
                                            //    taAbsentModels.Remove(rec);
                                            //}

                                            if (taShowAllEmpRec.Any(x => x.EmployeeId == id))
                                            {
                                                var rec = taShowAllEmpRec.FirstOrDefault(x => x.EmployeeId == id);
                                                taShowAllEmpRec.Remove(rec);
                                            }
                                        }


                                        //foreach (var rec in taAbsentModels)
                                        //    if (!taShowAllEmpRec.Any(x => x.EmployeeId == rec.EmployeeId))
                                        //        taShowAllEmpRec.Add(rec);

                                    }

                                    var emailFirstPart = "<html lang='en'><head> <meta charset='UTF-8'> <meta name='viewport' content='width=device-width, initial-scale=1.0'> <title>Employee Shift PunchIn/PunchOut Rec</title> <style>table{font-family: arial, sans-serif; border-collapse: collapse; width: 100%; font-size: 12px;}td, th{border: 1px solid #dddddd; text-align: left; padding: 8px;}tr:nth-child(even){background-color: #dddddd;}</style></head><body> <table style='min-width: 820px; width:100%;'> <thead> <tr> <th>Employee Id</th> <th>Employee Name</th> <th>Location</th> <th>Device</th> <th>Shift Start</th> <th>Shift End</th> <th>Punch In</th> <th>Punch Out</th> <th>Late In</th> <th>Early Out</th> <th>Total Hour Worked</th> </tr></thead> <tbody>";
                                    var emailMidPart = "";
                                    var emailSecondPart = "</tbody> </table></body></html>";

                                    if (taShowAllEmpRec.Any())
                                    {
                                        var pathName = "~/Content/";
                                        var path = HttpContext.Current.Server.MapPath(pathName);

                                        using (var workbook = new XLWorkbook())
                                        {
                                            var worksheet = workbook.Worksheets.Add("Sheet1");
                                            worksheet.Cell(1, 1).Value = "Employee Id";
                                            worksheet.Cell(1, 2).Value = "Employee Name";
                                            worksheet.Cell(1, 3).Value = "Location";
                                            worksheet.Cell(1, 4).Value = "Device";
                                            worksheet.Cell(1, 5).Value = "Shift Start";
                                            worksheet.Cell(1, 6).Value = "Shift End";
                                            worksheet.Cell(1, 7).Value = "Punch In";
                                            worksheet.Cell(1, 8).Value = "Punch Out";
                                            worksheet.Cell(1, 9).Value = "Late In";
                                            worksheet.Cell(1, 10).Value = "Early Out";
                                            worksheet.Cell(1, 11).Value = "Total Hour Worked";

                                            var count = 2;

                                            foreach (var rec in taShowAllEmpRec)
                                            {
                                                var punchIn = rec.PunchIn.HasValue ? rec.PunchIn.Value.ToString("hh:mm:ss") : "-";
                                                var punchOut = rec.PunchOut.HasValue ? rec.PunchOut.Value.ToString("hh:mm:ss") : "-";

                                                var calcPunchOut = punchIn == punchOut ? "Not Done" : punchOut;
                                                var calcEarlyOut = punchIn == punchOut ? "-" : rec.EarlyOut.ToString();
                                                var calcConvertedHours = punchIn == punchOut ? "00:00:00" : rec.ConvTotalHoursWorked;

                                                if (rec.PunchIn.HasValue && rec.PunchOut.HasValue && punchIn != punchOut)
                                                {
                                                    if (rec.PunchOut.Value.TimeOfDay.Subtract(rec.PunchIn.Value.TimeOfDay).TotalMinutes < 15)
                                                    {
                                                        calcPunchOut = "Not Done";
                                                        calcEarlyOut = "-";
                                                        calcConvertedHours = "00:00:00";
                                                    }
                                                }


                                                worksheet.Cell(count, 1).Value = rec.EmployeeId;
                                                worksheet.Cell(count, 2).Value = rec.EmployeeName;
                                                worksheet.Cell(count, 3).Value = rec.Location;
                                                worksheet.Cell(count, 4).Value = rec.DeviceCode;
                                                worksheet.Cell(count, 5).Value = rec.ShiftStart.Value.ToString();
                                                worksheet.Cell(count, 6).Value = rec.ShiftEnd.Value.ToString();
                                                worksheet.Cell(count, 7).Value = punchIn;
                                                worksheet.Cell(count, 8).Value = calcPunchOut;
                                                worksheet.Cell(count, 9).Value = rec.LateIn.ToString();
                                                worksheet.Cell(count, 10).Value = calcEarlyOut;
                                                worksheet.Cell(count, 11).Value = calcConvertedHours;
                                                count += 1;

                                                emailMidPart +=
                                                    $"<tr><td style='text-align:center'>{rec.EmployeeId}</td><td>{rec.EmployeeName}</td><td style='text-align:center'>{rec.Location}</td><td style='text-align:center'>{rec.DeviceCode}</td>" +
                                                    $"<td style='text-align:center'>{rec.ShiftStart.Value}</td><td style='text-align:center'>{rec.ShiftEnd.Value}</td><td style='text-align:center'>{punchIn}</td>" +
                                                    $"<td style='text-align:center'>{calcPunchOut}</td><td style='text-align:center'>{rec.LateIn}</td><td style='text-align:center'>{calcEarlyOut}</td>" +
                                                    $"<td style='text-align:center'>{calcConvertedHours}</td></tr>";
                                            }

                                            workbook.SaveAs(path + "shiftEnd.xlsx");
                                        }

                                        var empLocations = "";
                                        var empLocCount = 0;
                                        foreach (var loc in taShowAllEmpRec.GroupBy(x => x.LocationId))
                                        {
                                            if (empLocCount == 0)
                                                empLocations += loc.FirstOrDefault().Location.ToString();
                                            else
                                                empLocations += $", {loc.FirstOrDefault().Location}";

                                            empLocCount += 1;
                                        }

                                        var htmlString = emailFirstPart + emailMidPart + emailSecondPart;

                                        MailMessage mailMessage = new MailMessage
                                        {
                                            Subject = $"Shift End : {shift.NameEn} - ({empLocations}) - ({DateTime.Now.ToShortDateString()})",
                                            // Body = "Employee List : Not punched in or early out more than 15 minutes or not completed 8 hours",
                                            Body = htmlString,
                                            IsBodyHtml = true,
                                            From = new MailAddress("imillmaterialreq@gmail.com"),
                                        };

                                        var newAttachment = new Attachment(path + "shiftEnd.xlsx");
                                        mailMessage.Attachments.Add(newAttachment);


                                        mailMessage.To.Add(new MailAddress(setting.Email));

                                        SmtpClient smtp = new SmtpClient
                                        {
                                            Host = "smtp.gmail.com",
                                            Port = 587,
                                            EnableSsl = true,
                                            DeliveryMethod = SmtpDeliveryMethod.Network,
                                            UseDefaultCredentials = false,
                                            Credentials = new NetworkCredential("imillmaterialreq@gmail.com", "M@ter!alReq$t")
                                        };

                                        smtp.Send(mailMessage);

                                        newAttachment.Dispose();

                                        var newShiftMap = new tbl_ShiftEmailMap
                                        {
                                            IsEmailSent = true,
                                            IsForStart = false,
                                            ShiftOid = shift.Oid,
                                            TransDate = today.Date,
                                            ShiftEmailSettingOid = setting.Oid
                                        };
                                        _context.tbl_ShiftEmailMap.Add(newShiftMap);
                                        _context.SaveChanges();

                                        return $"Email Sent (Shift End) {shift.NameEn}";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error Occurred! {ex.InnerException.Message}";
                throw;
            }

            return $"True (Shift End): {today.Date}>{today.TimeOfDay}";
        }

        public string SyncHoDevice(DateTime? fromDate, DateTime? toDate, string ipAddress)
        {
            try
            {
                var today = DateTime.Now;
                if (fromDate == null)
                    fromDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Local);
                if (toDate == null)
                    toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59, DateTimeKind.Local);

                var device = _context.tbl_Device.FirstOrDefault(x => x.DeviceIP == ipAddress);

                var offset = 0;
                var totalRecords = 0;
                var remainingRecords = 0;
                var limitNumber = 20;

                var empTransList = new List<tbl_EmpTransaction>();

                var firstResponse = GetMachineData(fromDate.Value, toDate.Value, ipAddress, offset);
                if (firstResponse != null)
                {
                    if (firstResponse.Response != null)
                    {
                        if (firstResponse.Response.Data != null)
                        {
                            totalRecords = firstResponse.Response.Data.Total;

                            if (firstResponse.Response.Data.ACSPassRecordList.Any())
                            {
                                var passRecordList = firstResponse.Response.Data.ACSPassRecordList.ToList();

                                foreach (var passRecord in passRecordList)
                                {
                                    var personInfo = passRecord.LibMatInfoList.FirstOrDefault().MatchPersonInfo;
                                    var faceInfo = passRecord.FaceInfoList.FirstOrDefault();
                                    var transDateTime = UnixTimeStampToDateTime(faceInfo.Timestamp);

                                    var empTrans = new tbl_EmpTransaction
                                    {
                                        CardID = personInfo.CardID,
                                        DeviceCode = device.DeviceCode,
                                        EmployeeID = personInfo.PersonCode,
                                        EmployeeName = personInfo.PersonName,
                                        IsManual = false,
                                        Maskflag = faceInfo.MaskFlag,
                                        Permission = 0, // Dont have any Information
                                        pushed = false, // Dont have any Information
                                        Temperature = decimal.Parse(faceInfo.Temperature.ToString()),
                                        Timestamp = faceInfo.Timestamp,
                                        TransactionDate = transDateTime.Day,
                                        TransactionDateTime = transDateTime,
                                        TransactionHour = transDateTime.Hour,
                                        TransactionMinute = transDateTime.Minute,
                                        TransactionMonth = transDateTime.Month,
                                        TransactionSecond = transDateTime.Second,
                                        TransactionType = "I",
                                        TransactionYear = transDateTime.Year
                                    };

                                    empTransList.Add(empTrans);
                                }
                            }

                            remainingRecords = totalRecords - limitNumber;
                            offset += limitNumber;
                        }
                    }
                }

                var offsetToggler = true;

                while (remainingRecords > 0)
                {
                    var response = GetMachineData(fromDate.Value, toDate.Value, ipAddress, offset);
                    if (response != null)
                    {
                        if (response.Response != null)
                        {
                            if (response.Response.Data != null)
                            {
                                limitNumber += 20;

                                if (response.Response.Data.ACSPassRecordList.Any())
                                {
                                    var passRecordList = response.Response.Data.ACSPassRecordList.ToList();

                                    foreach (var passRecord in passRecordList)
                                    {
                                        var personInfo = passRecord.LibMatInfoList.FirstOrDefault().MatchPersonInfo;
                                        var faceInfo = passRecord.FaceInfoList.FirstOrDefault();
                                        var transDateTime = UnixTimeStampToDateTime(faceInfo.Timestamp);

                                        var empTrans = new tbl_EmpTransaction
                                        {
                                            CardID = personInfo.CardID,
                                            DeviceCode = device.DeviceCode,
                                            EmployeeID = personInfo.PersonCode,
                                            EmployeeName = personInfo.PersonName,
                                            IsManual = false,
                                            Maskflag = faceInfo.MaskFlag,
                                            Permission = 0, // Dont have any Information
                                            pushed = false, // Dont have any Information
                                            Temperature = decimal.Parse(faceInfo.Temperature.ToString()),
                                            Timestamp = faceInfo.Timestamp,
                                            TransactionDate = transDateTime.Day,
                                            TransactionDateTime = transDateTime,
                                            TransactionHour = transDateTime.Hour,
                                            TransactionMinute = transDateTime.Minute,
                                            TransactionMonth = transDateTime.Month,
                                            TransactionSecond = transDateTime.Second,
                                            TransactionType = "I",
                                            TransactionYear = transDateTime.Year
                                        };

                                        empTransList.Add(empTrans);
                                    }
                                }

                                remainingRecords = totalRecords - limitNumber;
                                if (offsetToggler)
                                {
                                    offset += 19;
                                    offsetToggler = false;
                                }
                                else
                                {
                                    offset += 20;
                                    offsetToggler = true;
                                }
                            }
                        }
                    }
                }

                if (empTransList.Any())
                {
                    var finalTransList = new List<tbl_EmpTransaction>();

                    var dbEmpTrans = _context.tbl_EmpTransaction.Where(x => x.TransactionDateTime >= fromDate.Value && x.TransactionDateTime <= toDate.Value);

                    foreach (var trans in empTransList)
                        if (!dbEmpTrans.Any(x => x.EmployeeID == trans.EmployeeID && x.DeviceCode == trans.DeviceCode &&
                         x.Timestamp == trans.Timestamp && x.TransactionDateTime == trans.TransactionDateTime))
                            finalTransList.Add(trans);


                    if (finalTransList.Any())
                    {
                        _context.tbl_EmpTransaction.AddRange(finalTransList);
                        _context.SaveChanges();
                        return $"{finalTransList.Count()} Transactions Updated | {DateTime.Now}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error : { ex.InnerException.Message }";
            }

            return "No Trasaction Updated!";
        }

        private Root GetMachineData(DateTime fromDate, DateTime toDate, string ipAddress, int offset)
        {
            using (WebClient client = new WebClient())
            {
                var queryInfoListItems = new List<QueryInfoListItem>
                {
                    new QueryInfoListItem
                    {
                        QryType = 4,
                        QryCondition = 3,
                        QryData = ConvertToUnixTimestamp(fromDate).ToString()
                    },

                    new QueryInfoListItem
                    {
                        QryType = 4,
                        QryCondition = 4,
                        QryData = ConvertToUnixTimestamp(toDate).ToString()
                    }
                };

                var queryInfoList = new QueryInfoListModel
                {
                    Num = "2",
                    QueryInfoList = queryInfoListItems,
                    Limit = "20",
                    Offset = offset.ToString()
                };

                var dataString = JsonConvert.SerializeObject(queryInfoList);
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                var response = client.UploadString(new Uri($"http://{ipAddress}/LAPI/V1.0/PACS/Controller/PassRecord"), "POST", dataString);
                var myRoot = JsonConvert.DeserializeObject<Root>(response);

                if (myRoot != null)
                    return myRoot;

                return null;
            }
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            var datetime = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Local);

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixdatetime = (datetime.ToUniversalTime() - epoch).TotalSeconds;
            return unixdatetime;
        }

        public string SyncDevices(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var today = DateTime.Now;
                // var message = "";
                if (fromDate == null)
                    fromDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Local);
                if (toDate == null)
                    toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59, DateTimeKind.Local);

                var devices = _context.tbl_Device.ToList();

                foreach (var device in devices)
                {
                    var ipAddress = device.DeviceIP;
                    var offset = 0;
                    var totalRecords = 0;
                    var remainingRecords = 0;
                    var limitNumber = 20;

                    var empTransList = new List<tbl_EmpTransaction>();

                    Root firstResponse = null;
                    try
                    {
                        firstResponse = GetMachineData(fromDate.Value, toDate.Value, ipAddress, offset);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (firstResponse != null)
                    {
                        if (firstResponse.Response != null)
                        {
                            if (firstResponse.Response.Data != null)
                            {
                                totalRecords = firstResponse.Response.Data.Total;

                                if (firstResponse.Response.Data.ACSPassRecordList.Any())
                                {
                                    var passRecordList = firstResponse.Response.Data.ACSPassRecordList.ToList();

                                    foreach (var passRecord in passRecordList)
                                    {
                                        var personInfo = passRecord.LibMatInfoList.FirstOrDefault().MatchPersonInfo;
                                        var faceInfo = passRecord.FaceInfoList.FirstOrDefault();
                                        var transDateTime = UnixTimeStampToDateTime(faceInfo.Timestamp);

                                        var empTrans = new tbl_EmpTransaction
                                        {
                                            CardID = personInfo.CardID,
                                            DeviceCode = device.DeviceCode,
                                            EmployeeID = personInfo.PersonCode,
                                            EmployeeName = personInfo.PersonName,
                                            IsManual = false,
                                            Maskflag = faceInfo.MaskFlag,
                                            Permission = 0, // Dont have any Information
                                            pushed = false, // Dont have any Information
                                            Temperature = decimal.Parse(faceInfo.Temperature.ToString()),
                                            Timestamp = faceInfo.Timestamp,
                                            TransactionDate = transDateTime.Day,
                                            TransactionDateTime = transDateTime,
                                            TransactionHour = transDateTime.Hour,
                                            TransactionMinute = transDateTime.Minute,
                                            TransactionMonth = transDateTime.Month,
                                            TransactionSecond = transDateTime.Second,
                                            TransactionType = "I",
                                            TransactionYear = transDateTime.Year
                                        };

                                        empTransList.Add(empTrans);
                                    }
                                }

                                remainingRecords = totalRecords - limitNumber;
                                offset += limitNumber;
                            }
                        }
                    }

                    var offsetToggler = true;

                    while (remainingRecords > 0)
                    {
                        var response = GetMachineData(fromDate.Value, toDate.Value, ipAddress, offset);
                        if (response != null)
                        {
                            if (response.Response != null)
                            {
                                if (response.Response.Data != null)
                                {
                                    limitNumber += 20;

                                    if (response.Response.Data.ACSPassRecordList.Any())
                                    {
                                        var passRecordList = response.Response.Data.ACSPassRecordList.ToList();

                                        foreach (var passRecord in passRecordList)
                                        {
                                            var personInfo = passRecord.LibMatInfoList.FirstOrDefault().MatchPersonInfo;
                                            var faceInfo = passRecord.FaceInfoList.FirstOrDefault();
                                            var transDateTime = UnixTimeStampToDateTime(faceInfo.Timestamp);

                                            var empTrans = new tbl_EmpTransaction
                                            {
                                                CardID = personInfo.CardID,
                                                DeviceCode = device.DeviceCode,
                                                EmployeeID = personInfo.PersonCode,
                                                EmployeeName = personInfo.PersonName,
                                                IsManual = false,
                                                Maskflag = faceInfo.MaskFlag,
                                                Permission = 0, // Dont have any Information
                                                pushed = false, // Dont have any Information
                                                Temperature = decimal.Parse(faceInfo.Temperature.ToString()),
                                                Timestamp = faceInfo.Timestamp,
                                                TransactionDate = transDateTime.Day,
                                                TransactionDateTime = transDateTime,
                                                TransactionHour = transDateTime.Hour,
                                                TransactionMinute = transDateTime.Minute,
                                                TransactionMonth = transDateTime.Month,
                                                TransactionSecond = transDateTime.Second,
                                                TransactionType = "I",
                                                TransactionYear = transDateTime.Year
                                            };

                                            empTransList.Add(empTrans);
                                        }
                                    }

                                    remainingRecords = totalRecords - limitNumber;
                                    if (offsetToggler)
                                    {
                                        offset += 19;
                                        offsetToggler = false;
                                    }
                                    else
                                    {
                                        offset += 20;
                                        offsetToggler = true;
                                    }
                                }
                            }
                        }
                    }

                    if (empTransList.Any())
                    {
                        var finalTransList = new List<tbl_EmpTransaction>();

                        var dbEmpTrans = _context.tbl_EmpTransaction.Where(x => x.TransactionDateTime >= fromDate.Value && x.TransactionDateTime <= toDate.Value);

                        foreach (var trans in empTransList)
                            if (!dbEmpTrans.Any(x => x.EmployeeID == trans.EmployeeID && x.DeviceCode == trans.DeviceCode &&
                             x.Timestamp == trans.Timestamp && x.TransactionDateTime == trans.TransactionDateTime))
                                finalTransList.Add(trans);


                        if (finalTransList.Any())
                        {
                            _context.tbl_EmpTransaction.AddRange(finalTransList);
                            _context.SaveChanges();
                            return $"{finalTransList.Count()} Transactions Updated | {DateTime.Now}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error : { ex.InnerException.Message }";
            }

            return "No Trasaction Updated!";
        }
    }
}