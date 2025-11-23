namespace HPM_System.EventService.Models
{
    public class EventModel : EventModelBase
    {
        public ICollection<int> ImageIds { get; set; } = new List<int>();
    }
}
