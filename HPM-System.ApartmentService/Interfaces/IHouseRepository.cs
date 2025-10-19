using HPM_System.ApartmentService.Models;

namespace HPM_System.ApartmentService.Interfaces
{
    public interface IHouseRepository
    {
        Task<IEnumerable<House>> GetAllHousesAsync();
        Task<House> GetHouseByIdAsync(long id);
        Task<House> CreateHouseAsync(House house);
        Task<bool> UpdateHouseAsync(House house);
        Task<bool> DeleteHouseAsync(long id);
        Task<bool> IsUserLinkedToAnyApartmentInHouseAsync(long houseId, Guid userId);
        Task<IEnumerable<House>> GetHousesByUserIdAsync(Guid userId);
        Task SaveChangesAsync();
    }
}
