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

    public class QueryInfoListModel
    {
        public string Num { get; set; }
        public List<QueryInfoListItem> QueryInfoList { get; set; }
        public string Limit { get; set; }
        public string Offset { get; set; }
    }

    public class QueryInfoListItem
    {
        public int QryType { get; set; }
        public int QryCondition { get; set; }
        public string QryData { get; set; }
    }

    public class PanoImage
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public string Data { get; set; }
        public string URL { get; set; }
    }

    public class FaceImage
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public string Data { get; set; }
        public string URL { get; set; }
    }

    public class FaceArea
    {
        public int LeftTopX { get; set; }
        public int LeftTopY { get; set; }
        public int RightBottomX { get; set; }
        public int RightBottomY { get; set; }
    }

    public class FaceInfoList
    {
        public int ID { get; set; }
        public int Timestamp { get; set; }
        public int CapSrc { get; set; }
        public PanoImage PanoImage { get; set; }
        public int MaskFlag { get; set; }
        public double Temperature { get; set; }
        public FaceImage FaceImage { get; set; }
        public FaceArea FaceArea { get; set; }
    }

    public class MatchPersonInfo
    {
        public string PersonCode { get; set; }
        public string PersonName { get; set; }
        public int Gender { get; set; }
        public string CardID { get; set; }
        public string IdentityNo { get; set; }
    }

    public class LibMatInfoList
    {
        public int ID { get; set; }
        public int LibID { get; set; }
        public int LibType { get; set; }
        public int MatchStatus { get; set; }
        public object MatchPersonID { get; set; }
        public object MatchFaceID { get; set; }
        public MatchPersonInfo MatchPersonInfo { get; set; }
    }

    public class ACSPassRecordList
    {
        public int FaceInfoNum { get; set; }
        public List<FaceInfoList> FaceInfoList { get; set; }
        public int CardInfoNum { get; set; }
        public List<object> CardInfoList { get; set; }
        public int GateInfoNum { get; set; }
        public List<object> GateInfoList { get; set; }
        public int LibMatInfoNum { get; set; }
        public List<LibMatInfoList> LibMatInfoList { get; set; }
    }

    public class Data
    {
        public int Total { get; set; }
        public int Offset { get; set; }
        public int Num { get; set; }
        public List<ACSPassRecordList> ACSPassRecordList { get; set; }
    }

    public class Response
    {
        public string ResponseURL { get; set; }
        public int CreatedID { get; set; }
        public int ResponseCode { get; set; }
        public int SubResponseCode { get; set; }
        public string ResponseString { get; set; }
        public int StatusCode { get; set; }
        public string StatusString { get; set; }
        public Data Data { get; set; }
    }

    public class Root
    {
        public Response Response { get; set; }
    }

    public class Data1
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public string Data { get; set; }
        public string URL { get; set; }
    }

    public class Response1
    {
        public string ResponseURL { get; set; }
        public int CreatedID { get; set; }
        public int ResponseCode { get; set; }
        public int SubResponseCode { get; set; }
        public string ResponseString { get; set; }
        public int StatusCode { get; set; }
        public string StatusString { get; set; }
        public Data1 Data { get; set; }
    }

    public class Photo
    {
        public Response1 Response { get; set; }
    }
}