using HPM_System.EventService.DataContext;
using HPM_System.EventService.Models;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.EventService.Repositories
{
    public class EventModelRepository : IEventModelRepository
    {
        private readonly ServiceDbContext _dbContext;
        private readonly IImageModelRepository _imageRepository;
        public EventModelRepository(ServiceDbContext context, IImageModelRepository imageRepository)
        {            
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        }

        public async Task<int> AddAsync(EventModel value, CancellationToken ct)
        {
            var result = await _dbContext.Events.AddAsync(value, ct);

            return await _dbContext.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(EventModel value, CancellationToken ct)
        {
            _dbContext.Events.Remove(value);

            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<EventModel>> GetAllAsync(CancellationToken ct)
        {
            return await _dbContext.Events.ToListAsync(ct);
        }

        public async Task<IEnumerable<EventModel>> GetAllUserEventsAsync(Guid userId, CancellationToken ct)
        {
            return await _dbContext.Events.Where(x => x.UserId == userId).ToListAsync(ct);
        }

        public async Task<EventModel?> GetByIdAsync(long id, CancellationToken ct)
        {
            return await _dbContext.Events.FirstOrDefaultAsync(x => x.EventId == id, ct);
        }

        public async Task UpdateAsync(EventModel value, CancellationToken ct)
        {
            var entity = await _dbContext.Events.FirstOrDefaultAsync(x => x.EventId == value.EventId);

            if (entity == null)
            {
                return;
            }

            entity.Place = value.Place;
            entity.EventName = value.EventName;
            entity.EventDescription = value.EventDescription;
            entity.EventDateTime = value.EventDateTime;
            entity.HouseId = value.HouseId;
            entity.UserId = value.UserId;
            entity.ImageIds = value.ImageIds;

            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
