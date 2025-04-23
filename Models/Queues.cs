namespace QBackend.Models
{
    public partial class Queue
    {
        public int QueueID { get; set; }
        public int ServiceID { get; set; }
        public string ServicePrefix { get; set; } = null!;
        public int QueueNumber { get; set; }

        public string QueueCode { get; set; } = null!;

        public string Status { get; set; } = null!;

        public DateTime CalledAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public partial class QueuePrintInfo
    {
        public string QueueCode { get; set; } = null!;

        public string ServiceName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
    public partial class GetQueues
    {
        public int QueueID { get; set; }
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = null!;
        public string ServicePrefix { get; set; } = null!;
        public int QueueNumber { get; set; }
        public string QueueCode { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CalledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LastAction { get; set; } = null!;
        public DateTime LastActionTime { get; set; }
        public int RowNum { get; set; }
    }
}