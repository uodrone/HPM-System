
namespace HPM_System.EventService.Models
{
    public class ImageRequest : ImageModel
    {
        // Todo: тут то, что относится к картинке и приходит с фронта

        public IFormFile? Data { get; set; }

        public string? FileUrl { get; set; }
    }
}
