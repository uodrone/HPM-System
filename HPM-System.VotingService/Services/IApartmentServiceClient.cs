
using DTO;

namespace VotingService.Services;

public interface IApartmentServiceClient
{
    Task<List<ApartmentResponseDto>> GetApartmentsByHouseIdAsync(long houseId);
}