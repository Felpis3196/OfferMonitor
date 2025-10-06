using Application.Dto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOfferService
    {
        Task<IEnumerable<OfferDto>> GetAllAsync();
        Task<OfferDto?> GetByIdAsync(Guid id);
        Task<Offer> CreateAsync(OfferDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
