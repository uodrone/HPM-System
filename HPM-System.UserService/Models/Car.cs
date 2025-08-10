using System.ComponentModel.DataAnnotations;

namespace HPM_System.UserService.Models
{
    public class Car
    {
        public long Id { get; set; }

        [StringLength(100, ErrorMessage = "Марка не может быть длиннее 50 символов")]
        public string? Mark { get; set; }

        [StringLength(100, ErrorMessage = "Модель не может быть длиннее 50 символов")]
        public string? Model { get; set; }

        [StringLength(50, ErrorMessage = "Цвет не может быть длиннее 30 символов")]
        public string? Color { get; set; }

        [Required(ErrorMessage = "Номер автомобиля обязателен")]
        [StringLength(15, ErrorMessage = "Номер не может быть длиннее 15 символов")]
        public string Number { get; set; }

        // Владелец автомобиля
        [Required(ErrorMessage = "ID пользователя обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "ID пользователя должен быть больше 0")]
        public Guid UserId { get; set; }
    }
}