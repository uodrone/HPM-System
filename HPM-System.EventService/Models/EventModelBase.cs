namespace HPM_System.EventService.Models
{
    public abstract class EventModelBase
    {
        public DateTime EventDateTime { get; set; }
        public string? EventDescription { get; set; }
        public long EventId { get; set; }
        public string? EventName { get; set; }
        public long? HouseId { get; set; }
        public string? Place { get; set; }
        public Guid UserId { get; set; }
    }
}