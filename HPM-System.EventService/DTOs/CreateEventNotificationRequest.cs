namespace HPM_System.EventService.DTOs
{
    public class CreateEventNotificationRequest
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public Guid CreatedBy { get; set; }
        public bool IsReadable { get; set; } = true;
        public List<Guid> UserIdList { get; set; } = new();
    }
}
