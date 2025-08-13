using Monitoring.Application.Interfaces.Camera;
using Monitoring.Application.Services.Camera;
using Monitoring.Infrastructure.Persistence;
using Monitoring.Infrastructure.Repositories.Camera;
using Monitoring.Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Monitoring.Infrastructure.Camera;
using Monitoring.Application.Mappings;
using Monitoring.Application.Interfaces.Realtime;
using Monitoring.ApiGateway.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add HttpClient
builder.Services.AddHttpClient();

// Add repositories
builder.Services.AddScoped<ICameraRepository, CameraRepository>();
builder.Services.AddScoped<Monitoring.Infrastructure.Persistence.IUnitOfWork, Monitoring.Infrastructure.Persistence.UnitOfWork>();

// Add camera strategies (Infrastructure layer)
builder.Services.AddScoped<Monitoring.Infrastructure.Camera.CameraStrategyFactory>();
builder.Services.AddScoped<CameraStrategySelector>();

// Add domain strategy factory
builder.Services.AddScoped<Monitoring.Domain.Services.Camera.ICameraStrategyFactory, Monitoring.Infrastructure.Camera.CameraStrategyFactory>();

// Add notifications
builder.Services.AddScoped<ICameraNotifications, ApiCameraNotifications>();

// Add services
builder.Services.AddScoped<ICameraService, CameraService>();
builder.Services.AddScoped<Monitoring.Application.Interfaces.Camera.ICameraStreamService, Monitoring.Application.Services.Camera.CameraStreamService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Remove HTTPS redirection for development
// app.UseHttpsRedirection();

app.UseStaticFiles(); // Enable static files

app.UseAuthorization();

app.MapControllers();

app.Run();
