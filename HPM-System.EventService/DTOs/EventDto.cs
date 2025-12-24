namespace HPM_System.EventService.DTOs
{
    public class EventDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime EventDateTime { get; set; }
        public string? Place { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SubscribedCount { get; set; }
        public bool IsSubscribed { get; set; } // только для GetUserEvents
    }
}
