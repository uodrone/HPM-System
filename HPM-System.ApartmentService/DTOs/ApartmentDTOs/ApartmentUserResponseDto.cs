// DTOs/ApartmentUserResponseDto.cs
using DTOs.StatusDTOs;
using DTOs.UserDTOs;

namespace DTOs.ApartmentDTOs
{
    public class ApartmentUserResponseDto
    {
        public Guid UserId { get; set; }
        public decimal Share { get; set; }
        public UserDto? UserDetails { get; set; }
        public List<StatusDto> Statuses { get; set; } = new List<StatusDto>();
    }
}