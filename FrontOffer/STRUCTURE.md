# Estrutura do Frontend

## 📁 Organização de Pastas

```
src/
├── api/                    # Camada de comunicação com a API
│   ├── client.ts          # Cliente HTTP configurado
│   └── services/          # Serviços específicos por domínio
│       └── offerService.ts
│
├── types/                  # Definições de tipos TypeScript
│   └── offer.ts           # Tipos relacionados a ofertas
│
├── composables/            # Composables Vue (lógica reutilizável)
│   └── useOffers.ts       # Gerenciamento de estado das ofertas
│
├── components/             # Componentes Vue reutilizáveis
│   ├── OfferCard.vue      # Card de exibição de oferta
│   └── ScrapeForm.vue     # Formulário de solicitação de scraping
│
├── views/                  # Páginas/Views da aplicação
│   └── OffersView.vue     # Página principal de ofertas
│
├── config/                 # Configurações da aplicação
│   └── api.ts             # Configuração da API (URLs, endpoints)
│
├── utils/                  # Funções utilitárias
│   └── constants.ts       # Constantes da aplicação
│
├── App.vue                 # Componente raiz
└── main.ts                # Ponto de entrada da aplicação
```

## 🔄 Fluxo de Dados

1. **View** (`OffersView.vue`) → Usa o **Composable** (`useOffers`)
2. **Composable** → Chama o **Service** (`offerService`)
3. **Service** → Usa o **Client** (`apiClient`) para fazer requisições HTTP
4. **Client** → Faz requisições para a API usando `fetch`

## 🎯 Responsabilidades

### `api/client.ts`
- Cliente HTTP centralizado
- Tratamento de erros
- Configuração de headers

### `api/services/offerService.ts`
- Métodos específicos para operações com ofertas
- Mapeamento de endpoints da API
- Tratamento de respostas

### `composables/useOffers.ts`
- Gerenciamento de estado reativo
- Lógica de negócio
- Integração entre UI e serviços

### `components/`
- Componentes reutilizáveis e independentes
- Recebem props e emitem eventos

### `views/OffersView.vue`
- Página principal
- Orquestra componentes e composables
- Gerencia ações do usuário

## ⚙️ Configuração

A URL da API pode ser configurada através da variável de ambiente:
- `VITE_API_BASE_URL` (padrão: `http://localhost:8080`)

## 📝 Próximos Passos

A estrutura está pronta para ser expandida. Você pode:
- Adicionar mais componentes conforme necessário
- Criar novas views para outras funcionalidades
- Adicionar mais composables para lógica específica
- Expandir os serviços conforme a API cresce




