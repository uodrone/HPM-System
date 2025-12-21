namespace DTO
{
    public class TelegramVoteRequestDto
    {
        public Guid UserId { get; set; }
        public string Response { get; set; } = string.Empty;
    }
}
