namespace HPM_System.EventService.DTOs
{
    public class CreateEventRequest
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; } // URL уже сгенерирован (фронт загрузил в FileStorageService сам)
        public DateTime EventDateTime { get; set; }
        public string? Place { get; set; }
        // Универсальный идентификатор сообщества
        public long? CommunityId { get; set; }

        // Тип сообщества: "house", "district", "city" и т.д., по умолчанию - дом
        public CommunityType CommunityType { get; set; } = CommunityType.House;
    }
}
