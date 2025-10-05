using HPM_System.ApartmentService.Interfaces;
using HPM_System.ApartmentService.Models;
using Microsoft.EntityFrameworkCore;
using HPM_System.ApartmentService.Data;

namespace HPM_System.ApartmentService.Repositories
{
    public class ApartmentRepository : IApartmentRepository
    {
        private readonly AppDbContext _context;

        public ApartmentRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Apartment>> GetAllApartmentsAsync()
        {
            return await _context.Apartment
                .Include(a => a.Users)
                    .ThenInclude(au => au.Statuses)
                        .ThenInclude(aus => aus.Status)
                .ToListAsync();
        }

        public async Task<Apartment> GetApartmentByIdAsync(long id)
        {
            return await _context.Apartment
                .Include(a => a.Users)
                    .ThenInclude(au => au.Statuses)
                        .ThenInclude(aus => aus.Status)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Apartment>> GetApartmentsByUserIdAsync(Guid userId)
        {
            return await _context.Apartment
                .Where(a => a.Users.Any(u => u.UserId == userId))
                .Include(a => a.Users)
                    .ThenInclude(au => au.Statuses)
                        .ThenInclude(aus => aus.Status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetApartmentsByHouseIdAsync(long houseId)
        {
            return await _context.Apartment
                .Where(a => a.HouseId == houseId)                
                .ToListAsync();
        }

        public async Task<Apartment> CreateApartmentAsync(Apartment apartment)
        {
            _context.Apartment.Add(apartment);
            await _context.SaveChangesAsync();
            return apartment;
        }

        public async Task<bool> UpdateApartmentAsync(Apartment apartment)
        {
            _context.Entry(apartment).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ApartmentExistsAsync(apartment.Id))
                    return false;
                else
                    throw;
            }
        }

        public async Task<IEnumerable<Apartment>> GetApartmentsByHouseIdWithUsersAndStatusesAsync(long houseId)
        {
            return await _context.Apartment
                .Where(a => a.HouseId == houseId)
                .Include(a => a.Users)
                    .ThenInclude(au => au.Statuses)
                        .ThenInclude(aus => aus.Status)
                .ToListAsync();
        }

        public async Task<bool> DeleteApartmentAsync(long id)
        {
            var apartment = await _context.Apartment
                .Include(a => a.Users)
                    .ThenInclude(au => au.Statuses)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apartment == null) return false;

            var allStatuses = apartment.Users.SelectMany(au => au.Statuses).ToList();
            _context.ApartmentUserStatuses.RemoveRange(allStatuses);
            _context.Apartment.Remove(apartment);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ApartmentUser> AddUserToApartmentAsync(long apartmentId, Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                user = new User { Id = userId };
                _context.Users.Add(user);
            }

            var apartmentUser = new ApartmentUser
            {
                ApartmentId = apartmentId,
                UserId = userId
            };

            _context.ApartmentUsers.Add(apartmentUser);
            await _context.SaveChangesAsync();
            return apartmentUser;
        }

        public async Task<bool> RemoveUserFromApartmentAsync(long apartmentId, Guid userId)
        {
            var apartmentUser = await _context.ApartmentUsers
                .Include(au => au.Statuses)
                .FirstOrDefaultAsync(au => au.ApartmentId == apartmentId && au.UserId == userId);

            if (apartmentUser == null) return false;

            _context.ApartmentUserStatuses.RemoveRange(apartmentUser.Statuses);
            _context.ApartmentUsers.Remove(apartmentUser);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ApartmentUser> GetUserApartmentLinkAsync(long apartmentId, Guid userId)
        {
            return await _context.ApartmentUsers
                .Include(au => au.Statuses)
                    .ThenInclude(aus => aus.Status)
                .FirstOrDefaultAsync(au => au.ApartmentId == apartmentId && au.UserId == userId);
        }

        public async Task<bool> ApartmentExistsAsync(long id)
        {
            return await _context.Apartment.AnyAsync(e => e.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
