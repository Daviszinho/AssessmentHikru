var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;

// Configure Oracle wallet settings
var oracleSettings = configuration.GetSection("OracleSettings");

// In Azure, the wallet is deployed to /home/site/wwwroot/wallet
var walletLocation = "/home/site/wwwroot/wallet";
var tnsAdmin = walletLocation;

// Set environment variables
Environment.SetEnvironmentVariable("TNS_ADMIN", walletLocation);
Environment.SetEnvironmentVariable("ORACLE_HOME", "/usr/lib/oracle/21/client64");
Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", "/usr/lib/oracle/21/client64/lib");

// Log the environment variables for debugging
Console.WriteLine($"=== Application Startup ===");
Console.WriteLine($"TNS_ADMIN set to: {Environment.GetEnvironmentVariable("TNS_ADMIN")}");
Console.WriteLine($"ORACLE_HOME set to: {Environment.GetEnvironmentVariable("ORACLE_HOME")}");
Console.WriteLine($"LD_LIBRARY_PATH set to: {Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")}");
Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
Console.WriteLine($"Wallet Location: {walletLocation}");

// Enhanced logging for wallet files
Console.WriteLine("\n=== Checking Wallet Files ===");
CheckAndLogFile(walletLocation, "cwallet.sso");
CheckAndLogFile(walletLocation, "ewallet.p12");
CheckAndLogFile(walletLocation, "ewallet.pem");
CheckAndLogFile(walletLocation, "keystore.jks");
CheckAndLogFile(walletLocation, "ojdbc.properties");
CheckAndLogFile(walletLocation, "sqlnet.ora");
CheckAndLogFile(walletLocation, "tnsnames.ora");
CheckAndLogFile(walletLocation, "truststore.jks");

// Log the contents of the wallet directory
LogDirectoryContents(walletLocation, "Wallet directory");

// Log the contents of sqlnet.ora and tnsnames.ora
LogFileContents(Path.Combine(walletLocation, "sqlnet.ora"), "sqlnet.ora");
LogFileContents(Path.Combine(walletLocation, "tnsnames.ora"), "tnsnames.ora");

// Log environment variables
Console.WriteLine("\n=== Environment Variables ===");
foreach (var envVar in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>().OrderBy(e => e.Key))
{
    Console.WriteLine($"{envVar.Key} = {envVar.Value}");
}

// Helper method to check and log file existence and size
void CheckAndLogFile(string directory, string fileName)
{
    var filePath = Path.Combine(directory, fileName);
    var exists = File.Exists(filePath);
    Console.WriteLine($"{fileName}: {(exists ? "Found" : "NOT FOUND")}");
    
    if (exists)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            Console.WriteLine($"  Size: {fileInfo.Length} bytes");
            Console.WriteLine($"  Last Modified: {fileInfo.LastWriteTimeUtc} (UTC)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error getting file info: {ex.Message}");
        }
    }
}

// Helper method to log file contents
void LogFileContents(string filePath, string description)
{
    Console.WriteLine($"\n=== {description} ===");
    Console.WriteLine($"File: {filePath}");
    
    if (!File.Exists(filePath))
    {
        Console.WriteLine("File does not exist");
        return;
    }
    
    try
    {
        var content = File.ReadAllText(filePath);
        Console.WriteLine(content);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error reading file: {ex.Message}");
    }
}

// Helper method to log directory contents
void LogDirectoryContents(string directory, string description)
{
    Console.WriteLine($"\n=== {description} ===");
    Console.WriteLine($"Directory: {directory}");
    
    if (!Directory.Exists(directory))
    {
        Console.WriteLine("Directory does not exist");
        return;
    }
    
    try
    {
        var files = Directory.GetFiles(directory);
        Console.WriteLine($"Found {files.Length} files in directory:");
        foreach (var file in files)
        {
            Console.WriteLine($"- {Path.GetFileName(file)}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error listing wallet files: {ex.Message}");
    }
}

// Check if wallet directory exists
if (!Directory.Exists(walletLocation))
{
    Console.WriteLine($"Warning: Wallet directory not found at: {walletLocation}");
}

// Ensure we have the connection string in production
var connectionString = configuration.GetConnectionString("OracleConnection");
if (string.IsNullOrEmpty(connectionString) && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException("Oracle connection string is not configured in appsettings.json");
}

// Add services with dependency injection
builder.Services.AddScoped<Lib.Repository.Repository.PositionRepository>(provider => 
    new Lib.Repository.Repository.PositionRepository(
        configuration,
        provider.GetService<IHostEnvironment>()));

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
