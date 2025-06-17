var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<Lib.Repository.Repository.PositionRepository>();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS - For development, allow all origins
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.AllowAnyOrigin()
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
app.UseCors(); // This will use the default policy we defined above

app.UseAuthorization();

app.MapControllers();

app.Run();
