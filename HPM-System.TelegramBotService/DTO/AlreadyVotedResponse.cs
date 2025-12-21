namespace HPM_System.TelegramBotService.DTO
{
    public class AlreadyVotedResponse
    {
        public string Message { get; set; } = string.Empty;
        public string PreviousResponse { get; set; } = string.Empty;
        public bool AlreadyVoted { get; set; }
    }
}
