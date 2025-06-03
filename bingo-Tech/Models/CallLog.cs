namespace bingo_Tech.Models
{
    public class CallLog
    {
        public int Id { get; set; }
        public string CallerName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Duration { get; set; }
    }
}
