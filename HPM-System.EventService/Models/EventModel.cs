namespace HPM_System.EventService.Models
{
    public class EventModel
    {
        public long EventId { get; set; }
        public long? HouseId { get; set; }
        public long UserId { get; set; }
        public DateTime EventDateTime { get; set; }
        public string? Place { get; set; }
        public string? EventName { get; set; }
        public string? EventDescription { get; set; }
        public ICollection<long>? ImageIds { get; set; } = null;
    }
}
