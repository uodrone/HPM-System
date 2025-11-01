using HPM_System.EventService.DTOs;
using HPM_System.EventService.Models;
using HPM_System.EventService.Repositories;
using HPM_System.EventService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HPM_System.EventService.Services.InterfacesImplementation
{
    public class EventService : IEventService
    {
        private readonly IEventModelRepository _eventRepository;
        private readonly IUserServiceClient _userClient;
        private readonly IApartmentServiceClient _apartmentService;

        public EventService(IEventModelRepository eventRepository, IUserServiceClient userClient, IApartmentServiceClient apartmentService)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _userClient = userClient ?? throw new ArgumentNullException(nameof(userClient));
            _apartmentService = apartmentService ?? throw new ArgumentNullException(nameof(apartmentService));

            //var allUsers = await _userClient.GetAllUsersAsync();
            //var allApartments = await _apartmentService.GetAllApartmentsAsync();
        }

        public async Task<ActionResult<EventModel>> CreateEventAsync(EventModel? eventModel, CancellationToken ct)
        {
            Random r = new Random(10);
            var next = r.Next(4);

            var newEvent = new EventModel();
            newEvent.Place = $"Место проведения {next}";
            newEvent.EventName = $"Название какото то события {next}";
            newEvent.EventDescription = $"Описание какого то события {next}";
            newEvent.EventDateTime = DateTime.UtcNow;
            newEvent.HouseId = 1L;
            newEvent.UserId = Guid.NewGuid();
            newEvent.ImageIds = new List<long>()
            {
                20L,25L,30L
            };

            var result = await _eventRepository.AddAsync(newEvent, ct);
            newEvent.EventId = result;

            return newEvent;
        }

        public async Task DeleteEventAsync(EventModel model, CancellationToken ct)
        {
            await _eventRepository.DeleteAsync(model, ct);
        }

        public async Task<ActionResult<IEnumerable<EventModel>>> GetAllEventsAsync(CancellationToken ct)
        {
            var result = await _eventRepository.GetAllAsync(ct);
            return result.ToList();
        }

        public async Task<ActionResult<IEnumerable<EventModel>>> GetAllUserEventsAsync(Guid userId, CancellationToken ct)
        {
            var result = await _eventRepository.GetAllUserEventsAsync(userId, ct);
            return result.ToList();
        }

        public async Task<EventModel?> GetEventByIdAsync(long id, CancellationToken ct)
        {
            return await _eventRepository.GetByIdAsync(id, ct);
        }

        public async Task UpdateEventAsync(EventModel updatedEvent, CancellationToken ct)
        {
            await _eventRepository.UpdateAsync(updatedEvent, ct);
        }
    }
}
