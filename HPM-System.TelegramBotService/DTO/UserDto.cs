namespace HPM_System.TelegramBotService.DTO
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Patronymic { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
