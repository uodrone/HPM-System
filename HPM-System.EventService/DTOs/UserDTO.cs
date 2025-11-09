namespace HPM_System.EventService.DTOs
{
    public class UserDTO
    {
        public Guid UserId { get; set; }
        public IEnumerable<long> SubscribeEventIds { get; set; } = [];
    }
}
