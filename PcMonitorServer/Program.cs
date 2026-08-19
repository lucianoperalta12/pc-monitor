using Microsoft.EntityFrameworkCore;
using PcMonitorServer.Data;
using PcMonitorServer.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// CORS permisivo solo para el frontend propio
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.WithOrigins("*").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();

app.Run();
