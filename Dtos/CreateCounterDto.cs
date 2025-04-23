namespace QBackend.Dtos
{
    public partial class CreateCounterDto
    {
        public string CounterName { get; set; } = null!;
        
        public bool IsActive { get; set; }
    }
}