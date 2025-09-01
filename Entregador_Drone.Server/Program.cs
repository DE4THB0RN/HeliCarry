using Entregador_Drone.Server.Serviços;
using Entregador_Drone.Server.Serviços.Interface;
using Entregador_Drone.Server.Serviços.ProjetoDroneDelivery.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("appConnString");


builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddScoped<IPathfindingService, AStarService>();
builder.Services.AddScoped<DistanciaService>();
builder.Services.AddScoped<GreedyPlanner>();
builder.Services.AddHostedService<AutomaticAssignmentService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        logger.LogInformation("Tentando conectar ao banco de dados usando CanConnectAsync...");
        var canConnect = await dbContext.Database.CanConnectAsync();

        if (canConnect)
        {
            logger.LogInformation("Conexão com o banco de dados bem-sucedida (via CanConnectAsync).");
        }
        else
        {
            logger.LogError("Falha ao conectar ao banco de dados (via CanConnectAsync).");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocorreu um erro ao tentar conectar ao banco de dados (via CanConnectAsync).");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
