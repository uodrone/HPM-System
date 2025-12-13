// HPM_System.EventService.Services/ReminderBackgroundService.cs
using HPM_System.EventService.DTOs;
using HPM_System.EventService.Interfaces;
using HPM_System.EventService.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HPM_System.EventService.Services
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReminderBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

        public ReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReminderBackgroundService запущен.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWorkAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в ReminderBackgroundService");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("ReminderBackgroundService остановлен.");
        }

        private async Task DoWorkAsync(CancellationToken ct)
        {
            // 🔑 Создаём новую область (scope), чтобы получить scoped-сервисы
            using var scope = _serviceProvider.CreateScope();
            var participantRepo = scope.ServiceProvider.GetRequiredService<IEventParticipantRepository>();
            var notificationClient = scope.ServiceProvider.GetRequiredService<INotificationServiceClient>();

            var now = DateTime.UtcNow;

            // 24h напоминание: событие начнётся через 23ч30м – 24ч30м
            await ProcessRemindersAsync(participantRepo, notificationClient,
                now.AddHours(23.5), now.AddHours(24.5), is24h: true, ct);

            // 2h напоминание: событие начнётся через 1ч45м – 2ч15м
            await ProcessRemindersAsync(participantRepo, notificationClient,
                now.AddHours(1.75), now.AddHours(2.25), is24h: false, ct);
        }

        private async Task ProcessRemindersAsync(
            IEventParticipantRepository participantRepo,
            INotificationServiceClient notificationClient,
            DateTime from,
            DateTime to,
            bool is24h,
            CancellationToken ct)
        {
            var participants = await participantRepo.GetParticipantsForReminderAsync(from, to, is24h, ct);
            if (!participants.Any()) return;

            var reminderType = is24h ? "24h" : "2h";
            _logger.LogInformation("Найдено {Count} участников для {ReminderType} напоминаний", participants.Count, reminderType);

            // Группируем по событию
            var grouped = participants.GroupBy(p => p.EventId);

            foreach (var group in grouped)
            {
                var eventEntity = group.First().Event;
                var userIds = group
                    .Where(p => p.IsSubscribed)
                    .Select(p => p.UserId)
                    .ToList();

                if (!userIds.Any()) continue;

                var timeText = is24h ? "через 24 часа" : "через 2 часа";

                var notification = new CreateEventNotificationRequest
                {
                    Title = $"Напоминание: {eventEntity.Title}",
                    Message = $"Событие «{eventEntity.Title}» начнётся {timeText}!\n{eventEntity.Description ?? ""}",
                    ImageUrl = eventEntity.ImageUrl,
                    CreatedBy = group.First().InvitedBy ?? Guid.Empty, // или сохраните CreatedBy в Event
                    IsReadable = true,
                    UserIdList = userIds
                };

                await notificationClient.CreateAsync(notification, ct);

                // Обновляем флаги
                foreach (var p in group)
                {
                    if (is24h)
                    {
                        p.Reminder24hSent = true;
                        p.Reminder24hSentAt = DateTime.UtcNow;
                    }
                    else
                    {
                        p.Reminder2hSent = true;
                        p.Reminder2hSentAt = DateTime.UtcNow;
                    }
                }

                await participantRepo.SaveChangesAsync(ct);
                _logger.LogInformation("Отправлено {ReminderType} напоминание для события {EventId} ({UserCount} получателей)",
                    reminderType, eventEntity.Id, userIds.Count);
            }
        }
    }
}