using HPM_System.EventService.DTOs;

namespace HPM_System.EventService.Services.Interfaces
{
    public interface IApartmentServiceClient
    {
        Task<ApartmentDTO?> GetApartmentByIdAsync(long apartmentID);

        Task<IEnumerable<ApartmentDTO>?> GetAllApartmentsAsync();
    }
}
