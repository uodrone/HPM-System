namespace HPM_System.EventService.Models;
public class EventParticipant
{
    public long EventId { get; set; }
    public Guid UserId { get; set; }
    public bool IsSubscribed { get; set; }

    public DateTime InvitedAt { get; set; }
    public DateTime? SubscribedAt { get; set; }
    public Guid? InvitedBy { get; set; }

    // 🆕 Флаги отправки напоминаний
    public bool Reminder24hSent { get; set; } = false;
    public DateTime? Reminder24hSentAt { get; set; }

    public bool Reminder2hSent { get; set; } = false;
    public DateTime? Reminder2hSentAt { get; set; }

    public virtual Event Event { get; set; } = null!;
}