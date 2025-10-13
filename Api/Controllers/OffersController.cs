using Application.Dto;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Scraper.Services;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _service;
        private readonly ScraperService _scraperService;

        public OffersController(IOfferService service, ScraperService scraper)
        {
            _service = service;
            _scraperService = scraper;
        }

        // GET api/offers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OfferDto>>> GetAll()
        {
            var offers = await _service.GetAllAsync();
            return Ok(offers);
        }

        // GET api/offers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OfferDto>> GetById(Guid id)
        {
            var offer = await _service.GetByIdAsync(id);
            if (offer == null) return NotFound();
            return Ok(offer);
        }

        // POST api/offers
        [HttpPost]
        public async Task<ActionResult<Offer>> Create([FromBody] OfferDto dto)
        {
            var newOffer = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newOffer.Id }, newOffer);
        }

        // DELETE api/offers/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpPost("scrape")]
        public async Task<IActionResult> RunScraper([FromBody] ScraperRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                return BadRequest("A URL é obrigatória.");

            await _scraperService.RunScraperAsync(request.Url);
            return Ok($"Scraper executado com sucesso para o site: {request.Url}");
        }
    }
}
