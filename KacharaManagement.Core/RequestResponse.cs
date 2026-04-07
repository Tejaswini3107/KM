using System;
using System.Collections.Generic;
using KacharaManagement.Core.Entities;

namespace KacharaManagement.Core
{ 
    public class UpdateRequest
    {
        public string Key { get; set; } = string.Empty;
        public int B1 { get; set; }
        public string S1 { get; set; } = string.Empty;
        public int B2 { get; set; }
        public string S2 { get; set; } = string.Empty;
        public int B3 { get; set; }
        public string S3 { get; set; } = string.Empty;
        public int Al { get; set; }
    }
    public class Login
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class HelloResponse
    {
        public string Status { get; set; } = "OK";
        public string Msg { get; set; } = "Gotham ready.";
    }

    public class UpdateResponse
    {
        public string Status { get; set; } = "OK";
        public bool Truck { get; set; } = false;
        public bool Buzzer { get; set; } = false;
        public string? Msg { get; set; }
    }

    public class TruckMovementRequest
    {
        public string Key { get; set; } = string.Empty;
        public int? HistoryId { get; set; }
        public string State { get; set; } = "Idle";
        public bool Started { get; set; }
        public bool Moving { get; set; }
        public bool Reached { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Location { get; set; }
    }

    public class TruckMovementResponse
    {
        public string Status { get; set; } = "OK";
        public string TruckState { get; set; } = "Idle";
        public bool Started { get; set; }
        public bool Moving { get; set; }
        public bool Reached { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Location { get; set; }
    }

    public class StatusResponse
    {
        public BinData Bin1 { get; set; } = new();
        public BinData Bin2 { get; set; } = new();
        public BinData Bin3 { get; set; } = new();
        public bool Alert { get; set; }
        public bool NeedsTruck { get; set; }
        public string TruckState { get; set; } = "Idle";
        public bool TruckStarted { get; set; }
        public bool TruckMoving { get; set; }
        public bool TruckReached { get; set; }
        public double? TruckLatitude { get; set; }
        public double? TruckLongitude { get; set; }
        public string? TruckLocation { get; set; }
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class BinData
    {
        public int Fill { get; set; }  // Only for Bin1
        public string State { get; set; } = string.Empty;
        public string Led { get; set; } = "green";
        public int Light { get; set; }  // Only for Bin2
        public int Water { get; set; }  // Only for Bin3
    }

    public class HistoryResponse
    {
        public int Count { get; set; }
        public List<HistoryItem> Data { get; set; } = new();
    }

    public class HistoryPageResponse
    {
        public List<HistoryItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class HistoryItem
    {
        public int Id { get; set; }
        public int Bin1Fill { get; set; }
        public string Bin1State { get; set; } = string.Empty;
        public int Bin2Light { get; set; }
        public string Bin2State { get; set; } = string.Empty;
        public int Bin3Water { get; set; }
        public string Bin3State { get; set; } = string.Empty;
        public int Alert { get; set; }
        public bool NeedsTruck { get; set; }
        public string? TruckState { get; set; }
        public bool? TruckStarted { get; set; }
        public bool? TruckMoving { get; set; }
        public bool? TruckReached { get; set; }
        public double? TruckLatitude { get; set; }
        public double? TruckLongitude { get; set; }
        public string? TruckLocation { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }

    public class LogPageResponse
    {
        public List<Entities.LogEntry> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class DashboardOverviewResponse
    {
        public Entities.SensorHistory? LatestHistory { get; set; }
        public Entities.LogEntry? LatestLog { get; set; }
        public int TotalHistoryCount { get; set; }
        public int AlertCount { get; set; }
        public int NeedsTruckCount { get; set; }
        public int LogCount { get; set; }
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class ErrorResponse
    {
        public string Status { get; set; } = "ERROR";
        public string Msg { get; set; } = string.Empty;
    }
}
