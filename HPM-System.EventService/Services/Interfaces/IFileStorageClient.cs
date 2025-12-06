using HPM_System.EventService.DTOs;

namespace HPM_System.EventService.Services.Interfaces
{
    public interface IFileStorageClient
    {
        Task<int> UploadFileAsync(IFormFile file);
        Task<FileMetadataDTO?> GetFileMetadataAsync(long fileId);
        Task<string?> GetFileUrlAsync(long fileId);
        Task DeleteFileAsync(long fileId);
    }
}
