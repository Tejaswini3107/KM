using System;

namespace KacharaManagement.Core.Entities
{
    public class LogEntry
    {
        public int Id { get; set; }
        public string Level { get; set; } = "Info"; // Info, Error, Exception, Request, Response
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string Source { get; set; } = string.Empty; // Arduino, Unity, Admin, System
        public string RequestPath { get; set; } = string.Empty;
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}