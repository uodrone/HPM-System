// Services/IUserServiceClient.cs
using HPM_System.ApartmentService.DTOs;

namespace HPM_System.ApartmentService.Services
{
    /// <summary>
    /// Интерфейс для взаимодействия с микросервисом пользователей.
    /// </summary>
    public interface IUserServiceClient
    {
        /// <summary>
        /// Асинхронно получает информацию о пользователе по его ID.
        /// </summary>
        /// <param name="userId">ID пользователя.</param>
        /// <returns>
        /// Объект UserDto, если пользователь найден;
        /// null, если пользователь не найден (404 Not Found);
        /// Исключение, если произошла сетевая ошибка, таймаут или ошибка десериализации.
        /// </returns>
        /// <exception cref="HttpRequestException">Произошла ошибка HTTP при запросе.</exception>
        /// <exception cref="TaskCanceledException">Запрос был отменен или превысил время ожидания.</exception>
        /// <exception cref="System.Text.Json.JsonException">Ошибка при десериализации JSON-ответа.</exception>
        Task<UserDto?> GetUserByIdAsync(int userId);

        /// <summary>
        /// Асинхронно получает информацию о пользователе по его номеру телефона.
        /// </summary>
        /// <param name="phone">Номер телефона пользователя.</param>
        /// <returns>
        /// Объект UserDto, если пользователь найден;
        /// null, если пользователь не найден (404 Not Found);
        /// Исключение, если произошла сетевая ошибка, таймаут или ошибка десериализации.
        /// </returns>
        /// <exception cref="HttpRequestException">Произошла ошибка HTTP при запросе.</exception>
        /// <exception cref="TaskCanceledException">Запрос был отменен или превысил время ожидания.</exception>
        /// <exception cref="System.Text.Json.JsonException">Ошибка при десериализации JSON-ответа.</exception>
        Task<UserDto?> GetUserByPhoneAsync(string phone);

        /// <summary>
        /// Асинхронно проверяет, существует ли пользователь с заданным ID.
        /// </summary>
        /// <param name="userId">ID пользователя.</param>
        /// <returns>
        /// true, если пользователь существует (2xx OK);
        /// false, если пользователь не существует (404 Not Found) или сервер вернул ошибку (4xx/5xx, кроме таймаута/сетевой ошибки);
        /// Исключение, если произошла сетевая ошибка или таймаут.
        /// </returns>
        /// <exception cref="HttpRequestException">Произошла ошибка HTTP при запросе.</exception>
        /// <exception cref="TaskCanceledException">Запрос был отменен или превысил время ожидания.</exception>
        Task<bool> UserExistsAsync(int userId);
    }
}