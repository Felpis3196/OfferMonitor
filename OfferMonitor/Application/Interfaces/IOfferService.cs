using Application.Dto;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IOfferService
    {
        Task<IEnumerable<OfferDto>> GetAllAsync();
        Task<OfferDto?> GetByIdAsync(Guid id);
        Task<Offer> CreateAsync(OfferDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<int> DeleteAllAsync();
    }
}
