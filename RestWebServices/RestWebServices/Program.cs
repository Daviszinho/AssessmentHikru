var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<Lib.Repository.Repository.PositionRepository>();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS
var allowedOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowedOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS - Must be before UseAuthorization and MapControllers
app.UseCors(allowedOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
