using DTO;
using VotingService.Models;

namespace VotingService.Services;

public class MockApartmentServiceClient : IApartmentServiceClient
{
    public Task<List<ApartmentResponseDto>> GetApartmentsByHouseIdAsync(long houseId)
    {
        var apartments = new List<ApartmentResponseDto>();

        // Для теста: генерируем 3 квартиры в каждом доме
        for (int i = 1; i <= 3; i++)
        {
            var apartmentId = (houseId * 100) + i; // уникальный ID для каждой квартиры
            var apartmentNumber = i * 10; // например, 10, 20, 30...
            var totalArea = 50m + (i * 5); // 55, 60, 65, 70, 75
            var residentialArea = totalArea - 5; // 50, 55, 60...

            var apartment = new ApartmentResponseDto
            {
                Id = apartmentId,
                Number = apartmentNumber,
                NumbersOfRooms = i > 3 ? 3 : 2, // первые 3 — 2-комнатные, остальные — 3-комнатные
                ResidentialArea = residentialArea,
                TotalArea = totalArea,
                Floor = i,
                EntranceNumber = (i % 2) + 1, // 1 или 2
                HouseId = houseId,
                Users = new List<ApartmentUserResponseDto>()
            };

            // Добавляем 1 или 2 собственника в квартиру
            var userCount = i % 2 == 0 ? 2 : 1; // каждая 2-я квартира — 2 собственника

            for (int u = 0; u < userCount; u++)
            {
                var userIndex = (int)houseId * 10 + i * 2 + u; // уникальный индекс для GUID
                var userId = Guid.Parse($"f47ac10b-58cc-4372-a567-{userIndex:D12}".Substring(0, 36).PadRight(36, '0').Replace("0000", $"{userIndex % 10000:D4}"));
                var share = userCount == 2 ? 0.5m : 1.0m;

                apartment.Users.Add(new ApartmentUserResponseDto
                {
                    UserId = userId,
                    Share = share,
                    UserDetails = new UserDto
                    {
                        Id = userId,
                        FirstName = $"Имя_{userIndex}",
                        LastName = $"Фамилия_{userIndex}",
                        Patronymic = $"Отчество_{userIndex}",
                        PhoneNumber = $"+7900{userIndex:D6}",
                        Email = $"user{userIndex}@example.com"
                    },
                    Statuses = new List<StatusDto>
                    {
                        new() { Id = 1, Name = "Собственник" }
                    }
                });
            }

            apartments.Add(apartment);
        }

        return Task.FromResult(apartments);
    }
}