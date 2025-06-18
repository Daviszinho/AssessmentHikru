var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;

// Log application startup
Console.WriteLine($"=== Application Startup ===");
Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");

// Get connection string from configuration
var connectionString = configuration.GetConnectionString("OracleConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Oracle database connection string is not configured in appsettings.json");
}

// Log the connection string (without password)
var safeConnectionString = connectionString.Replace(
    "Password=DavisOracle25!", 
    "Password=*****");
Console.WriteLine($"Using connection string: {safeConnectionString}");

// Log environment variables
Console.WriteLine("\n=== Environment Variables ===");
foreach (var envVar in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>().OrderBy(x => x.Key))
{
    Console.WriteLine($"{envVar.Key} = {envVar.Value}");
}

// Add services with dependency injection
builder.Services.AddScoped<Lib.Repository.Repository.PositionRepository>(provider => 
    new Lib.Repository.Repository.PositionRepository(
        configuration,
        provider.GetService<IHostEnvironment>() ?? throw new InvalidOperationException("IHostEnvironment service is not available")));

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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

// Build the app
var app = builder.Build();

// Initialize OracleInit with the environment and logging
app.Logger.LogInformation("Initializing OracleInit...");
try
{
    Hikru.Assessment.OracleConnectivity.OracleInit.Initialize(configuration, app.Environment);
    app.Logger.LogInformation("OracleInit initialized successfully");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to initialize OracleInit");
    throw;
}

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
