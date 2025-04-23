namespace QBackend.Models
{
    public partial class Service
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = null!;
        public string ServicePrefix { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}