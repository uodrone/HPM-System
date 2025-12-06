namespace HPM_System.EventService.DTOs
{
    public class FileUploadResultDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Bucket { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string OriginalFileName {  get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string? DownloadUrl {  get; set; }
    }
}
