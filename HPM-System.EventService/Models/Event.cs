namespace HPM_System.EventService.Models;

public class Event
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime EventDateTime { get; set; }
    public string? Place { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual List<EventParticipant> Participants { get; set; } = new();
}