using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OfferRepository : IOfferRepository
    {
        private readonly ApplicationDbContext _context;

        public OfferRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Offer>> GetAllAsync()
        {
            return await _context.Offers.ToListAsync();
        }

        public async Task<Offer?> GetByIdAsync(Guid id)
        {
            return await _context.Offers.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Offer> AddAsync(Offer offer)
        {
            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();
            return offer;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var offer = await _context.Offers.FindAsync(id);
            if (offer == null) return false;

            _context.Offers.Remove(offer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteAllAsync()
        {
            var count = await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Offers\"");
            return count;
        }

    }
}
