using System;

namespace KacharaManagement.Core.Entities
{
    public class SensorHistory
    {
        public int Id { get; set; }
        public int Bin1Fill { get; set; }
        public string Bin1State { get; set; } = string.Empty;
        public int Bin2Light { get; set; }
        public string Bin2State { get; set; } = string.Empty;
        public int Bin3Water { get; set; }
        public string Bin3State { get; set; } = string.Empty;
        public int Alert { get; set; }
        public bool NeedsTruck { get; set; } = false;
        public string Source { get; set; } = string.Empty; // Arduino, Unity, Admin
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}