namespace HPM_System.EventService.Models
{
    public class ImageModel
    {
        public long ImageId { get; set; }
        public long EventId { get; set; }
        public string? ImageName { get; set; }
        public string? ImageUrl { get; set; }
    }
}
