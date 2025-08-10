using System.ComponentModel.DataAnnotations;

namespace HPM_System.IdentityServer.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email или телефон обязателен")]
        [Display(Name = "Email или телефон")]
        public string EmailOrPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}