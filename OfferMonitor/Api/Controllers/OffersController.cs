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

                // Gera um ID único para este request
                var requestId = Guid.NewGuid().ToString();
                
                // Adiciona log inicial
                _logService.AddLog(requestId, $"📩 Pedido de scraping recebido para: {request.Url}", "INFO");
                _logService.AddLog(requestId, "🔄 Preparando envio para fila de processamento...", "INFO");

                var rabbitMqHost = _configuration["RabbitMQ:Host"] ?? "rabbitmq";
                var rabbitMqUser = _configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitMqPass = _configuration["RabbitMQ:Password"] ?? "guest";

                // Usa menos retries para resposta mais rápida (3 tentativas, 2 segundos cada)
                IConnection connection;
                try
                {
                    _logService.AddLog(requestId, "🔌 Conectando ao RabbitMQ...", "INFO");
                    connection = RabbitMqHelper.GetConnectionWithRetry(rabbitMqHost, rabbitMqUser, rabbitMqPass, maxRetries: 3, delaySeconds: 2);
                    _logService.AddLog(requestId, "✅ Conectado ao RabbitMQ com sucesso", "SUCCESS");
                }
                catch (Exception ex)
                {
                    _logService.AddLog(requestId, $"❌ Erro ao conectar ao RabbitMQ: {ex.Message}", "ERROR");
                    return StatusCode(503, new { 
                        message = "Serviço de mensageria indisponível. Tente novamente mais tarde.", 
                        success = false,
                        requestId = requestId
                    });
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
                    properties.Headers = new Dictionary<string, object>
                    {
                        { "RequestId", requestId }
                    };

                    // Cria mensagem com URL e RequestId
                    var message = new
                    {
                        Url = request.Url,
                        RequestId = requestId
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: "scrape_requests",
                        basicProperties: properties,
                        body: body
                    );

                    _logService.AddLog(requestId, $"✅ Pedido de scraping publicado na fila com sucesso", "SUCCESS");
                    _logService.AddLog(requestId, "⏳ Aguardando processamento pelo serviço de scraping...", "INFO");
                    
                    return Ok(new { 
                        message = $"Pedido de scraping enviado para: {request.Url}", 
                        url = request.Url,
                        requestId = requestId,
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
                var requestId = Guid.NewGuid().ToString();
                _logService.AddLog(requestId, $"❌ Erro ao processar pedido de scraping: {ex.Message}", "ERROR");
                return StatusCode(500, new { 
                    message = "Erro interno ao processar pedido de scraping.", 
                    success = false, 
                    error = ex.Message,
                    requestId = requestId
                });
            }
        }

        [HttpPost("scrape/logs")]
        public IActionResult AddScrapingLog([FromBody] ScrapingLogRequest request)
        {
            try
            {
                _logService.AddLog("global", request.Message, request.Level ?? "INFO");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao adicionar log.", error = ex.Message });
            }
        }

        [HttpGet("scrape/logs")]
        public IActionResult GetScrapingLogs()
        {
            try
            {
                var logs = _logService.GetLogs("global");
                return Ok(new { logs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar logs.", error = ex.Message });
            }
        }
    }

    public class ScrapingLogRequest
    {
        public string RequestId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Level { get; set; }
    }
}
