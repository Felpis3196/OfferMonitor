#  OfferMonitor

Monitoramento inteligente de ofertas — automatize a busca, análise e visualização de promoções em tempo real.  
*Projeto desenvolvido por: Felipe Bello*

---

##  Sobre o Projeto

O *OfferMonitor* é uma plataforma moderna para *rastreamento e análise de ofertas*, integrando scraping, processamento e exibição visual de resultados.

Com uma arquitetura full-stack modular (*.NET + Vue 3 + TypeScript), o sistema é ideal para aplicações que precisam de **coleta contínua de dados, **painéis dinâmicos* e *alertas de preço*.

---

##  Stack Tecnológica

| Camada        | Tecnologia                  |
|---------------|-----------------------------|
| *Front-end* | Vue 3 • Vite • TypeScript   |
| *Back-end*  | .NET 7 • REST API           |
| *Infra*     | Docker Compose • Node.js    |
| *Comunicação* | Axios • API modular       |
| *Outros*    | Loggers • Hooks • Scripts   |

---

##  Como Executar

###  Pré-requisitos
- Docker e Docker Compose instalados
- Node.js (v18+) para o front-end

###  Execução rápida via Docker
```bash
docker-compose up --build
