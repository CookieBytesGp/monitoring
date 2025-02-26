using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.User;
using UserSerivce.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var offlineDbPath = Path.Combine(Directory.GetCurrentDirectory(), "monitorDB.db");

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlite($"Data Source={offlineDbPath}",
        b => b.MigrationsAssembly("Persistence")), ServiceLifetime.Transient);

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
