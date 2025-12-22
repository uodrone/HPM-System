namespace HPM_System.TelegramBotService.DTO
{
    public class VoteResult
    {
        public bool Success { get; set; }
        public bool AlreadyVoted { get; set; }
        public string? PreviousResponse { get; set; }
        public string? Message { get; set; }
    }
}
