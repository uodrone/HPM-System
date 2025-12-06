namespace HPM_System.EventService.DTOs
{
    public class FileMetadataDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; } 
        public string ContentType { get; set; } = string.Empty;
        public string FileHash { get; set; } = string.Empty;
    }
}
