# ==============================
# BUILD
# ==============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia projetos
COPY ["Scraper/Scraper.csproj", "Scraper/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

RUN dotnet restore "Scraper/Scraper.csproj"

COPY . .
WORKDIR "/src/Scraper"
RUN dotnet publish "Scraper.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==============================
# FINAL: .NET runtime
# ==============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app

# Copia aplicação publicada
COPY --from=build /app/publish .

# Usuário seguro
RUN useradd -m appuser
USER appuser

ENTRYPOINT ["dotnet", "Scraper.dll"]
