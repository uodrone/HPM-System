namespace HPM_System.TelegramBotService.DTO
{
    public record VotingParticipant(
        Guid UserId,
        long ApartmentId
    );
}
