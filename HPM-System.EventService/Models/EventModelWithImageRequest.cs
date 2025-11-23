namespace HPM_System.EventService.Models
{
    public class EventModelWithImageRequest : EventModelBase
    {
        public ICollection<ImageRequest> Images { get; set; } = new List<ImageRequest>();

        public EventModelWithImageRequest(EventModel eventModel)
        {
            EventDateTime = eventModel.EventDateTime;
            EventDescription = eventModel.EventDescription;
            EventId = eventModel.EventId;
            EventName = eventModel.EventName;
            HouseId = eventModel.HouseId;
            Place = eventModel.Place;
            UserId = eventModel.UserId;
            Images = new List<ImageRequest>();
        }

        public EventModel ConvertToEventMode()
        {
            return new EventModel()
            {
                EventDateTime = EventDateTime,
                EventDescription = EventDescription,
                EventId = EventId,
                EventName = EventName,
                HouseId = HouseId,
                Place = Place,
                UserId = UserId,
                ImageIds = Images.Select(x => x.ImageId).ToList()
            };
        }
    }
}
