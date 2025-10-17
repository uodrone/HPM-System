namespace HPM_System.EventService.DTOs
{
    public class UserDTO
    {
        public long UserId { get; set; }
        public IEnumerable<long> SubscribeEventIds { get; set; } = [];
    }
}
