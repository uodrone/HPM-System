using HPM_System.NotificationService.Application.DTO;
using HPM_System.NotificationService.Application.Interfaces;
using HPM_System.NotificationService.Application.Services;
using HPM_System.NotificationService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HPM_System.NotificationService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationAppService _notificationService;

        public NotificationsController(INotificationAppService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IEnumerable<Notification>> Get()
        {
            return await _notificationService.GetAllAsync();
        }

        [HttpGet("{id:guid}")]
        public async Task<Notification?> GetByID(Guid id)
        {
            return await _notificationService.GetByIDAsync(id);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationDTO dto)
        {            
            return Ok(await _notificationService.CreateNotificationAsync(dto));
        }
    }
}
