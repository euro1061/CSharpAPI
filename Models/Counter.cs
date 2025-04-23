namespace QBackend.Models
{
    public partial class Counter
    {
        public int CounterID { get; set; }
        public string CounterName { get; set; } = null!;

        public int IsActive { get; set; }

        public int CurrentQueueID { get; set; }
    }
}