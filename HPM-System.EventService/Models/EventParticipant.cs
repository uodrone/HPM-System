using Microsoft.Extensions.Logging;

namespace HPM_System.EventService.Models
{
    public class EventParticipant
    {
        public long EventId { get; set; }
        public Guid UserId { get; set; }
        public bool IsSubscribed { get; set; }

        // Когда событие было "предложено" пользаку (при создании рассылки)
        public DateTime InvitedAt { get; set; }

        // Когда пользак подписался (может быть null)
        public DateTime? SubscribedAt { get; set; }

        // Кто инициировал рассылку (например, инициатор события), а может и автоматическое событие быть, поэтому может быть null
        public Guid? InvitedBy { get; set; }
    }
}
