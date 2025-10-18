using HPM_System.ApartmentService.Models;

namespace HPM_System.ApartmentService.Interfaces
{
    public interface IStatusRepository
    {
        Task<IEnumerable<Status>> GetAllStatusesAsync();
        Task<Status> GetStatusByIdAsync(int id);
        Task<Status> CreateStatusAsync(Status status);
        Task<bool> UpdateStatusAsync(Status status);
        Task<bool> DeleteStatusAsync(int id);
        Task<bool> IsStatusUsedAsync(int statusId);
        Task<ApartmentUser> GetApartmentUserAsync(long apartmentId, Guid userId);
        Task SetUserStatusesForApartmentAsync(int apartmentId, Guid userId, IEnumerable<int> statusIds);
        Task<IEnumerable<Status>> GetStatusesByIdsAsync(IEnumerable<int> ids);
        Task<ApartmentUserStatus> GetApartmentUserStatusAsync(long apartmentId, Guid userId, int statusId);
        Task<ApartmentUserStatus> AssignStatusToUserAsync(ApartmentUserStatus apartmentUserStatus);
        Task<bool> RevokeStatusFromUserAsync(long apartmentId, Guid userId, int statusId);
        Task<IEnumerable<Status>> GetUserStatusesForApartmentAsync(long apartmentId, Guid userId);
        Task<Status> FindStatusByNameAsync(string name, int? excludeId = null);
        Task SaveChangesAsync();
    }
}
