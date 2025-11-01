using HPM_System.EventService.DataContext;
using HPM_System.EventService.Models;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.EventService.Repositories
{
    public class ImageModelRepository : IImageModelRepository
    {
        private readonly ServiceDbContext _dbContext;
        public ImageModelRepository(ServiceDbContext context)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> AddAsync(ImageModel value, CancellationToken ct)
        {
            var result = await _dbContext.Images.AddAsync(value, ct);
            return await _dbContext.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(ImageModel value, CancellationToken ct)
        {
            _dbContext.Images.Remove(value);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<ImageModel>> GetAllAsync(CancellationToken ct)
        {
            return await _dbContext.Images.ToListAsync(ct);
        }

        public async Task<ImageModel?> GetByIdAsync(long id, CancellationToken ct)
        {
            return await _dbContext.Images.FirstOrDefaultAsync(x => x.ImageId == id, ct);
        }

        public async Task UpdateAsync(ImageModel value, CancellationToken ct)
        {
            var entity = await _dbContext.Images.FirstOrDefaultAsync(x => x.ImageId == value.ImageId);

            if (entity == null)
            {
                return;
            }

            entity.ImageName = value.ImageName;
            entity.ImageUrl = value.ImageUrl;

            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
