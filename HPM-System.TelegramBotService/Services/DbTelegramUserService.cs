using HPM_System.TelegramBotService.Interfaces;
using HPM_System.TelegramBotService.Data;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.TelegramBotService.Services
{
    public class DbTelegramUserService : IDbTelegramUserService
    {
        private readonly AppDbContext _context;

        public DbTelegramUserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<long?> GetTelegramChatIdByUserIdAsync(Guid userId)
        {
            var user = await _context.TelegramUsers
                .FirstOrDefaultAsync(u => u.UserId == userId);
            return user?.TelegramChatId;
        }
    }
}
