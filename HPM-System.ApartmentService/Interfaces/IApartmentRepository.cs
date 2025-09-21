using HPM_System.ApartmentService.Models;

namespace HPM_System.ApartmentService.Interfaces
{
    public interface IApartmentRepository
    {
        Task<IEnumerable<Apartment>> GetAllApartmentsAsync();
        Task<Apartment> GetApartmentByIdAsync(long id);
        Task<IEnumerable<Apartment>> GetApartmentsByUserIdAsync(Guid userId);
        Task<Apartment> CreateApartmentAsync(Apartment apartment);
        Task<bool> UpdateApartmentAsync(Apartment apartment);
        Task<bool> DeleteApartmentAsync(long id);
        Task<ApartmentUser> AddUserToApartmentAsync(long apartmentId, Guid userId);
        Task<bool> RemoveUserFromApartmentAsync(long apartmentId, Guid userId);
        Task<ApartmentUser> GetUserApartmentLinkAsync(long apartmentId, Guid userId);
        Task<bool> ApartmentExistsAsync(long id);
        Task SaveChangesAsync();
    }
}
