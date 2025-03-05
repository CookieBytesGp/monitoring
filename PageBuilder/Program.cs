using PageBuilder.Services.ToolService;
using Persistence.Tools;
using Persistence;
using Microsoft.EntityFrameworkCore;
using PageBuilder.Services.PageService;
using Persistence.Page;
using System.Net;
using Microsoft.Extensions.Options;
using Persistence.Tool;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Transient);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IToolRepository, ToolRepository>();
builder.Services.AddScoped<IToolService, ToolService>();
builder.Services.AddScoped<IPageRepository, PageRepository>();
builder.Services.AddScoped<IPageService, PageService>();
// Configure Kestrel server options
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 5002); // Set the desired port here
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost44157", policy =>
    {
        policy.WithOrigins("http://localhost:44157")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowLocalhost44157");
app.UseAuthorization();

app.MapControllers();

app.Run();
