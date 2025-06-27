using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SQLiteConnectivity.Repository;
using Lib.Repository.Repository;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;

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

// Add detailed environment detection logging
logger.LogInformation($"=== ENVIRONMENT DETECTION DEBUG ===");
logger.LogInformation($"Environment.EnvironmentName: {environment.EnvironmentName}");
logger.LogInformation($"Environment.IsProduction(): {environment.IsProduction()}");
logger.LogInformation($"Environment.IsDevelopment(): {environment.IsDevelopment()}");
logger.LogInformation($"Environment.IsStaging(): {environment.IsStaging()}");
logger.LogInformation($"ASPNETCORE_ENVIRONMENT env var: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
logger.LogInformation($"DOTNET_ENVIRONMENT env var: {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}");
logger.LogInformation($"WEBSITE_SITE_NAME env var: {Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")}");
logger.LogInformation($"WEBSITE_HOSTNAME env var: {Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}");

// Force environment detection from environment variables
var forcedEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? 
                       Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? 
                       "Development";
logger.LogInformation($"Forced environment detection: {forcedEnvironment}");

// Additional environment checks
logger.LogInformation($"Environment.GetEnvironmentVariable('ASPNETCORE_ENVIRONMENT'): {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
logger.LogInformation($"Environment.GetEnvironmentVariable('DOTNET_ENVIRONMENT'): {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}");
logger.LogInformation($"Environment.GetEnvironmentVariable('WEBSITE_SITE_NAME'): {Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")}");

// Check if we're running in Azure
var isAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
logger.LogInformation($"Running in Azure: {isAzure}");

// Force the environment to be Production if we're in Azure and the variable is set
if (isAzure && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Production", StringComparison.OrdinalIgnoreCase) == true)
{
    logger.LogInformation("Forcing environment to Production for Azure deployment");
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
}

logger.LogInformation($"=== END ENVIRONMENT DETECTION DEBUG ===");

// Log all environment variables (be careful with sensitive data in production)
/*if (!environment.IsProduction())
{
    logger.LogInformation("Environment Variables:" + 
        string.Join(Environment.NewLine, 
            Environment.GetEnvironmentVariables()
                .Cast<System.Collections.DictionaryEntry>()
                .Select(e => $"{e.Key} = {e.Value}")));
}
*/
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
            //logger.LogInformation($"Updated connection string for {key}");

            logger.LogInformation($"New connection string for {key}: {newConnectionString}");
        }
    }
}
logger.LogInformation("Connection strings processed");

// Add after reading connection strings and before registering repositories
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "SQLite";
logger.LogInformation($"DatabaseProvider from config: {databaseProvider}");

