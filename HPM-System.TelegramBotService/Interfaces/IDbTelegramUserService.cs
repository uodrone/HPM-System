namespace HPM_System.TelegramBotService.Interfaces
{
    public interface IDbTelegramUserService
    {
        Task<long?> GetTelegramChatIdByUserIdAsync(Guid userId);
    }
}
