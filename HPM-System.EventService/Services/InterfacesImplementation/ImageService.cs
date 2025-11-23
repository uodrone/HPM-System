using HPM_System.EventService.Models;
using HPM_System.EventService.Repositories;
using HPM_System.EventService.Services.Interfaces;

namespace HPM_System.EventService.Services.InterfacesImplementation
{
    public class ImageService : IImageService
    {
        private readonly IImageModelRepository _imageModelRepository;

        public ImageService(IImageModelRepository imageModelRepository)
        {
            _imageModelRepository = imageModelRepository ?? throw new ArgumentNullException(nameof(imageModelRepository));
        }

        public async Task<int> CreateImageAsync(ImageModel value, CancellationToken ct)
        {
            return await _imageModelRepository.AddAsync(value, ct);
        }

        public async Task DeleteImageAsync(ImageModel value, CancellationToken ct)
        {
            await _imageModelRepository.DeleteAsync(value, ct);
        }

        public async Task<IEnumerable<ImageModel>> GetAllImageAsync(CancellationToken ct)
        {
            return await _imageModelRepository.GetAllAsync(ct);
        }

        public async Task<ImageModel?> GetImageByIdAsync(long id, CancellationToken ct)
        {
            return await _imageModelRepository.GetByIdAsync(id, ct);
        }

        public async Task UpdateImageAsync(ImageModel value, CancellationToken ct)
        {
            await _imageModelRepository.UpdateAsync(value, ct);
        }
    }
}
