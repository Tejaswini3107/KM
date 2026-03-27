using System;
using System.Collections.Generic;

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

    public class StatusResponse
    {
        public BinData Bin1 { get; set; } = new();
        public BinData Bin2 { get; set; } = new();
        public BinData Bin3 { get; set; } = new();
        public bool Alert { get; set; }
        public bool NeedsTruck { get; set; }
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
        public string Timestamp { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public string Status { get; set; } = "ERROR";
        public string Msg { get; set; } = string.Empty;
    }
}
