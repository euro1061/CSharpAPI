namespace QBackend.Dtos
{
    public partial class ServiceDto
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public char ServicePrefix { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public partial class CreateServiceDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public char ServicePrefix { get; set; }
        public string? Description { get; set; }
    }

    public partial class UpdateServiceDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public char ServicePrefix { get; set; }
        public string? Description { get; set; }
    }
}
