namespace VTACheckClock.Models
{
    class Notice
    {
        public int id { get; set; }
        public string? caption { get; set; }
        public string? body { get; set; }
        public string? valid_from { get; set; }
        public string? valid_thru { get; set; }
        public string? image { get; set; }
        public string? active { get; set; }
    }
}
