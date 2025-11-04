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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> Get()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<NotificationDto?>> GetByID(Guid id)
        {
            var result = await _service.GetByIDAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetByUserId(Guid userId)
        {
            return Ok(await _service.GetByUserIdAsync(userId));
        }

        [HttpPost]
        public async Task<ActionResult<NotificationDto>> Create([FromBody] CreateNotificationDTO dto)
        {
            var result = await _service.CreateNotificationAsync(dto);
            return CreatedAtAction(nameof(GetByID), new { id = result.Id }, result);
        }
    }
}