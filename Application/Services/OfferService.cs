using Application.Dto;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class OfferService : IOfferService
    {
        private readonly IOfferRepository _offerRepository;

        public OfferService(IOfferRepository offerRepository)
        {
            _offerRepository = offerRepository;
        }

        public async Task<IEnumerable<OfferDto>> GetAllAsync()
        {
            var offers = await _offerRepository.GetAllAsync();

            return offers.Select(o => new OfferDto
            {
                Id = o.Id,
                Title = o.Title,
                Store = o.Store,
                Category = o.Category,
                Url = o.Url,
                Price = o.Price
            });
        }

        public async Task<OfferDto?> GetByIdAsync(Guid id)
        {
            var offer = await _offerRepository.GetByIdAsync(id);

            if (offer == null) return null;

            return new OfferDto
            {
                Id = offer.Id,
                Title = offer.Title,
                Store = offer.Store,
                Category = offer.Category,
                Url = offer.Url,
                Price = offer.Price
            };
        }

        public async Task<Offer> CreateAsync(OfferDto dto)
        {
            var offer = new Offer
            {
                Title = dto.Title,
                Store = dto.Store,
                Category = dto.Category,
                Url = dto.Url,
                Price = dto.Price
            };

            return await _offerRepository.AddAsync(offer);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _offerRepository.DeleteAsync(id);
        }
    }
}
