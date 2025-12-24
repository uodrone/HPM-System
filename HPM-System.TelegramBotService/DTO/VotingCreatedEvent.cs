namespace HPM_System.TelegramBotService.DTO
{
    public record VotingCreatedEvent(
        Guid VotingId,
        string QuestionPut,
        List<string> ResponseOptions,
        DateTime EndTime,
        List<VotingParticipant> Participants
    );
}