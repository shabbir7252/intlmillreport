using System;
using System.Linq;
using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;

namespace ImillReports.Repository
{
    public class TARepository : ITARepository
    {
        private readonly FCCUNVDBEntities _context;

        public TARepository(FCCUNVDBEntities context)
        {
            _context = context;
        }

        public string SyncTAReport(int year, int month, int from, int toYear, int toMonth, int to)
        {
            var fromDate = new DateTime(year, month, from, 00, 00, 00);
            var toDate = new DateTime(toYear, toMonth, to, 23, 59, 59);

            var fetchedTrans = _context.tbl_Transactions.Where(a => a.TransDate >= fromDate && a.TransDate <= toDate);
            _context.tbl_Transactions.RemoveRange(fetchedTrans);
            _context.SaveChanges();

            var empTransactions = _context.tbl_EmpTransaction.Where(x => x.TransactionDateTime >= fromDate &&
                                                                         x.TransactionDateTime <= toDate)
                                                             .GroupBy(a => a.EmployeeID).ToList();

            var tAModels = new List<TimeAttendanceViewModel>();

            var locations = _context.tbl_Location;

            foreach (var item in empTransactions)
            {
                for (var i = fromDate.Date; i <= toDate.Date; i.AddDays(1))
                {
                    var iFirstTime = new DateTime(i.Year, i.Month, i.Day, 00, 00, 00);
                    var iLastTime = new DateTime(i.Year, i.Month, i.Day, 23, 59, 59);
                    var selectedDateTime = item.Where(x => x.TransactionDateTime >= iFirstTime && x.TransactionDateTime <= iLastTime).ToList();

                    if (selectedDateTime.Count > 0)
                    {
                        var firstRecord = selectedDateTime.OrderBy(x => x.TransactionDateTime).FirstOrDefault();
                        if (firstRecord.EmployeeID != "")
                        {
                            var lastRecord = selectedDateTime.Where(a => a.DeviceCode == firstRecord.DeviceCode).OrderByDescending(x => x.TransactionDateTime).FirstOrDefault();
                            var punchIn = firstRecord.TransactionDateTime;
                            var punchOut = firstRecord.TransactionDateTime != lastRecord.TransactionDateTime
                                                                                    ? lastRecord.TransactionDateTime
                                                                                    : firstRecord.TransactionDateTime;
                            // var val = 0;
                            var empId = int.TryParse(firstRecord.EmployeeID, out int val) == true ? int.Parse(firstRecord.EmployeeID) : 0;
                            var employeeName = "";
                            tbl_Shift shift = null;
                            TimeSpan? shiftStart = null;
                            TimeSpan? shiftEnd = null;
                            var locationOid = 0;
                            var locationNameAr = "";

                            if (_context.tbl_Employees.Any(x => x.EmployeeID == empId))
                            {
                                var employee = _context.tbl_Employees.FirstOrDefault(x => x.EmployeeID == empId);
                                var empOid = employee.Oid;
                                employeeName = employee.NameAr;

                                tbl_EmpLocShiftMap empLocShiftMap = null;
                                var mapCount = _context.tbl_EmpLocShiftMap.Count(x => x.EmpId == empOid);
                                if (mapCount == 1)
                                    empLocShiftMap = _context.tbl_EmpLocShiftMap.FirstOrDefault(x => x.EmpId == empOid);
                                else
                                {
                                    var mappingList = _context.tbl_EmpLocShiftMap.Where(x => x.EmpId == empOid).OrderByDescending(x => x.fromDate).ToList();
                                    empLocShiftMap = mappingList.FirstOrDefault(x => x.fromDate >= i && x.fromDate <= i && x.toDate > i);
                                }


                                if (empLocShiftMap != null)
                                {
                                    shift = _context.tbl_Shift.FirstOrDefault(x => x.Oid == empLocShiftMap.ShiftOid);
                                    shiftStart = shift.StartTime;
                                    shiftEnd = shift.EndTime;
                                    var location = locations.FirstOrDefault(x => x.Oid == empLocShiftMap.LocationOid);
                                    locationOid = location.Oid;
                                    locationNameAr = location.NameAr;
                                }
                            }

                            if (empId != 0)
                            {
                                var taModel = new TimeAttendanceViewModel()
                                {
                                    TransDate = firstRecord.TransactionDateTime.Value.Date,
                                    DeviceCode = firstRecord.DeviceCode,
                                    EmployeeId = empId,
                                    EmployeeName = employeeName,
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
                                    TotalHoursWorked = punchOut.Value.TimeOfDay - punchIn.Value.TimeOfDay,
                                    IsOpened = shift == null
                                };

                                tAModels.Add(taModel);
                            }
                        }
                    }

                    i = i.AddDays(1);
                }
            }

            var fromDateObj = fromDate.Date;
            var toDateObj = toDate.Date;

            var dbTransactions = new List<tbl_Transactions>();
            var transactions = _context.tbl_Transactions.Where(x => x.TransDate >= fromDateObj && x.TransDate <= toDateObj);
            foreach (var item in tAModels)
            {
                if (!transactions.Any(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId))
                {
                    if (!dbTransactions.Any(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId))
                    {
                        dbTransactions.Add(new tbl_Transactions
                        {
                            DeviceCode = item.DeviceCode,
                            EarlyOut = item.EarlyOut,
                            Employee = item.EmployeeName,
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
                else if (transactions.Any(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId && x.IsOpened))
                {
                    var trans = transactions.FirstOrDefault(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId && x.IsOpened);
                    trans.DeviceCode = item.DeviceCode;
                    trans.EarlyOut = item.EarlyOut;
                    trans.Employee = item.EmployeeName;
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

                    if (dbTransactions.Any(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId && x.IsOpened))
                    {
                        dbTransactions.FirstOrDefault(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId && x.IsOpened).IsOpened = item.IsOpened;
                    }
                }
            }

            _context.tbl_Transactions.AddRange(dbTransactions);
            _context.SaveChanges();

            return "True";
        }

        public List<TimeAttendanceViewModel> GetTAReport(DateTime? fromDate, DateTime? toDate, int[] employees)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            var tAModels = new List<TimeAttendanceViewModel>();
            IQueryable<tbl_Transactions> transactions = null;

            if (employees == null || employees.Length <= 0)
                transactions = _context.tbl_Transactions.Where(x => x.TransDate >= fromDate && x.TransDate <= toDate);
            else
                transactions = _context.tbl_Transactions.Where(x => x.TransDate >= fromDate && x.TransDate <= toDate && employees.Contains(x.EmployeeId));

            foreach (var item in transactions)
            {
                if (!tAModels.Any(x => x.TransDate == item.TransDate && x.EmployeeId == item.EmployeeId))
                {
                    if (item.PunchOut > item.PunchIn.AddMinutes(5))
                    {
                        tAModels.Add(new TimeAttendanceViewModel
                        {
                            Oid = item.Oid,
                            DeviceCode = item.DeviceCode,
                            EarlyOut = item.EarlyOut,
                            EmployeeName = item.Employee,
                            EmployeeId = item.EmployeeId,
                            LateIn = item.LateIn,
                            Location = item.LocationName,
                            LocationId = item.LocationOid,
                            PunchIn = item.PunchIn,
                            PunchOut = item.PunchOut,
                            TemperatureIn = decimal.Parse(item.TemperatureIn),
                            TemperatureOut = decimal.Parse(item.TemperatureOut),
                            TotalWorkingHours = item.TotalHoursWorked,
                            TransDate = item.TransDate,
                            ShiftStart = item.ShiftStart,
                            ShiftEnd = item.ShiftEnd,
                            IsOpened = item.IsOpened
                        });
                    }
                }
            }

            return tAModels;
        }

        public int AddAllocation(DateTime fromDate, DateTime? toDate, List<int> employees, string shift, string location)
        {
            var flag = false;
            var flagCount = 0;
            var nonFlagCount = 0;

            if (employees.Count() > 0)
                foreach (var oid in employees)
                {
                    var empObj = _context.tbl_Employees.FirstOrDefault(x => x.Oid == oid);
                    var shiftOid = int.Parse(shift);
                    var locationOid = int.Parse(location);

                    flag = _context.tbl_EmpLocShiftMap.Any(x => x.EmpId == empObj.Oid &&
                                                                x.ShiftOid == shiftOid &&
                                                                x.LocationOid == locationOid &&
                                                                x.fromDate >= fromDate &&
                                                                x.toDate <= toDate);

                    if (!flag)
                    {
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
                                toDate = toDate
                            };

                            _context.tbl_EmpLocShiftMap.Add(mapping);
                            _context.SaveChanges();
                        }

                        nonFlagCount += 1;
                    }
                    else
                    {
                        flagCount += 1;
                    }
                }

            if (flagCount <= 0 && nonFlagCount > 0)
                return 1;

            if (flagCount > 0 && nonFlagCount > 0)
                return 2;

            return 3;
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
                    ToDate = item.toDate
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
                    NameEn = shift.NameEn,
                    NameAr = shift.NameAr,
                    StartTime = shift.StartTime,
                    EndTime = shift.EndTime,
                    DisplayText = $"({shift.Code}) - {shift.NameEn}"
                });

            return model;

        }

        public void UpdateEmpAllocations(string location, string shift, List<int> verifiedIds)
        {
            if (verifiedIds.Count() > 0)
                foreach (var id in verifiedIds)
                {
                    var record = _context.tbl_EmpLocShiftMap.Include("tbl_Employees").FirstOrDefault(x => x.Oid == id);

                    if (record != null)
                    {
                        if (!string.IsNullOrEmpty(location))
                            record.LocationOid = int.Parse(location);

                        if (!string.IsNullOrEmpty(shift))
                            record.ShiftOid = int.Parse(shift);

                        _context.SaveChanges();
                    }
                }
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
    }
}