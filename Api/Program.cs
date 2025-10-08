using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scraper.Services;
using RabbitMQ.Client; 
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // ← Isso registra os controllers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// RabbitMQ
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddHostedService<RabbitMqWorker>();

// Scraper
builder.Services.AddScoped<ScraperService>();

// EF Core + Postgres
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IOfferRepository, OfferRepository>();
builder.Services.AddScoped<IOfferService, OfferService>();

var app = builder.Build();

// Aplica migrations automaticamente
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("Migrations aplicadas com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar migrations: {ex.Message}");
    }
}

// Retry automático para conexões (Banco + RabbitMQ)
const int maxRetries = 10;
const int delaySeconds = 5;

for (int i = 0; i < maxRetries; i++)
{
    try
    {
        Console.WriteLine($"Tentando conectar... tentativa {i + 1}/{maxRetries}");

        // Testa conexão com banco
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.OpenConnection();
            db.Database.CloseConnection();
        }

        // Testa conexão com RabbitMQ
        var cfg = app.Configuration;
        var factory = new ConnectionFactory()
        {
            HostName = cfg["RabbitMQ:Host"],
            UserName = cfg["RabbitMQ:Username"],
            Password = cfg["RabbitMQ:Password"]
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        Console.WriteLine("Conexão com PostgreSQL e RabbitMQ bem-sucedida!");
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Falha na tentativa {i + 1}/{maxRetries}: {ex.Message}");
        if (i == maxRetries - 1)
        {
            Console.WriteLine("Não foi possível conectar após várias tentativas. Abortando inicialização.");
            throw;
        }
        Thread.Sleep(delaySeconds * 1000);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
