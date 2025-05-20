# CashFlow Daily Balance API

API para gerenciamento de fluxo de caixa e cálculo de balanço diário, desenvolvida em .NET 8.0 com arquitetura limpa.

## Objetivo do Projeto

O CashFlow Daily Balance tem como objetivo principal automatizar o registro e o cálculo de balanços diários a partir de transações financeiras. A aplicação:

- Registra transações financeiras (créditos e débitos) com data, valor e descrição
- Calcula automaticamente o balanço diário com base nessas transações
- Atualiza os balanços diários de duas formas:
  - **Automaticamente**: através de um serviço agendado que é executado a cada 1 hora, consolidando transações recentes
  - **Manualmente**: permite atualizar pontualmente o balanço de uma data específica ou de um período
- Disponibiliza consultas para análise financeira por períodos e datas específicas
- Utiliza sistema de cache em múltiplas camadas para otimizar a performance

## CI/CD

O projeto utiliza GitHub Actions para integração contínua e entrega contínua:

- **Integração Contínua**: Testes automatizados são executados em cada push e pull request
- **Entrega Contínua**: Builds bem-sucedidos na branch main são automaticamente publicados no DockerHub
- **Imagem Docker**: Disponível em andrarajo/cashflow-dailybalance

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

A API estará disponível em: http://localhost:5001/swagger

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
- Serviço agendado para atualização a cada 1 hora dos balanços
- Endpoint para processamento manual de balanços de datas específicas
- Consulta de balanços por data ou período
- Cache em múltiplas camadas para maior performance (memória e Redis)
- Gerenciamento eficiente de conexões com banco de dados

## Fluxo de Processamento

1. Transações financeiras são registradas no sistema com data, tipo (crédito/débito), valor e descrição
2. O balanço diário pode ser calculado de duas formas:
   - **Automaticamente**: A cada 1 hora, o serviço agendado (`DailyBalanceSchedulerService`) processa os balanços pendentes
   - **Manualmente**: Através da API é possível solicitar o cálculo de balanço para uma data específica ou período
3. O cálculo do balanço considera:
   - Saldo do dia anterior (como saldo inicial)
   - Soma de todas as transações de crédito do dia
   - Soma de todas as transações de débito do dia
   - Cálculo do saldo final (saldo anterior + créditos - débitos)
4. Os resultados são armazenados no banco de dados e também em cache para consultas rápidas

## Dependências Principais

- ASP.NET Core 8.0
- Entity Framework Core 8.0
- PostgreSQL (via Npgsql)
- Redis (via StackExchange.Redis)
- Swagger/OpenAPI
- xUnit (para testes)

## Estrutura da Aplicação

A aplicação segue os princípios de Clean Architecture:

- **CashFlowDailyBalance.Domain**: 
  - Entidades e regras de negócio
  - Interfaces de repositórios
  - Enums e definições de domínio

- **CashFlowDailyBalance.Application**: 
  - Serviços de aplicação
  - DTOs e interfaces
  - Mapeamentos
  - Serviços agendados

- **CashFlowDailyBalance.Infra.Data**: 
  - Implementações de repositórios
  - Contexto de banco de dados 
  - Configurações de entidades
  - Migrações

- **CashFlowDailyBalance.Infra.CrossCutting**: 
  - Serviços transversais
  - Cache
  - Logging
  - Utilidades

- **CashFlowDailyBalance.Infra.IoC**: 
  - Configuração de injeção de dependências
  - Registro de serviços

- **CashFlowDailyBalance.API**: 
  - Controllers
  - Configuração da aplicação
  - Middlewares
  - Filtros

- **CashFlowDailyBalance.*.Tests**: 
  - Testes unitários
  - Mocks e fixtures 