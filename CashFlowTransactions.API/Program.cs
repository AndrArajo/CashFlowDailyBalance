using CashFlowDailyBalance.Infra.Data.Context;
using CashFlowDailyBalance.Infra.IoC;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DotNetEnv;

// Carregar variáveis do arquivo .env se existir (para desenvolvimento local)
if (File.Exists(".env"))
{
    Env.Load();
}
else if (File.Exists("../.env"))
{
    Env.Load("../.env");
}

var builder = WebApplication.CreateBuilder(args);

// Configurar a ordem de prioridade de configuração
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables() // As variáveis de ambiente tem prioridade
    .AddUserSecrets<Program>(optional: true);

// Configuração do banco de dados usando variáveis de ambiente
var dbHost = builder.Configuration["DB_HOST"] ?? Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = builder.Configuration["DB_PORT"] ?? Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var dbName = builder.Configuration["POSTGRES_DB"] ?? Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "cashflow";
var dbUser = builder.Configuration["POSTGRES_USER"] ?? Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
var dbPassword = builder.Configuration["POSTGRES_PASSWORD"] ?? Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";

// Configuração da string de conexão
var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "CashFlow Daily Balance API", 
        Version = "v1",
        Description = "API para gerenciamento de balanço diário de fluxo de caixa"
    });
});

// Registrar serviços da aplicação
builder.Services.AddInfrastructure(builder.Configuration);

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", 
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Aplicar migrações automaticamente ao iniciar a aplicação
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("Aplicando migrações ao banco de dados...");
        dbContext.Database.Migrate();
        Console.WriteLine("Migrações aplicadas com sucesso!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao aplicar migrações: {ex.Message}");
    // Não interromper a aplicação caso ocorra um erro nas migrações
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CashFlow Daily Balance API v1"));
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
