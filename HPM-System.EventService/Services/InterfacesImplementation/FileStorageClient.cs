using HPM_System.EventService.DTOs;
using HPM_System.EventService.Services.Interfaces;
using System.Net;
using System.Text.Json;

namespace HPM_System.EventService.Services.InterfacesImplementation
{
    public class FileStorageClient : ClientBase, IFileStorageClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FileStorageClient> _logger;
        private readonly string _fileStorageBaseUrl;
        private readonly string _bucketName = "events-images";

        public FileStorageClient(
            HttpClient httpClient, 
            ILogger<FileStorageClient> logger, 
            IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentException(nameof(logger));

            // TODO: 
            // не уверен до конца, что это тот адрес http://localhost:тут должны быть некие цифры,
            // по которому стучаться! Важно проверить и доделать!

            _fileStorageBaseUrl = configuration["Services:FileStorageService:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:55688";
        }

        public async Task DeleteFileAsync(long fileId)
        {
            if (fileId == default)
            {
                _logger.LogWarning("Некорректный Id файла {fileId}", fileId);
                return;
            }

            string requestUrl = $"{_fileStorageBaseUrl}/api/files/{fileId}";

            try
            {
                _logger.LogDebug("Отправка запроса на удаление файла по ID: {requestUrl}", requestUrl);

                var response = await _httpClient.DeleteAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Файл успешно удалён");
                    return;
                }
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Файл с Id {fileId} не найден (404)", fileId);
                    return;
                }

                _logger.LogWarning("Не успешный HTTP статус при удалении {fileId}: {StatusCode} {ReasonPhrase}",
                    fileId,
                    response.StatusCode,
                    response.ReasonPhrase);

                response.EnsureSuccessStatusCode();
                return;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP ошибка при удалении файла {fileId} по URL {requestUrl}", fileId, requestUrl);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Таймаут или отмена запроса при удалении файла {fileId} по URL {requestUrl}", fileId, requestUrl);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Ошибка десериализации JSON при удалении файла {fileId} по URL {requestUrl}", fileId, requestUrl);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданное исключение при удалении файла {fileId} по URL {requestUrl}", fileId, requestUrl);
                throw;
            }
        }

        public async Task<FileMetadataDTO?> GetFileMetadataAsync(long fileId)
        {
            if (fileId == default)
            {
                _logger.LogWarning("Некорректный Id файла {fileId}", fileId);
                return null;
            }

            string requestUrl = $"{_fileStorageBaseUrl}/api/files/{fileId}";

            try
            {
                _logger.LogDebug("Отправка запроса на получение метаданных файла по ID: {Url}", requestUrl);

                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Получен успешный ответ для файла ID {fileId}. Длина контента: {Length} символов.", fileId, content.Length);

                    return JsonSerializer.Deserialize<FileMetadataDTO>(
                        content,
                        Options);
                }
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Файл с Id {fileId} не найден (404)", fileId);
                    return null;
                }

                _logger.LogWarning("Не успешный HTTP статус при получении метаданных файла {fileId}: {StatusCode} {ReasonPhrase}",
                    fileId, 
                    response.StatusCode, 
                    response.ReasonPhrase);

                response.EnsureSuccessStatusCode();
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP ошибка при получении метаданных файла {fileId} по URL {requestUrl}", fileId, requestUrl);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Таймаут или отмена запроса при получении метаданных файла {fileId} по URL {requestUrl}", fileId, requestUrl);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Ошибка десериализации JSON при получении метаданных файла {fileId} по URL {requestUrl}", fileId, requestUrl);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданное исключение при получении метаданных файла {fileId} по URL {requestUrl}", fileId, requestUrl);
                throw;
            }
        }


        public async Task<string?> GetFileUrlAsync(long fileId)
        {
            var metadata = await GetFileMetadataAsync(fileId);
            if (metadata is null)
            {
                return null;
            }
            string baseUrl = $"{_fileStorageBaseUrl}/api/files/view/{metadata.BucketName}/{metadata.FileName}";
            return baseUrl;
        }

        public async Task<int> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Файл не предоставлен");
            }

            using var formContent = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(file.ContentType);
            formContent.Add(fileContent, "file", file.FileName);

            string requestUrl = $"{_fileStorageBaseUrl}/api/files/upload";
            try
            {
                var response = await _httpClient.PostAsync(requestUrl, formContent);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FileUploadResultDTO>(responseContent, Options);

                if (result == null)
                {
                    throw new InvalidOperationException("Не удалось десериализовать файл");
                }

                if (result.Id == 0)
                {
                    throw new InvalidOperationException("Не удалось получить Id файла");
                }

                _logger.LogInformation("Файл успешно загружен, {Message}, Id: {Id}, URL: {FileUrl}", 
                    result.Message, 
                    result.Id, 
                    result.FileUrl);

                return result.Id;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке файла в FileStorageServise");
                throw new InvalidOperationException("Не удалось загрузить файл: проверьте доступность сервиса.", ex);
            }
        }
    }
}
