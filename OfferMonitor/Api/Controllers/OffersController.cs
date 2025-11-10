using Application.Dto;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using Scraper.Helpers;
using Scraper.Services;
using System.Text;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _service;

        public OffersController(IOfferService service)
        {
            _service = service;
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

        // DELETE api/offers/{id}
        [HttpDelete]
        public async Task<ActionResult> Delete()
        {
            try
            {
                _service.DeleteAllAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Erro ao deletar todas as ofertas.");
            }
            
        }

        [HttpPost("scrape")]
        public IActionResult RequestScraping([FromBody] ScraperRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                return BadRequest("A URL é obrigatória.");

            using var connection = RabbitMqHelper.GetConnectionWithRetry("rabbitmq", "guest", "guest");
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: "scrape_requests",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var body = Encoding.UTF8.GetBytes(request.Url);
            channel.BasicPublish(
                exchange: "",
                routingKey: "scrape_requests",
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"📩 Pedido de scraping publicado: {request.Url}");
            return Ok($"📩 Pedido de scraping enviado para: {request.Url}");
        }

    }
}
