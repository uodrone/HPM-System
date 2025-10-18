using HPM_System.ApartmentService.Interfaces;
using HPM_System.ApartmentService.Models;
using Microsoft.EntityFrameworkCore;
using HPM_System.ApartmentService.Data;

namespace HPM_System.ApartmentService.Repositories
{
public class StatusRepository : IStatusRepository
{
    private readonly AppDbContext _context;

    public StatusRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Status>> GetAllStatusesAsync()
    {
        return await _context.Statuses.ToListAsync();
    }

    public async Task<Status> GetStatusByIdAsync(int id)
    {
        return await _context.Statuses.FindAsync(id);
    }

    public async Task<Status> CreateStatusAsync(Status status)
    {
        _context.Statuses.Add(status);
        await _context.SaveChangesAsync();
        return status;
    }

    public async Task<bool> UpdateStatusAsync(Status status)
    {
        _context.Entry(status).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            if (!await StatusExistsAsync(status.Id))
                return false;
            else
                throw;
        }
    }

    public async Task<bool> DeleteStatusAsync(int id)
    {
        var status = await _context.Statuses.FindAsync(id);
        if (status == null) return false;

        _context.Statuses.Remove(status);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsStatusUsedAsync(int statusId)
    {
        return await _context.ApartmentUserStatuses.AnyAsync(aus => aus.StatusId == statusId);
    }

    public async Task<ApartmentUser> GetApartmentUserAsync(long apartmentId, Guid userId)
    {
        return await _context.ApartmentUsers
            .FirstOrDefaultAsync(au => au.ApartmentId == apartmentId && au.UserId == userId);
    }

    public async Task<ApartmentUserStatus> GetApartmentUserStatusAsync(long apartmentId, Guid userId, int statusId)
    {
        return await _context.ApartmentUserStatuses
            .FirstOrDefaultAsync(aus => aus.ApartmentId == apartmentId &&
                                      aus.UserId == userId &&
                                      aus.StatusId == statusId);
    }

    public async Task<ApartmentUserStatus> AssignStatusToUserAsync(ApartmentUserStatus apartmentUserStatus)
    {
        _context.ApartmentUserStatuses.Add(apartmentUserStatus);
        await _context.SaveChangesAsync();
        return apartmentUserStatus;
    }

    public async Task<bool> RevokeStatusFromUserAsync(long apartmentId, Guid userId, int statusId)
    {
        var apartmentUserStatus = await _context.ApartmentUserStatuses
            .FirstOrDefaultAsync(aus => aus.ApartmentId == apartmentId &&
                                      aus.UserId == userId &&
                                      aus.StatusId == statusId);

        if (apartmentUserStatus == null) return false;

        _context.ApartmentUserStatuses.Remove(apartmentUserStatus);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Status>> GetStatusesByIdsAsync(IEnumerable<int> ids)
    {
        if (ids == null || !ids.Any())
            return Enumerable.Empty<Status>();

        return await _context.Statuses
            .Where(s => ids.Contains(s.Id))
            .ToListAsync();
    }

        public async Task SetUserStatusesForApartmentAsync(int apartmentId, Guid userId, IEnumerable<int> statusIds)
    {
        var currentStatusAssignments = await _context.ApartmentUserStatuses
            .Where(aus => aus.ApartmentId == apartmentId && aus.UserId == userId)
            .ToListAsync();

        var currentStatusIds = currentStatusAssignments.Select(aus => aus.StatusId).ToHashSet();
        var newStatusIds = new HashSet<int>(statusIds);

        // Удаляем лишние
        var toRemove = currentStatusAssignments
            .Where(aus => !newStatusIds.Contains(aus.StatusId))
            .ToList();

        // Добавляем недостающие
        var toAdd = newStatusIds
            .Except(currentStatusIds)
            .Select(id => new ApartmentUserStatus
            {
                ApartmentId = apartmentId,
                UserId = userId,
                StatusId = id
            })
            .ToList();

        // Выполняем изменения
        if (toRemove.Any())
        {
            _context.ApartmentUserStatuses.RemoveRange(toRemove);
        }

        if (toAdd.Any())
        {
            await _context.ApartmentUserStatuses.AddRangeAsync(toAdd);
        }
    }

        public async Task<IEnumerable<Status>> GetUserStatusesForApartmentAsync(long apartmentId, Guid userId)
    {
        return await _context.ApartmentUserStatuses
            .Where(aus => aus.ApartmentId == apartmentId && aus.UserId == userId)
            .Include(aus => aus.Status)
            .Select(aus => aus.Status)
            .ToListAsync();
    }

    public async Task<Status> FindStatusByNameAsync(string name, int? excludeId = null)
    {
        return await _context.Statuses
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower() &&
                                   (excludeId == null || s.Id != excludeId));
    }

    private async Task<bool> StatusExistsAsync(int id)
    {
        return await _context.Statuses.AnyAsync(e => e.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
}
