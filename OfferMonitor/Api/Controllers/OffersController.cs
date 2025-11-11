using Application.Dto;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Scraper.Helpers;
using Scraper.Services;
using System.Text;
using System.Text.Json;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _service;
        private readonly IConfiguration _configuration;
        private readonly IScrapingLogService _logService;

        public OffersController(IOfferService service, IConfiguration configuration, IScrapingLogService logService)
        {
            _service = service;
            _configuration = configuration;
            _logService = logService;
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

        // DELETE api/offers/all
        [HttpDelete("all")]
        public async Task<ActionResult> DeleteAll()
        {
            try
            {
                await _service.DeleteAllAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Erro ao deletar todas as ofertas.");
            }
            
        }

        [HttpPost("scrape")]
        public async Task<IActionResult> RequestScraping([FromBody] ScraperRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Url))
                {
                    return BadRequest(new { message = "A URL é obrigatória.", success = false });
                }

                var rabbitMqHost = _configuration["RabbitMQ:Host"] ?? "rabbitmq";
                var rabbitMqUser = _configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitMqPass = _configuration["RabbitMQ:Password"] ?? "guest";

                // Usa menos retries para resposta mais rápida (3 tentativas, 2 segundos cada)
                IConnection connection;
                try
                {
                    connection = RabbitMqHelper.GetConnectionWithRetry(rabbitMqHost, rabbitMqUser, rabbitMqPass, maxRetries: 3, delaySeconds: 2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao conectar ao RabbitMQ: {ex.Message}");
                    return StatusCode(503, new { message = "Serviço de mensageria indisponível. Tente novamente mais tarde.", success = false });
                }

                try
                {
                    using var channel = connection.CreateModel();

                    channel.QueueDeclare(
                        queue: "scrape_requests",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    var body = Encoding.UTF8.GetBytes(request.Url);
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: "scrape_requests",
                        basicProperties: properties,
                        body: body
                    );

                    Console.WriteLine($"📩 Pedido de scraping publicado: {request.Url}");
                    
                    return Ok(new { 
                        message = $"Pedido de scraping enviado para: {request.Url}", 
                        url = request.Url,
                        success = true 
                    });
                }
                finally
                {
                    connection?.Close();
                    connection?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao processar pedido de scraping: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Erro interno ao processar pedido de scraping.", success = false, error = ex.Message });
            }
        }

    }
}
