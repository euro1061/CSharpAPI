namespace QBackend.Dtos
{
    public partial class CreateQueueDto
    {
        public int ServiceID { get; set; }
        public int QueueID { get; set; }
        public string QueueCode { get; set; } = null!;
    }
}