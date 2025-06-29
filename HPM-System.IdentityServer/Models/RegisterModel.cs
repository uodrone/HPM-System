using System.ComponentModel.DataAnnotations;

namespace HPM_System.IdentityServer.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Email неверного формата")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        public string? LastName { get; set; }

        public string? Patronymic { get; set; }

        [Required(ErrorMessage = "Телефон обязателен")]
        public string? PhoneNumber { get; set; }
    }
}
