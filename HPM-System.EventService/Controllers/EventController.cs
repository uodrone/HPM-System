using HPM_System.EventService.Models;
using HPM_System.EventService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace HPM_System.EventService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase, IEventController
    {
        
        private readonly ILogger<EventController> _logger;
        private readonly IEventService _eventService;
        private readonly IFileStorageClient _fileStorageClient;
        private readonly IApartmentServiceClient _apartmentService;
        private readonly IImageService _imageService;
        public EventController(
            ILogger<EventController> logger,
            IEventService eventService, 
            IUserServiceClient userClient, 
            IApartmentServiceClient apartmentService, 
            IFileStorageClient fileStorageClient,
            IImageService imageService
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _apartmentService = apartmentService ?? throw new ArgumentNullException(nameof(apartmentService));
            _fileStorageClient = fileStorageClient ?? throw new ArgumentNullException(nameof(fileStorageClient));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }

        /// <summary>
        /// Создать новое событие
        /// </summary>
        [HttpPost]
        [EndpointDescription("Создать новое событие")]
        public async Task<ActionResult<EventModel>> CreateEventAsync([Description("Модель события для добавления")][FromBody] EventModelWithImageRequest? eventModel,  CancellationToken ct)
        {
            try
            {
                if (!ModelState.IsValid || eventModel == null)
                {
                    return BadRequest(ModelState);
                }

                foreach (var image in eventModel.Images) 
                {
                    if (image.Data != null)
                    {
                        var imageId = await _fileStorageClient.UploadFileAsync(image.Data);
                        image.ImageId = imageId;
                    }
                }

                var result =  await _eventService.CreateEventAsync(eventModel.ConvertToEventMode(), ct);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }

        /// <summary>
        /// Получить все события
        /// </summary>
        [HttpGet]
        [EndpointDescription("Получить все события")]
        public async Task<ActionResult<IEnumerable<EventModelWithImageRequest>>> GetAllEventsAsync(CancellationToken ct)
        {
            try
            {
                List<EventModelWithImageRequest> output = new();

                var events = await _eventService.GetAllEventsAsync(ct);

                if (events?.Value == null || !events.Value.Any())
                {
                    return Ok(Enumerable.Empty<EventModelWithImageRequest>());
                }

                foreach (var ev in events.Value)
                {
                    var eventModel = new EventModelWithImageRequest(ev);

                    foreach (var imageId in ev.ImageIds)
                    {
                        var url = await _fileStorageClient.GetFileUrlAsync(imageId);

                        var image = new ImageRequest
                        {
                            EventId = ev.EventId,
                            FileUrl = url,
                            ImageId = imageId
                        };

                        eventModel.Images.Add(image);
                    }

                    output.Add(eventModel);
                }

                return Ok(output);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }

        /// <summary>
        /// Получить событие по ID
        /// </summary>
        [HttpGet("{id}")]
        [EndpointDescription("Получить событие по ИД")]
        public async Task<ActionResult<EventModelWithImageRequest>> GetEventByIdAsync(long id, CancellationToken ct)
        {
            try
            {
                var eventModel = await _eventService.GetEventByIdAsync(id, ct);

                if (eventModel == null)
                {
                    return NotFound($"Событие с ID {id} не найдена");
                }

                var eventModelWithImageRequest = new EventModelWithImageRequest(eventModel);

                foreach (var imageId in eventModel.ImageIds)
                {
                    var url = await _fileStorageClient.GetFileUrlAsync(imageId);
                    var image = new ImageRequest
                    {
                        EventId = eventModel.EventId,
                        FileUrl = url,
                        ImageId = imageId
                    };

                    eventModelWithImageRequest.Images.Add(image);
                }

                return Ok(eventModelWithImageRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }

        /// <summary>
        /// Получить события пользователя
        /// </summary>
        [HttpGet("{userId}")]
        [EndpointDescription("Получить события пользователя")]
        public async Task<ActionResult<IEnumerable<EventModelWithImageRequest>>> GetAllUserEventsAsync(Guid userId, CancellationToken ct)
        {
            try
            {
                List<EventModelWithImageRequest> output = new();

                var allUserEvents = await _eventService.GetAllUserEventsAsync(userId, ct);
                if (allUserEvents?.Value == null || !allUserEvents.Value.Any())
                {
                    return Ok(Enumerable.Empty<EventModelWithImageRequest>());
                }

                foreach (var userEvent in allUserEvents.Value) 
                {
                    var eventModelWithImageRequest = new EventModelWithImageRequest(userEvent);

                    foreach (var imageId in userEvent.ImageIds)
                    {
                        var url = await _fileStorageClient.GetFileUrlAsync(imageId);
                        var image = new ImageRequest
                        {
                            EventId = userEvent.EventId,
                            FileUrl = url,
                            ImageId = imageId
                        };

                        eventModelWithImageRequest.Images.Add(image);
                    }
                }

                return Ok(output);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }

        [HttpPut("{id}")]
        [EndpointDescription("Обновить событие")]
        public async Task<IActionResult> UpdateEventAsync([Description("Модель события для обновления")][FromBody] EventModelWithImageRequest updatedEvent, CancellationToken ct)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest();

                var oldEvent = await _eventService.GetEventByIdAsync(updatedEvent.EventId, ct);

                if (oldEvent == null)
                {
                    return NotFound();
                }

                var newImageIds = updatedEvent.Images.Select(x => x.ImageId).ToList();
                var imagesToRemove = oldEvent.ImageIds.Except(newImageIds).ToList();
                var imagesToAdd = newImageIds.Except(oldEvent.ImageIds).ToList();

                foreach (var imageId in imagesToRemove)
                {
                    var imageToRemove = await _imageService.GetImageByIdAsync(imageId, ct);
                    if (imageToRemove != null)
                    {
                        await _fileStorageClient.DeleteFileAsync(imageToRemove.ImageId);
                        await _imageService.DeleteImageAsync(imageToRemove, ct);
                        oldEvent.ImageIds.Remove(imageToRemove.ImageId);
                    }
                }

                foreach (var imageId in imagesToAdd)
                {
                    var imageToAdd = updatedEvent.Images.FirstOrDefault(x => x.ImageId == imageId);

                    if (imageToAdd != null)
                    {
                        if (imageToAdd.Data != null)
                        {
                            await _fileStorageClient.UploadFileAsync(imageToAdd.Data);
                        }

                        var id = await _imageService.CreateImageAsync(imageToAdd, ct);
                        oldEvent.ImageIds.Add(id);
                    }
                }

                await _eventService.UpdateEventAsync(oldEvent, ct);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }

        /// <summary>
        /// Удалить событие
        /// </summary>
        [HttpDelete("{id}")]
        [EndpointDescription("Удалить событие")]
        public async Task<IActionResult> DeleteEventAsync(int id, CancellationToken ct)
        {
            try
            {
                var eventToRemove = await _eventService.GetEventByIdAsync(id, ct);

                if (eventToRemove == null)
                {
                    return NotFound($"Событие с ID {id} не найдено");
                }

                await _eventService.DeleteEventAsync(eventToRemove, ct);

                foreach (var imageId in eventToRemove.ImageIds)
                {
                    var imageToRemove = await _imageService.GetImageByIdAsync(imageId, ct);

                    if (imageToRemove != null)
                    {
                        await _fileStorageClient.DeleteFileAsync(imageToRemove.ImageId);
                        await _imageService.DeleteImageAsync(imageToRemove, ct);
                    }
                }


                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }
    }
}

