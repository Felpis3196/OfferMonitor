using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scraper.Services;
using RabbitMQ.Client; 
using Scraper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Garante que o JSON usa camelCase (url ao invés de Url)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS - Permite requisições do front-end
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "http://localhost:8080",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:3000"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// RabbitMQ
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddHostedService<RabbitMqWorker>();

// EF Core + Postgres
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IOfferRepository, OfferRepository>();
builder.Services.AddScoped<IOfferService, OfferService>();
builder.Services.AddSingleton<Application.Services.IScrapingLogService, Application.Services.ScrapingLogService>();

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

// CORS deve ser chamado antes de outros middlewares
app.UseCors("AllowFrontend");

// Removido UseHttpsRedirection para evitar problemas com CORS em desenvolvimento
// app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();
