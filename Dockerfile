FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar arquivos de configuração e solução
COPY *.sln .
COPY CashFlowDailyBalance.API/*.csproj ./CashFlowDailyBalance.API/
COPY CashFlowDailyBalance.Application/*.csproj ./CashFlowDailyBalance.Application/
COPY CashFlowDailyBalance.Domain/*.csproj ./CashFlowDailyBalance.Domain/
COPY CashFlowDailyBalance.Infra.Data/*.csproj ./CashFlowDailyBalance.Infra.Data/
COPY CashFlowDailyBalance.Infra.IoC/*.csproj ./CashFlowDailyBalance.Infra.IoC/

# Restaurar pacotes
RUN dotnet restore

# Copiar todo o código fonte
COPY . .

# Publicar a API (que inclui o serviço background de Schedule)
RUN dotnet publish CashFlowDailyBalance.API/CashFlowDailyBalance.API.csproj -c Release -o /app/publish/api


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish/api ./api

# Configurar variáveis de ambiente para o contêiner
ENV ASPNETCORE_URLS=http://+:5000

# Expor a porta da API
EXPOSE 5000

# Iniciar a aplicação (que executará tanto a API quanto o Schedule)
ENTRYPOINT ["dotnet", "api/CashFlowDailyBalance.API.dll"]
