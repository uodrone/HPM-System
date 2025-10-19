using HPM_System.ApartmentService.Interfaces;
using HPM_System.ApartmentService.Models;
using Microsoft.EntityFrameworkCore;
using HPM_System.ApartmentService.Data;

namespace HPM_System.ApartmentService.Repositories
{
    public class HouseRepository : IHouseRepository
    {
        private readonly AppDbContext _context;

        public HouseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<House>> GetAllHousesAsync()
        {
            return await _context.House.OrderBy(a => a.Number).ToListAsync();
        }

        public async Task<House> GetHouseByIdAsync(long id)
        {
            return await _context.House.FindAsync(id);
        }

        public async Task<House> CreateHouseAsync(House house)
        {
            _context.House.Add(house);
            await _context.SaveChangesAsync();
            return house;
        }

        public async Task<bool> UpdateHouseAsync(House house)
        {
            _context.Entry(house).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                if (!await HouseExistsAsync(house.Id))
                    return false;
                else
                    throw;
            }
        }

        public async Task<bool> DeleteHouseAsync(long id)
        {
            var house = await _context.House.FindAsync(id);
            if (house == null) return false;

            _context.House.Remove(house);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserLinkedToAnyApartmentInHouseAsync(long houseId, Guid userId)
        {
            return await _context.Apartment
                .AnyAsync(a => a.HouseId == houseId && a.Users.Any(au => au.UserId == userId));
        }

        private async Task<bool> HouseExistsAsync(long id)
        {
            return await _context.House.AnyAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<House>> GetHousesByUserIdAsync(Guid userId)
        {
            return await _context.Apartment
                .Where(a => a.Users.Any(au => au.UserId == userId))
                .Join(
                    _context.House,               // присоединяем таблицу House
                    a => a.HouseId,               // из Apartment — HouseId
                    h => h.Id,                    // из House — Id
                    (a, h) => h                   // выбираем House
                )
                .Distinct()                       // убираем дубли
                .OrderBy(a => a.Number)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
