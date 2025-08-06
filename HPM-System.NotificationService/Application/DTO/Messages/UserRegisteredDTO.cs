using System.ComponentModel.DataAnnotations;

namespace HPM_System.NotificationService.Application.DTO.Rabbit
{
    public class UserRegisteredDTO
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MinLength(1)]
        public string Name { get; set; } = string.Empty;
    }
}
