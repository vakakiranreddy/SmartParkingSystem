using Microsoft.EntityFrameworkCore;
using SmartParkingSystem.Data;
using SmartParkingSystem.Interfaces.Repositories;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Repositories
{
    public class GuestRepository : IGuestRepository
    {
        private readonly ParkingDbContext _context;

        public GuestRepository(ParkingDbContext context)
        {
            _context = context;
        }

        public async Task<Guest> GetByIdAsync(int id)
        {
            return await _context.Guests.FindAsync(id);
        }

        public async Task<IEnumerable<Guest>> GetAllAsync()
        {
            return await _context.Guests.ToListAsync();
        }

        public async Task<Guest> AddAsync(Guest guest)
        {
            _context.Guests.Add(guest);
            await _context.SaveChangesAsync();
            return guest;
        }

        public async Task<Guest> UpdateAsync(Guest guest)
        {
            _context.Entry(guest).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return guest;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null)
                return false;

            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Guests.AnyAsync(g => g.Id == id);
        }

        public async Task<Guest> GetByLicensePlateAsync(string licensePlate)
        {
            return await _context.Guests
                .FirstOrDefaultAsync(g => g.LicensePlate == licensePlate);
        }

        public async Task<bool> LicensePlateExistsAsync(string licensePlate)
        {
            return await _context.Guests
                .AnyAsync(g => g.LicensePlate == licensePlate);
        }
    }
}