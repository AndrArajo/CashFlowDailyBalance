FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar arquivos de configuração e solução
COPY *.sln .

# Copiar apenas os projetos necessários para a aplicação principal
COPY CashFlowDailyBalance.API/*.csproj ./CashFlowDailyBalance.API/
COPY CashFlowDailyBalance.Application/*.csproj ./CashFlowDailyBalance.Application/
COPY CashFlowDailyBalance.Domain/*.csproj ./CashFlowDailyBalance.Domain/
COPY CashFlowDailyBalance.Infra.Data/*.csproj ./CashFlowDailyBalance.Infra.Data/
COPY CashFlowDailyBalance.Infra.IoC/*.csproj ./CashFlowDailyBalance.Infra.IoC/
COPY CashFlowDailyBalance.Infra.CrossCutting/*.csproj ./CashFlowDailyBalance.Infra.CrossCutting/
COPY CashFlowDailyBalance.Infra.External/*.csproj ./CashFlowDailyBalance.Infra.External/

# Restaurar pacotes apenas para a aplicação principal
RUN dotnet restore CashFlowDailyBalance.API/CashFlowDailyBalance.API.csproj

# Copiar apenas o código fonte das aplicações de produção (ignorando arquivos de teste)
COPY CashFlowDailyBalance.API/. ./CashFlowDailyBalance.API/
COPY CashFlowDailyBalance.Application/. ./CashFlowDailyBalance.Application/
COPY CashFlowDailyBalance.Domain/. ./CashFlowDailyBalance.Domain/
COPY CashFlowDailyBalance.Infra.Data/. ./CashFlowDailyBalance.Infra.Data/
COPY CashFlowDailyBalance.Infra.IoC/. ./CashFlowDailyBalance.Infra.IoC/
COPY CashFlowDailyBalance.Infra.CrossCutting/. ./CashFlowDailyBalance.Infra.CrossCutting/
COPY CashFlowDailyBalance.Infra.External/. ./CashFlowDailyBalance.Infra.External/

# Publicar a API
RUN dotnet publish CashFlowDailyBalance.API/CashFlowDailyBalance.API.csproj -c Release -o /app/publish/api

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish/api ./api

# Configurar variáveis de ambiente para o contêiner
ENV ASPNETCORE_URLS=http://+:5000

# Expor a porta da API
EXPOSE 5000

# Iniciar a aplicação
ENTRYPOINT ["dotnet", "api/CashFlowDailyBalance.API.dll"]