// Get the appropriate connection string based on environment and provider
string connectionString;
if (databaseProvider.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
{
    // Force production mode when running in Azure
    var isAzureForOracle = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
    var aspNetCoreEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
    
    // Use production if we're in Azure OR if environment variables are set to Production
    var isProduction = isAzureForOracle || 
                      (aspNetCoreEnv?.Equals("Production", StringComparison.OrdinalIgnoreCase) == true) ||
                      (dotnetEnv?.Equals("Production", StringComparison.OrdinalIgnoreCase) == true);
    
    logger.LogInformation($"Environment detection for Oracle:");
    logger.LogInformation($"  - Is Azure: {isAzureForOracle}");
    logger.LogInformation($"  - ASPNETCORE_ENVIRONMENT: {aspNetCoreEnv}");
    logger.LogInformation($"  - DOTNET_ENVIRONMENT: {dotnetEnv}");
    logger.LogInformation($"  - Final isProduction decision: {isProduction}");
    
    var connectionStringName = isProduction ? "OracleConnection_Production" : "OracleConnection_Development";
    connectionString = builder.Configuration.GetConnectionString(connectionStringName) ?? throw new InvalidOperationException($"Oracle connection string '{connectionStringName}' is not configured.");
    logger.LogInformation($"Using Oracle connection string: {connectionStringName}");
}
else
{
    // Force production mode when running in Azure for SQLite too
    var isAzureForSqlite = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
    var aspNetCoreEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
    
    // Use production if we're in Azure OR if environment variables are set to Production
    var isProduction = isAzureForSqlite || 
                      (aspNetCoreEnv?.Equals("Production", StringComparison.OrdinalIgnoreCase) == true) ||
                      (dotnetEnv?.Equals("Production", StringComparison.OrdinalIgnoreCase) == true);
    
    logger.LogInformation($"Environment detection for SQLite:");
    logger.LogInformation($"  - Is Azure: {isAzureForSqlite}");
    logger.LogInformation($"  - ASPNETCORE_ENVIRONMENT: {aspNetCoreEnv}");
    logger.LogInformation($"  - DOTNET_ENVIRONMENT: {dotnetEnv}");
    logger.LogInformation($"  - Final isProduction decision: {isProduction}");
    
    var connectionStringName = isProduction ? "SQLiteConnection_Production" : "SQLiteConnection_Development";
    connectionString = builder.Configuration.GetConnectionString(connectionStringName) ?? throw new InvalidOperationException($"SQLite connection string '{connectionStringName}' is not configured.");
    logger.LogInformation($"Using SQLite connection string: {connectionStringName}");
    // Log database info
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
    // Initialize database with tables and sample data
    await InitializeDatabaseAsync(connectionString, logger);
}


// Register CQRS Repositories
try
{
    logger.LogInformation("Registering Command Repository");
    logger.LogInformation("Connection string: " + connectionString);
    if (databaseProvider.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
    {
        // Register Oracle repositories
        builder.Services.AddScoped<Lib.Repository.Repository.Commands.IPositionCommandRepository>(_ =>
        {
            var repo = new OracleConnectivity.Repository.Commands.PositionCommandRepository(connectionString);
            logger.LogInformation("Oracle PositionCommandRepository initialized successfully");
            return repo;
        });
        builder.Services.AddScoped<Lib.Repository.Repository.Queries.IPositionQueryRepository>(_ =>
        {
            var repo = new OracleConnectivity.Repository.Queries.PositionQueryRepository(connectionString);
            logger.LogInformation("Oracle PositionQueryRepository initialized successfully");
            return repo;
        });
    }
    else
    {
        // Register SQLite repositories (default)
        builder.Services.AddScoped<Lib.Repository.Repository.Commands.IPositionCommandRepository>(_ =>
        {
            var repo = new SQLiteConnectivity.Repository.Commands.PositionCommandRepository(connectionString);
            logger.LogInformation("SQLite PositionCommandRepository initialized successfully");
            return repo;
        });
        builder.Services.AddScoped<Lib.Repository.Repository.Queries.IPositionQueryRepository>(_ =>
        {
            var repo = new SQLiteConnectivity.Repository.Queries.PositionQueryRepository(connectionString);
            logger.LogInformation("SQLite PositionQueryRepository initialized successfully");
            return repo;
        });
    }
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

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:53614",  // React dev server
                "http://localhost:3000",   // Alternative React port
                "http://localhost:5173",   // Vite default port
                "http://127.0.0.1:53614",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:5173"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddAuthorization();

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
            context.Response.Headers.AccessControlAllowOrigin = origin;
            context.Response.Headers.AccessControlAllowMethods = "GET, POST, PUT, DELETE, OPTIONS";
            context.Response.Headers.AccessControlAllowHeaders = "Content-Type, Authorization, Accept, Cache-Control, Pragma";
            context.Response.Headers.AccessControlAllowCredentials = "true";
            context.Response.Headers.AccessControlMaxAge = "86400";
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

// Database initialization method
static async Task InitializeDatabaseAsync(string connectionString, ILogger logger)
{
    try
    {
        // Extract database path from connection string
        var dbPath = connectionString.Split('=')[1].Split(';')[0];
        
        // Solo si la base de datos NO existe, la creamos y ejecutamos el script
        if (!File.Exists(dbPath))
        {
            logger.LogInformation($"Creating database file: {dbPath}");
            File.Create(dbPath).Close();

            // Find the SQL script file
            var scriptPath = FindScriptFile();
            if (scriptPath == null)
            {
                logger.LogWarning("SQL initialization script not found. Database will be empty.");
                return;
            }

            logger.LogInformation($"Reading SQL script from: {scriptPath}");
            string sqlScript = await File.ReadAllTextAsync(scriptPath);
            
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                logger.LogInformation("Executing database initialization script...");
                
                // Execute the entire script as one command
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlScript;
                    await command.ExecuteNonQueryAsync();
                }
                
                logger.LogInformation("Database initialization completed successfully!");
            }
        }
        else
        {
            logger.LogInformation("Database file already exists. Initialization script will NOT be run.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error initializing database");
        throw;
    }
}

// Helper method to find the SQL script file
static string? FindScriptFile()
{
    var scriptFileName = "InitializeDatabase.sql";
    
    // Try multiple possible locations for the script file
    var possiblePaths = new[]
    {
        scriptFileName, // Current directory
        Path.Combine("Scripts", scriptFileName), // Scripts subdirectory
        Path.Combine(Directory.GetCurrentDirectory(), scriptFileName), // Full path in current directory
        Path.Combine(Directory.GetCurrentDirectory(), "Scripts", scriptFileName), // Full path in Scripts subdirectory
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, scriptFileName), // Output directory
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", scriptFileName), // Output directory Scripts subdirectory
        Path.Combine("..", "SQLiteConnectivity", "Scripts", scriptFileName), // Relative to SQLiteConnectivity project
        Path.Combine("..", "..", "SQLiteConnectivity", "Scripts", scriptFileName) // Two levels up
    };

    foreach (var path in possiblePaths)
    {
        if (File.Exists(path))
        {
            return path;
        }
    }

    return null;
}
