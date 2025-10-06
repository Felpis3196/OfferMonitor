using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IOfferRepository
    {
        Task<IEnumerable<Offer>> GetAllAsync();
        Task<Offer?> GetByIdAsync(Guid id);
        Task<Offer> AddAsync(Offer offer);
        Task<bool> DeleteAsync(Guid id);
    }
}
