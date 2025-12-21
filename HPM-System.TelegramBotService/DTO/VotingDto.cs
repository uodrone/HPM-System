namespace HPM_System.TelegramBotService.DTO
{
    public class VotingDto
    {
        public Guid Id { get; set; }
        public string QuestionPut { get; set; } = string.Empty;
        public List<string> ResponseOptions { get; set; } = new();
    }
}
