using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SQLiteConnectivity.Repository;
using Lib.Repository.Repository;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Get environment and logger
var environment = builder.Environment;
var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
    config.AddDebug();
}).CreateLogger("ProgramStartup");

// Log all environment variables (be careful with sensitive data in production)
if (!environment.IsProduction())
{
    logger.LogInformation("Environment Variables:" + 
        string.Join(Environment.NewLine, 
            Environment.GetEnvironmentVariables()
                .Cast<System.Collections.DictionaryEntry>()
                .Select(e => $"{e.Key} = {e.Value}")));
}

logger.LogInformation($"Application starting in {environment.EnvironmentName} environment");
logger.LogInformation($"Content Root: {environment.ContentRootPath}");
logger.LogInformation($"Web Root: {environment.WebRootPath}");
logger.LogInformation($"Application Name: {environment.ApplicationName}");

// Handle App_Data directory and connection string
var contentRoot = builder.Environment.ContentRootPath;
var appDataPath = Path.Combine(contentRoot, "App_Data");

// Create App_Data directory if it doesn't exist
if (!Directory.Exists(appDataPath))
{
    Directory.CreateDirectory(appDataPath);
    logger.LogInformation($"Created App_Data directory at: {appDataPath}");
}

// Process connection strings to replace {App_Data} token
var connectionStrings = builder.Configuration.GetSection("ConnectionStrings").Get<Dictionary<string, string>>();
if (connectionStrings != null)
{
    foreach (var key in connectionStrings.Keys.ToList())
    {
        if (!string.IsNullOrEmpty(connectionStrings[key]))
        {
            var newConnectionString = connectionStrings[key].Replace("{App_Data}", appDataPath + Path.DirectorySeparatorChar);
            builder.Configuration.GetSection("ConnectionStrings")[key] = newConnectionString;
            logger.LogInformation($"Updated connection string for {key}");
        }
    }
}

// Log all configuration values (after processing)
var configValues = builder.Configuration.AsEnumerable()
    .Where(kvp => kvp.Value != null)
    .Select(kvp => $"{kvp.Key} = {kvp.Value}");

logger.LogInformation("Configuration Values: " + string.Join(", ", configValues));

// Add CORS policy with comprehensive settings
builder.Services.AddCors(options =>
{
    // Default policy that will be applied to all endpoints
    options.DefaultPolicyName = "AllowReactApp";
    
    // Single policy for all environments
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "https://happy-stone-0deafcf10.1.azurestaticapps.net",
                "http://localhost:3000",
                "http://localhost:5000",
                "http://localhost:5173",  // Vite default port
                "http://localhost:4173",  // Vite preview port
                "http://localhost:5246",  // Your local backend port
                "http://localhost:8080",  // Common dev port
                "http://127.0.0.1:3000",
                "http://127.0.0.1:5000",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:4173",
                "http://127.0.0.1:5246",  // Your local backend port
                "http://127.0.0.1:8080")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition")
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// Add services to the container.
builder.Services.AddControllers();

// Get the appropriate connection string based on environment
var isProduction = environment.IsProduction();
var connectionStringName = isProduction ? "SQLiteConnection_Production" : "SQLiteConnection_Development";
var connectionString = builder.Configuration.GetConnectionString(connectionStringName);

if (string.IsNullOrEmpty(connectionString))
{
    var errorMessage = $"SQLite connection string '{connectionStringName}' is not configured.";
    logger.LogError(errorMessage);
    throw new InvalidOperationException(errorMessage);
}

// Log database info
logger.LogInformation($"Using connection string: {connectionStringName}");
if (connectionString.Contains("Data Source="))
{
    var dbPath = connectionString.Split('=')[1].Split(';')[0];
    logger.LogInformation($"Database path: {dbPath}");
    
    if (File.Exists(dbPath))
    {
        logger.LogInformation("Database file exists");
    }
    else
    {
        logger.LogWarning("Database file does not exist at the specified path");
    }
}

// Register CQRS Repositories
try
{
    // Register Command Repository
    builder.Services.AddScoped<Lib.Repository.Repository.Commands.IPositionCommandRepository>(_ => 
    {
        var repo = new SQLiteConnectivity.Repository.Commands.PositionCommandRepository(connectionString);
        logger.LogInformation("PositionCommandRepository initialized successfully");
        return repo;
    });

    // Register Query Repository
    builder.Services.AddScoped<Lib.Repository.Repository.Queries.IPositionQueryRepository>(_ => 
    {
        var repo = new SQLiteConnectivity.Repository.Queries.PositionQueryRepository(connectionString);
        logger.LogInformation("PositionQueryRepository initialized successfully");
        return repo;
    });
}
catch (Exception ex)
{
    logger.LogError(ex, "Error initializing repositories");
    throw;
}

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hikru Assessment API", Version = "v1" });
});

// Build the app
var app = builder.Build();

// Enable detailed error pages in development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Log the connection string being used for verification
var productionConnectionString = builder.Configuration.GetConnectionString("SQLiteConnection_Production");
if (!string.IsNullOrEmpty(productionConnectionString))
{
    logger.LogInformation("Production connection string is configured");
    
    // Verify database file exists and is accessible
    if (productionConnectionString.Contains("Data Source="))
    {
        var dbPath = productionConnectionString.Split('=')[1].Split(';')[0];
        logger.LogInformation($"Production database path: {dbPath}");
        
        if (File.Exists(dbPath))
        {
            var fileInfo = new FileInfo(dbPath);
            logger.LogInformation($"Database file exists. Size: {fileInfo.Length} bytes. Last modified: {fileInfo.LastWriteTime}");
        }
        else
        {
            logger.LogWarning("Production database file does not exist at the specified path");
        }
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS with the configured policy
app.UseCors("AllowReactApp");

// Handle preflight requests
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        var origin = context.Request.Headers["Origin"].ToString();
        logger.LogInformation($"Preflight request from origin: {origin}");
        
        if (!string.IsNullOrEmpty(origin) && 
            (origin.Contains("happy-stone-0deafcf10.1.azurestaticapps.net") || 
             origin.Contains("localhost:") ||
             origin.Contains("127.0.0.1:")))
        {
            logger.LogInformation($"Allowing CORS for origin: {origin}");
            context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
            context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept, Cache-Control, Pragma");
            context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            context.Response.Headers.Add("Access-Control-Max-Age", "86400");
            context.Response.StatusCode = 204; // No Content
            return;
        }
        else
        {
            logger.LogWarning($"Blocking CORS for origin: {origin}");
        }
    }
    await next();
});

app.UseHttpsRedirection();

// Add error handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception occurred");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new 
        { 
            error = "An unexpected error occurred",
            message = ex.Message,
            details = environment.IsDevelopment() ? ex.StackTrace : null
        });
    }
});

app.UseAuthorization();
app.MapControllers();

// Log startup completion
logger.LogInformation("Application startup complete");

app.Run();
