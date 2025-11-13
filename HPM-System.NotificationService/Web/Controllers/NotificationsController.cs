// HPM_System.NotificationService.Web.Controllers/NotificationsController.cs
using HPM_System.NotificationService.Application.DTO;
using HPM_System.NotificationService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HPM_System.NotificationService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationAppService _service;

        public NotificationsController(INotificationAppService service)
        {
            _service = service;
        }

        /// <summary>
        /// Получить все уведомления
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> Get()
        {
            return Ok(await _service.GetAllAsync());
        }

        /// <summary>
        /// Получить уведомление по ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<NotificationDto?>> GetByID(Guid id)
        {
            var result = await _service.GetByIDAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Получить все уведомления для конкретного пользователя
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetByUserId(Guid userId)
        {
            return Ok(await _service.GetByUserIdAsync(userId));
        }

        /// <summary>
        /// Получить только непрочитанные уведомления для пользователя
        /// </summary>
        [HttpGet("user/{userId:guid}/unread")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUnreadByUserId(Guid userId)
        {
            return Ok(await _service.GetUnreadByUserIdAsync(userId));
        }

        /// <summary>
        /// Получить количество непрочитанных уведомлений для пользователя
        /// </summary>
        [HttpGet("user/{userId:guid}/unread/count")]
        public async Task<ActionResult<int>> GetUnreadCount(Guid userId)
        {
            return Ok(await _service.GetUnreadCountAsync(userId));
        }

        /// <summary>
        /// Создать новое уведомление
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<NotificationDto>> Create([FromBody] CreateNotificationDTO dto)
        {
            var result = await _service.CreateNotificationAsync(dto);
            return CreatedAtAction(nameof(GetByID), new { id = result.Id }, result);
        }

        /// <summary>
        /// Отметить уведомление как прочитанное
        /// </summary>
        /// <param name="recipientId">ID записи NotificationUsers (связи пользователя с уведомлением)</param>
        [HttpPatch("recipient/{recipientId:guid}/mark-read")]
        public async Task<ActionResult> MarkAsRead(Guid recipientId)
        {
            var result = await _service.MarkAsRead(recipientId);
            if (!result)
                return NotFound(new { message = "Запись получателя не найдена или уже прочитана" });

            return Ok(new { message = "Уведомление отмечено как прочитанное" });
        }

        /// <summary>
        /// Отметить уведомление как прочитанное для конкретного пользователя
        /// (альтернативный способ, если вы знаете NotificationId и UserId)
        /// </summary>
        [HttpPatch("notification/{notificationId:guid}/user/{userId:guid}/mark-read")]
        public async Task<ActionResult> MarkAsReadByIds(Guid notificationId, Guid userId)
        {
            var result = await _service.MarkAsReadByIdsAsync(notificationId, userId);
            if (!result)
                return NotFound(new { message = "Уведомление или пользователь не найдены" });

            return Ok(new { message = "Уведомление отмечено как прочитанное" });
        }

        /// <summary>
        /// Отметить все уведомления пользователя как прочитанные
        /// </summary>
        [HttpPatch("user/{userId:guid}/mark-all-read")]
        public async Task<ActionResult> MarkAllAsRead(Guid userId)
        {
            var count = await _service.MarkAllAsReadAsync(userId);
            return Ok(new { message = $"Отмечено прочитанными: {count} уведомлений" });
        }
    }
}