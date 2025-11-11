#  OfferMonitor

Monitoramento inteligente de ofertas — automatize a busca, análise e visualização de promoções em tempo real.  
*Projeto desenvolvido por: Felipe Bello*

---

##  Sobre o Projeto

O *OfferMonitor* é uma plataforma moderna para *rastreamento e análise de ofertas*, integrando scraping, processamento e exibição visual de resultados.

Com uma arquitetura full-stack modular (*.NET 9 + Vue 3 + TypeScript + RabbitMQ), o sistema é ideal para aplicações que precisam de **coleta contínua de dados, **painéis dinâmicos* e *alertas de preço*.

---

##  Stack Tecnológica

| Camada        | Tecnologia                          |
|---------------|-------------------------------------|
| *Front-end* | Vue 3 • Vite • TypeScript           |
| *Back-end*  | .NET 9 • REST API • RabbitMQ        |
| *Infra*     | Docker Compose • Node.js • Nginx    |
| *Comunicação* | Axios • API modular               |
| *Outros*    | Loggers • Hooks • Scripts           |

---

##  Como Executar

###  Pré-requisitos
- Docker e Docker Compose instalados
- Node.js (v18+) para o front-end
- .NET 9 SDK para o back-end

###  Execução rápida via Docker
```bash
docker-compose up --build
