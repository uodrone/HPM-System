namespace HPM_System.EventService.DTOs
{
    public class CreateEventRequest
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; } // URL уже сгенерирован (фронт загрузил в FileStorageService сам)
        public DateTime EventDateTime { get; set; }
        public string? Place { get; set; }
        public long? HouseId { get; set; } // ← источник аудитории

        // В будущем:
        // public string? CommunityType { get; set; } // "house", "district", "city"
        // public string? CommunityId { get; set; }
    }
}
