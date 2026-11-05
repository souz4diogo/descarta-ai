using DescartaAi.Api.Dados;
using DescartaAi.Api;
using DescartaAi.Api.Modulos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default"), n => n.UseNetTopologySuite())
     .UseSnakeCaseNamingConvention());

var origens = builder.Configuration["Cors:Origens"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddExceptionHandler<ErroDeRequisicaoHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.SwaggerDoc("v1", new() { Title = "Descarta Aí API", Version = "v1" }));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Seed:Executar"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await Seed.ExecutarAsync(db);
}

var v1 = app.MapGroup("/v1");
v1.MapMateriais();
v1.MapPontos();

app.Run();
