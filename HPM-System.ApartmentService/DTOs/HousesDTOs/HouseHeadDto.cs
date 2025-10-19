namespace HPM_System.ApartmentService.DTOs.HousesDTOs
{
    public class HouseHeadDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? Patronymic { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
