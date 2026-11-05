# Descarta Aí

Plataforma web e mobile de descarte consciente: mostra como descartar cada tipo de resíduo e indica o ponto de coleta mais próximo em Araçatuba e região.

Projeto de extensão (UNIP, Análise e Desenvolvimento de Sistemas), desenvolvido pela NSI (Nexus Software Intelligence).

## Funcionalidades

- **Guia de descarte**: busca por item (sem precisar de acento) e filtro por tipo de resíduo, com instruções, cuidados e aviso de resíduo perigoso.
- **Mapa de pontos de coleta**: busca por proximidade (raio de 2 a 50 km), filtro por material, uso opcional da localização do usuário e link "Como chegar" no Google Maps.
- **Páginas institucionais**: Sobre, Política de Privacidade (LGPD) e Termos de Uso.
- Interface acessível: navegação por teclado, foco visível, rótulos em todos os campos, áreas de toque de 44 px e suporte a leitores de tela.

Em desenvolvimento: app mobile, identificação de resíduos por foto e cadastro de pontos por parceiros.

## Estrutura

| Pasta | Conteúdo |
|---|---|
| `api/` | API REST em ASP.NET Core (.NET 10), EF Core, PostgreSQL + PostGIS |
| `web/` | Site em React + Vite + TypeScript, mapa com Leaflet e OpenStreetMap |
| `mobile/` | App em React Native + Expo (próxima fase) |

## Como rodar localmente

Pré-requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download), [Node.js 20+](https://nodejs.org) e [Docker](https://www.docker.com).

```bash
# 1. Banco de dados (PostgreSQL 16 + PostGIS)
docker compose up -d

# 2. API — aplica as migrações e a carga inicial ao iniciar
cd api/DescartaAi.Api
dotnet run
# Swagger: http://localhost:5158/swagger

# 3. Site (em outro terminal)
cd web
cp .env.example .env    # ajuste as variáveis se precisar
npm install
npm run dev
# http://localhost:5173
```

### Variáveis do site (`web/.env`)

| Variável | Descrição |
|---|---|
| `VITE_API_URL` | Endereço da API (padrão `http://localhost:5158/v1`) |
| `VITE_CONTATO_EMAIL` | E-mail de contato exibido em Sobre, Privacidade e Termos |
| `VITE_NSI_CNPJ` | CNPJ do responsável pelo site |

### Configuração da API em produção

Veja `.env.example` na raiz. Pontos principais:

| Variável | Descrição |
|---|---|
| `ConnectionStrings__Default` | Conexão com o PostgreSQL (use usuário e senha próprios, não `postgres`) |
| `Cors__Origens` | Domínios do site autorizados a chamar a API |
| `Banco__MigrarAoIniciar` | `true` para aplicar migrações e carga inicial ao subir |
| `Swagger__Habilitado` | Expõe `/swagger` fora do ambiente de desenvolvimento (padrão: desligado) |
| `Limite__RequisicoesPorMinuto` | Limite por IP (padrão 120); acima disso a API responde 429 |
| `Proxy__Confiaveis__0` | IP do proxy reverso, para o limite usar o IP real do usuário |

## Organização do código

```
api/DescartaAi.Api/
├── Dominio/          entidades e enums
├── Dados/            DbContext, migrações e carga inicial (catálogo + pontos do OSM)
├── Modulos/
│   ├── Materiais/    endpoints, serviço e DTOs do guia
│   └── Pontos/       endpoints, serviço, DTOs e validação da busca por proximidade
└── Infraestrutura/   segurança (CORS, limite, cabeçalhos, HTTPS) e tratamento de erros

web/src/
├── paginas/          uma página por rota
├── componentes/      peças reutilizáveis (avisos, esqueletos de carregamento)
├── api.ts            cliente da API com cancelamento de requisições
└── util.ts           formatação e hook useConsulta
```

## API

Base: `/v1`. Documentação interativa em `/swagger` (em desenvolvimento).

| Método | Rota | Descrição |
|---|---|---|
| GET | `/categorias` | Tipos de resíduo |
| GET | `/materiais?busca=&categoriaId=` | Lista e busca de materiais |
| GET | `/materiais/{id}` | Instruções e cuidados de descarte |
| GET | `/pontos/proximos?lat=&lng=&raio=&materialId=` | Pontos aprovados por distância (raio em metros, máx. 50 000) |
| GET | `/pontos/{id}` | Detalhe de um ponto |

Erros seguem o padrão RFC 7807 (`application/problem+json`).

## Dados

Os pontos de coleta iniciais vêm do OpenStreetMap (© colaboradores do OpenStreetMap, licença [ODbL](https://opendatacommons.org/licenses/odbl/)). Correções e novos pontos são bem-vindos pelo contato informado no site.

## Licença

Veja [LICENSE](LICENSE).
