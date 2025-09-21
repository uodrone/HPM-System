namespace HPM_System.ApartmentService.DTOs.HousesDTOs
{
    public class HouseHeadDto
    {
        public Guid? Id { get; set; } // id пользователя
        public string? Address { get; set; }
        public long HouseNumber { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
