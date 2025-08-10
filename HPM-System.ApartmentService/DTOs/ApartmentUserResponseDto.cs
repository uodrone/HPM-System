// DTOs/ApartmentUserResponseDto.cs
namespace HPM_System.ApartmentService.DTOs
{
    public class ApartmentUserResponseDto
    {
        public Guid UserId { get; set; }
        public decimal Share { get; set; }
        public UserDto? UserDetails { get; set; }
        public List<StatusDto> Statuses { get; set; } = new List<StatusDto>();
    }
}