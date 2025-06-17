var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<Lib.Repository.Repository.PositionRepository>();
// Add controllers and API explorer
builder.Services.AddControllers();

// Add OpenAPI support
builder.Services.AddOpenApi();

// Configure CORS - Allow all for development
Console.WriteLine("Configuring CORS to allow all origins for development");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()
               .WithExposedHeaders("*");
    });
    
    Console.WriteLine("CORS policy 'AllowAll' has been configured");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// IMPORTANT: The order of middleware is critical here
app.UseRouting();

// Enable CORS with the AllowAll policy
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Log successful configuration
Console.WriteLine("CORS middleware has been configured with AllowAll policy");

app.Run();
