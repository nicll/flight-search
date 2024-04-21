using FlightDatabaseImportService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextPool<FlightDbContext>(opts => opts.UseNpgsql("Host=localhost;Port=5432;Database=flights;Pooling=true"));

builder.Services.AddHostedService<FlightDatabaseImportService.Workers.AircraftStaticDataImportWorker>();
builder.Services.AddHostedService<FlightDatabaseImportService.Workers.AircraftDynamicDataImportWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
