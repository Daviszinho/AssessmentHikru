using Microsoft.AspNetCore.Mvc;
using Lib.Repository.Entities;
using Lib.Repository.Repository.Queries;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using Oracle.ManagedDataAccess.Client;

namespace RestWebServices.Controllers
{
    [Route("api/positions")]
    [ApiController]
    public class PositionsQueryController : ControllerBase
    {
        private readonly ILogger<PositionsQueryController> _logger;
        private readonly IPositionQueryRepository _queryRepository;
        private readonly IConfiguration _configuration;
        private string _timestamp => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]";

        public PositionsQueryController(
            ILogger<PositionsQueryController> logger,
            IPositionQueryRepository queryRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
            _configuration = configuration;
            _logger.LogInformation("PositionsQueryController initialized");
        }

        // GET: api/positions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Position>>> GetPositions()
        {
            try
            {
                _logger.LogInformation($"{_timestamp} [INFO] Getting all positions");
                //_logger.LogInformation("$[INFO] Getting all positions" + _queryRepository.GetConnectionString());
                var positions = await _queryRepository.GetAllPositionsAsync();
                _logger.LogInformation($"{_timestamp} [INFO] Retrieved {positions?.Count()} positions");
                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_timestamp} [ERROR] Error getting positions");
                return StatusCode(500, new { message = "An error occurred while retrieving positions", error = ex.Message });
            }
        }

        // GET: api/positions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Position>> GetPosition(int id)
        {
            _logger.LogInformation($"{_timestamp} [INFO] Getting position with ID: {id}");
            
            try
            {
                var position = await _queryRepository.GetPositionByIdAsync(id);
                if (position == null)
                {
                    _logger.LogWarning($"{_timestamp} [WARN] Position with ID {id} not found");
                    return NotFound();
                }

                _logger.LogInformation($"{_timestamp} [INFO] Retrieved position with ID: {id}");
                return Ok(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_timestamp} [ERROR] Error getting position with ID: {id}");
                return StatusCode(500, new { message = $"An error occurred while retrieving position with ID {id}", error = ex.Message });
            }
        }

        // GET: api/positions/environment-test
        [HttpGet("environment-test")]
        public IActionResult EnvironmentTest()
        {
            try
            {
                // Direct environment variable reading
                var aspNetCoreEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
                var websiteSiteName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
                var websiteHostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
                
                // Get all environment variables
                var allEnvVars = Environment.GetEnvironmentVariables()
                    .Cast<System.Collections.DictionaryEntry>()
                    .Where(e => e.Key is string key && key.Contains("ENVIRONMENT", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(
                        e => (string)e.Key,
                        e => e.Value?.ToString() ?? string.Empty
                    );
                
                return Ok(new
                {
                    DirectASPNETCORE_ENVIRONMENT = aspNetCoreEnv,
                    DirectDOTNET_ENVIRONMENT = dotnetEnv,
                    WebsiteSiteName = websiteSiteName,
                    WebsiteHostname = websiteHostname,
                    AllEnvironmentVariables = allEnvVars,
                    CurrentTime = DateTime.UtcNow,
                    ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id,
                    MachineName = Environment.MachineName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new 
                { 
                    Error = ex.Message, 
                    StackTrace = ex.StackTrace
                });
            }
        }

        // GET: api/positions/diagnostic
        [HttpGet("diagnostic")]
        public IActionResult Diagnostic()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("OracleConnection_Production");
                var databaseProvider = _configuration["DatabaseProvider"];
                
                // Get environment from multiple sources
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;
                var environmentFromConfig = _configuration["ASPNETCORE_ENVIRONMENT"] ?? string.Empty;
                var environmentFromHost = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;
                var environmentFromHosting = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;
                
                // Get IWebHostEnvironment from HttpContext
                var webHostEnvironment = HttpContext.RequestServices.GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
                var webHostEnvironmentName = webHostEnvironment?.EnvironmentName ?? string.Empty;
                var webHostIsProduction = webHostEnvironment?.IsProduction() ?? false;
                var webHostIsDevelopment = webHostEnvironment?.IsDevelopment() ?? false;
                
                // Verificar si el wallet existe
                var walletPath = "c:\\home\\site\\wwwroot\\wallet";
                var walletExists = Directory.Exists(walletPath);
                var sqlnetExists = System.IO.File.Exists(Path.Combine(walletPath, "sqlnet.ora"));
                var tnsnamesExists = System.IO.File.Exists(Path.Combine(walletPath, "tnsnames.ora"));
                
                // Obtener todas las variables de ambiente relacionadas
                var allEnvironmentVars = new Dictionary<string, string>
                {
                    ["ASPNETCORE_ENVIRONMENT"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty,
                    ["DOTNET_ENVIRONMENT"] = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty,
                    ["WEBSITE_SITE_NAME"] = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") ?? string.Empty,
                    ["WEBSITE_HOSTNAME"] = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME") ?? string.Empty,
                    ["WEBSITE_INSTANCE_ID"] = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? string.Empty
                };
                
                return Ok(new
                {
                    ConnectionString = connectionString,
                    DatabaseProvider = databaseProvider,
                    Environment = environment,
                    EnvironmentFromConfig = environmentFromConfig,
                    EnvironmentFromHost = environmentFromHost,
                    EnvironmentFromHosting = environmentFromHosting,
                    WebHostEnvironmentName = webHostEnvironmentName,
                    WebHostIsProduction = webHostIsProduction,
                    WebHostIsDevelopment = webHostIsDevelopment,
                    WalletPath = walletPath,
                    WalletExists = walletExists,
                    SqlnetExists = sqlnetExists,
                    TnsnamesExists = tnsnamesExists,
                    AllEnvironmentVariables = allEnvironmentVars,
                    CurrentTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new 
                { 
                    Error = ex.Message, 
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message
                });
            }
        }

        // GET: api/positions/oracle-diagnostic
        [HttpGet("oracle-diagnostic")]
        public IActionResult OracleDiagnostic()
        {
            try
            {
                var results = new Dictionary<string, object>();
                
                // 1. Check environment variables
                results["ASPNETCORE_ENVIRONMENT"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;
                results["DOTNET_ENVIRONMENT"] = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;
                results["WEBSITE_SITE_NAME"] = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") ?? string.Empty;
                
                // 2. Check wallet files
                var walletPath = "c:\\home\\site\\wwwroot\\wallet";
                results["WalletPath"] = walletPath;
                results["WalletExists"] = Directory.Exists(walletPath);
                
                if (Directory.Exists(walletPath))
                {
                    var files = Directory.GetFiles(walletPath);
                    results["WalletFiles"] = files;
                    
                    // Check specific files
                    results["SqlnetExists"] = System.IO.File.Exists(Path.Combine(walletPath, "sqlnet.ora"));
                    results["TnsnamesExists"] = System.IO.File.Exists(Path.Combine(walletPath, "tnsnames.ora"));
                    results["CwalletExists"] = System.IO.File.Exists(Path.Combine(walletPath, "cwallet.sso"));
                    results["KeystoreExists"] = System.IO.File.Exists(Path.Combine(walletPath, "keystore.jks"));
                    results["TruststoreExists"] = System.IO.File.Exists(Path.Combine(walletPath, "truststore.jks"));
                    
                    // Read sqlnet.ora content
                    var sqlnetPath = Path.Combine(walletPath, "sqlnet.ora");
                    if (System.IO.File.Exists(sqlnetPath))
                    {
                        results["SqlnetContent"] = System.IO.File.ReadAllText(sqlnetPath);
                    }
                    
                    // Read tnsnames.ora content
                    var tnsnamesPath = Path.Combine(walletPath, "tnsnames.ora");
                    if (System.IO.File.Exists(tnsnamesPath))
                    {
                        results["TnsnamesContent"] = System.IO.File.ReadAllText(tnsnamesPath);
                    }
                }
                
                // 3. Check connection string
                var connectionString = _configuration.GetConnectionString("OracleConnection_Production");
                results["ConnectionString"] = connectionString ?? string.Empty;
                
                // 4. Try to create Oracle connection (without opening it)
                try
                {
                    var connection = new OracleConnection(connectionString);
                    results["ConnectionCreated"] = true;
                    results["ConnectionState"] = connection.State.ToString();
                    
                    // Try to open connection with timeout
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        connection.Open();
                        stopwatch.Stop();
                        results["ConnectionOpened"] = true;
                        results["ConnectionTimeMs"] = stopwatch.ElapsedMilliseconds;
                        results["ConnectionStateAfterOpen"] = connection.State.ToString();
                        
                        // Test a simple query
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT 1 FROM DUAL";
                            command.CommandTimeout = 30;
                            var result = command.ExecuteScalar();
                            results["TestQueryResult"] = result;
                        }
                        
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        results["ConnectionOpened"] = false;
                        results["ConnectionTimeMs"] = stopwatch.ElapsedMilliseconds;
                        results["ConnectionError"] = ex.Message;
                        results["ConnectionErrorType"] = ex.GetType().Name;
                    }
                }
                catch (Exception ex)
                {
                    results["ConnectionCreated"] = false;
                    results["ConnectionCreationError"] = ex.Message;
                    results["ConnectionCreationErrorType"] = ex.GetType().Name;
                }
                
                // 5. Network connectivity test
                try
                {
                    var host = "adb.mx-queretaro-1.oraclecloud.com";
                    var port = 1522;
                    
                    using (var client = new System.Net.Sockets.TcpClient())
                    {
                        var connectTask = client.ConnectAsync(host, port);
                        var timeoutTask = Task.Delay(10000); // 10 seconds timeout
                        
                        if (Task.WhenAny(connectTask, timeoutTask).Result == connectTask)
                        {
                            results["NetworkConnectivity"] = "Success";
                            client.Close();
                        }
                        else
                        {
                            results["NetworkConnectivity"] = "Timeout";
                        }
                    }
                }
                catch (Exception ex)
                {
                    results["NetworkConnectivity"] = "Error";
                    results["NetworkError"] = ex.Message;
                }
                
                results["CurrentTime"] = DateTime.UtcNow;
                
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new 
                { 
                    Error = ex.Message, 
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message
                });
            }
        }

        // GET: api/positions/fix-wallet
        [HttpGet("fix-wallet")]
        public IActionResult FixWallet()
        {
            try
            {
                var results = new Dictionary<string, object>();
                var walletPath = "c:\\home\\site\\wwwroot\\wallet";
                var sqlnetPath = Path.Combine(walletPath, "sqlnet.ora");
                
                results["WalletPath"] = walletPath;
                results["SqlnetPath"] = sqlnetPath;
                results["SqlnetExists"] = System.IO.File.Exists(sqlnetPath);
                
                if (System.IO.File.Exists(sqlnetPath))
                {
                    // Read current content
                    var currentContent = System.IO.File.ReadAllText(sqlnetPath);
                    results["OriginalContent"] = currentContent;
                    
                    // Fix the paths for Windows
                    var fixedContent = currentContent
                        .Replace("/home/site/wwwroot/wallet", "c:\\home\\site\\wwwroot\\wallet")
                        .Replace("DIRECTORY=\"/home/site/wwwroot/wallet\"", "DIRECTORY=\"c:\\\\home\\\\site\\\\wwwroot\\\\wallet\"");
                    
                    results["FixedContent"] = fixedContent;
                    
                    // Backup original file
                    var backupPath = sqlnetPath + ".backup";
                    System.IO.File.Copy(sqlnetPath, backupPath, true);
                    results["BackupCreated"] = true;
                    results["BackupPath"] = backupPath;
                    
                    // Write fixed content
                    System.IO.File.WriteAllText(sqlnetPath, fixedContent);
                    results["FileFixed"] = true;
                    
                    // Verify the fix
                    var verifyContent = System.IO.File.ReadAllText(sqlnetPath);
                    results["VerifiedContent"] = verifyContent;
                }
                else
                {
                    results["Error"] = "sqlnet.ora file not found";
                }
                
                results["CurrentTime"] = DateTime.UtcNow;
                
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new 
                { 
                    Error = ex.Message, 
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message
                });
            }
        }
    }
}
