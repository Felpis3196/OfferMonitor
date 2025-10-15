# ==============================
#  BUILD
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
#  FINAL: ASP.NET + Chrome + ChromeDriver
# ==============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Dependências do Chrome
RUN apt-get update && apt-get install -y \
    wget gnupg unzip curl fonts-liberation libasound2 libatk-bridge2.0-0 libatk1.0-0 \
    libdrm2 libxkbcommon0 libxcomposite1 libxdamage1 libxrandr2 libgbm1 libnss3 \
    libx11-xcb1 libxtst6 libxss1 libcups2 xdg-utils libxfont2 libxrender1 libxext6 \
    && wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | gpg --dearmor -o /usr/share/keyrings/google-chrome-keyring.gpg \
    && echo "deb [signed-by=/usr/share/keyrings/google-chrome-keyring.gpg] http://dl.google.com/linux/chrome/deb/ stable main" > /etc/apt/sources.list.d/google-chrome.list \
    && apt-get update \
    && apt-get install -y google-chrome-stable \
    && rm -rf /var/lib/apt/lists/*

# ChromeDriver compatível
RUN CHROME_VERSION=$(google-chrome --version | grep -oP '\d+' | head -1) && \
    CHROMEDRIVER_VERSION=$(curl -s "https://googlechromelabs.github.io/chrome-for-testing/LATEST_RELEASE_${CHROME_VERSION}") && \
    wget -q -O /tmp/chromedriver.zip "https://edgedl.me.gvt1.com/edgedl/chrome/chrome-for-testing/${CHROMEDRIVER_VERSION}/linux64/chromedriver-linux64.zip" && \
    unzip /tmp/chromedriver.zip -d /usr/local/bin && \
    chmod +x /usr/local/bin/chromedriver

# Variáveis de ambiente para Selenium
ENV CHROME_BIN=/usr/bin/google-chrome
ENV CHROMEDRIVER_BIN=/usr/local/bin/chromedriver

# Usuário seguro
RUN useradd -m appuser
USER appuser

# Copia aplicação publicada
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Scraper.dll"]
