# CashFlow Daily Balance API

API para gerenciamento de fluxo de caixa e cálculo de balanço diário, desenvolvida em .NET 8.0 com arquitetura limpa.

## Executando com Docker

### Pré-requisitos
- Docker
- Docker Compose

### Configuração
1. Clone o repositório
2. Copie o arquivo `.env.example` para `.env`
   ```bash
   cp .env.example .env
   ```
3. Ajuste as variáveis de ambiente no arquivo `.env` conforme necessário:
   ```
   # Configurações do banco de dados
   DB_HOST=postgres
   DB_PORT=5432
   POSTGRES_DB=cashflow
   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=sua_senha_segura
   
   # Configurações do Redis
   REDIS_HOST=redis
   REDIS_PORT=6379
   ```

### Executando
Para iniciar a aplicação com Docker Compose:
```bash
docker-compose up -d
```

A API estará disponível em: http://localhost:8080/swagger

Para parar a aplicação:
```bash
docker-compose down
```

## Executando com dotnet

### Pré-requisitos
- .NET 8.0 SDK
- PostgreSQL
- Redis

### Configuração
1. Clone o repositório
2. Copie o arquivo `.env.example` para `.env`
   ```bash
   cp .env.example .env
   ```
3. Ajuste as variáveis de ambiente no arquivo `.env` para apontar para suas instâncias locais de PostgreSQL e Redis:
   ```
   # Configurações do banco de dados
   DB_HOST=localhost
   DB_PORT=5432
   POSTGRES_DB=cashflow
   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=sua_senha
   
   # Configurações do Redis
   REDIS_HOST=localhost
   REDIS_PORT=6379
   ```

### Restaurar pacotes e construir
```bash
dotnet restore
dotnet build
```

### Executar migrações do banco de dados
```bash
cd CashFlowDailyBalance.API
dotnet ef database update
```

### Executar a aplicação
```bash
cd CashFlowDailyBalance.API
dotnet run
```

A API estará disponível em: https://localhost:5001/swagger

## Principais Funcionalidades

- Registro de transações financeiras (créditos e débitos)
- Cálculo automático de balanço diário
- Consulta de balanços por data ou período
- Cache em múltiplas camadas para maior performance (memória e Redis)
- Gerenciamento eficiente de conexões com banco de dados

## Dependências Principais

- ASP.NET Core 8.0
- Entity Framework Core 8.0
- PostgreSQL (via Npgsql)
- Redis (via StackExchange.Redis)
- Swagger/OpenAPI
- xUnit (para testes)

## Estrutura da Aplicação

A aplicação segue os princípios de Clean Architecture:

- **Domain**: Entidades e regras de negócio
- **Application**: Serviços de aplicação, DTOs e interfaces
- **Infrastructure**: Implementações de repositórios, contexto de banco de dados e serviços transversais
- **API**: Controllers e configuração da aplicação 