using HPM_System.EventService.Models;

namespace HPM_System.EventService.Services.Interfaces
{
    public interface IImageService
    {
        public Task<int> CreateImageAsync(ImageModel value, CancellationToken ct);
        public Task DeleteImageAsync(ImageModel value, CancellationToken ct);
        public Task<IEnumerable<ImageModel>> GetAllImageAsync(CancellationToken ct);
        public Task<ImageModel?> GetImageByIdAsync(long id, CancellationToken ct);
        public Task UpdateImageAsync(ImageModel value, CancellationToken ct);
    }
}
