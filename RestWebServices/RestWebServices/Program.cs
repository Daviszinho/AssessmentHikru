var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<Lib.Repository.Repository.PositionRepository>();
// Add controllers and API explorer
builder.Services.AddControllers();

// Add OpenAPI support
builder.Services.AddOpenApi();

// Configure CORS with specific origins
var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:53614";
Console.WriteLine($"Configuring CORS to allow origin: {frontendUrl}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins(frontendUrl)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .WithExposedHeaders("*");
    });
    
    Console.WriteLine("CORS policy 'AllowFrontend' has been configured");
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

// Enable CORS with the AllowFrontend policy
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Log successful configuration
Console.WriteLine("CORS middleware has been configured with AllowAll policy");

app.Run();
