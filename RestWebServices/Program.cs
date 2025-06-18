using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SQLiteConnectivity.Repository;
using Lib.Repository.Repository;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure CORS to allow requests from the React frontend
var reactAppUrl = builder.Configuration["ReactAppUrl"] ?? "http://localhost:3000";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins(reactAppUrl)
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});

// Get connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("SQLiteConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("SQLite connection string is not configured.");
}

// Register PositionRepository with SQLite
builder.Services.AddScoped<IPositionRepository>(_ => new PositionRepository(connectionString));

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hikru Assessment API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS with the configured policy
app.UseCors("AllowReactApp");

app.UseAuthorization();
app.MapControllers();

app.Run();
