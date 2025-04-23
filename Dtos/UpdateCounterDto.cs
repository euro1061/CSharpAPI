namespace QBackend.Dtos
{
    public partial class UpdateCounterDto
    {
        public int CounterID { get; set; }
        public string CounterName { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}