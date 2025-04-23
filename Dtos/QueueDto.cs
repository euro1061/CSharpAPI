namespace QBackend.Dtos
{
    public partial class CallQueueDto
    {
        public int QueueID { get; set; }
        public int CounterID { get; set; }
    }

    public partial class SkipQueueDto
    {
        public int QueueID { get; set; }
        public int CounterID { get; set; }
    }

    public partial class CompleteQueueDto
    {
        public int QueueID { get; set; }
        public int CounterID { get; set; }
    }

    public partial class InProgressQueueDto
    {
        public int QueueID { get; set; }
        public int CounterID { get; set; }
    }

    public partial class GetQueueParams
    {
        public int? ServiceID { get; set; }
        public string? Status { get; set; }
        public string? QueueCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}