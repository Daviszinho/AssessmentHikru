using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Oracle.ManagedDataAccess.Client;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Hikru.Assessment.OracleConnectivity
{   
    public static class OracleInit
    {   
        private static IConfiguration _configuration;
        private static IHostEnvironment _environment;
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        public static void Initialize(IConfiguration configuration, IHostEnvironment environment = null)
        {
            if (!_initialized)
            {
                lock (_lock)
                {
                    if (!_initialized)
                    {
                        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                        _environment = environment;
                        _initialized = true;
                    }
                }
            }
        }

        public static async Task<OracleConnection> ConnectToOracleAsync()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("OracleInit has not been initialized. Call Initialize() first.");
            }

            Console.WriteLine("\n=== OracleInit: Starting connection process ===");
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"TNS_ADMIN: {Environment.GetEnvironmentVariable("TNS_ADMIN")}");
            Console.WriteLine($"ORACLE_HOME: {Environment.GetEnvironmentVariable("ORACLE_HOME")}");
            Console.WriteLine($"LD_LIBRARY_PATH: {Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")}");

            try
            {
                // In Azure, the wallet is at /home/site/wwwroot/wallet
                var walletPath = "/home/site/wwwroot/wallet";
                Console.WriteLine($"\nChecking for wallet at: {walletPath}");

                // Verify wallet directory exists and is accessible
                if (!Directory.Exists(walletPath))
                {
                    Console.WriteLine($"❌ Wallet directory not found at: {walletPath}");
                    Console.WriteLine("Trying alternative paths...");
                    
                    // Try alternative paths
                    var alternativePath = Path.Combine(Directory.GetCurrentDirectory(), "wallet");
                    if (Directory.Exists(alternativePath))
                    {
                        walletPath = alternativePath;
                        Console.WriteLine($"✅ Found wallet at alternative path: {walletPath}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Wallet not found at alternative path: {alternativePath}");
                    }
                }
                else
                {
                    Console.WriteLine($"✅ Wallet directory found at: {walletPath}");
                }

                // Log directory contents for debugging
                LogDirectoryContents(walletPath, "Wallet directory");
                LogDirectoryContents(Directory.GetCurrentDirectory(), "Current directory");

                if (Directory.Exists(walletPath))
                {
                    try
                    {
                        Console.WriteLine("\nAttempting to connect with wallet...");
                        Console.WriteLine($"Using TNS_ADMIN: {Environment.GetEnvironmentVariable("TNS_ADMIN")}");
                        
                        // Try to list TNS entries if tnsnames.ora exists
                        var tnsNamesPath = Path.Combine(walletPath, "tnsnames.ora");
                        if (File.Exists(tnsNamesPath))
                        {
                            Console.WriteLine($"Found tnsnames.ora at: {tnsNamesPath}");
                            try
                            {
                                var tnsContent = File.ReadAllText(tnsNamesPath);
                                Console.WriteLine($"TNS entries: {tnsContent}\n");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error reading tnsnames.ora: {ex.Message}");
                            }
                        }
                        
                        return await ConnectWithWalletAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to connect with wallet: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                            if (ex.InnerException.InnerException != null)
                            {
                                Console.WriteLine($"❌ Inner inner exception: {ex.InnerException.InnerException.Message}");
                            }
                        }
                        Console.WriteLine("⚠️ Falling back to connection string...");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Wallet directory not found at: {walletPath}");
                    Console.WriteLine("⚠️ Falling back to connection string...");
                }

                Console.WriteLine("\nAttempting to connect with connection string...");
                var connectionString = _configuration.GetConnectionString("OracleConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = _configuration["OracleSettings:ConnectionString"];
                }
                Console.WriteLine($"Using connection string: {new string('*', connectionString?.Length ?? 0)}");
                
                return await ConnectWithConnectionStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌❌❌ CRITICAL ERROR in ConnectToOracleAsync ❌❌❌");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"\nInner Exception:");
                    Console.WriteLine($"Message: {ex.InnerException.Message}");
                    Console.WriteLine($"Stack Trace: {ex.InnerException.StackTrace}");
                }
                throw new Exception("Failed to connect to Oracle database. See inner exception for details.", ex);
            }
        }
        
        private static void LogDirectoryContents(string path, string description)
        {
            try
            {
                Console.WriteLine($"\n{description} contents ({path}):");
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path);
                    var dirs = Directory.GetDirectories(path);
                    
                    Console.WriteLine($"  Files ({files.Length}):");
                    foreach (var file in files.Take(20)) // Show more files for better debugging
                    {
                        var fileInfo = new FileInfo(file);
                        Console.WriteLine($"    - {Path.GetFileName(file)} ({fileInfo.Length} bytes, LastWrite: {fileInfo.LastWriteTimeUtc:u})");
                        
                        // For key files, log the first few lines
                        if (file.EndsWith(".ora") || file.EndsWith(".properties"))
                        {
                            try
                            {
                                var lines = File.ReadAllLines(file).Take(10);
                                Console.WriteLine("      Content preview:");
                                foreach (var line in lines)
                                {
                                    Console.WriteLine($"      | {line}");
                                }
                                if (File.ReadAllLines(file).Length > 10)
                                {
                                    Console.WriteLine("      ... (more lines not shown)");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"      Could not read file contents: {ex.Message}");
                            }
                        }
                    }
                    if (files.Length > 20)
                    {
                        Console.WriteLine($"    ... and {files.Length - 20} more files");
                    }
                    
                    Console.WriteLine($"  Directories ({dirs.Length}):");
                    foreach (var dir in dirs.Take(5))
                    {
                        Console.WriteLine($"    - {Path.GetFileName(dir)}/");
                    }
                    if (dirs.Length > 5)
                    {
                        Console.WriteLine($"    ... and {dirs.Length - 5} more directories");
                    }
                }
                else
                {
                    Console.WriteLine($"  ❌ Directory does not exist: {path}");
                    Console.WriteLine($"  Current directory: {Directory.GetCurrentDirectory()}");
                    Console.WriteLine($"  Directory exists: {Directory.Exists(Path.GetDirectoryName(path) ?? "/")}");
                    
                    // Try to list parent directory contents
                    try
                    {
                        var parentDir = Path.GetDirectoryName(path);
                        if (!string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir))
                        {
                            Console.WriteLine($"  Contents of parent directory ({parentDir}):");
                            var parentFiles = Directory.GetFiles(parentDir).Take(10);
                            var parentDirs = Directory.GetDirectories(parentDir).Take(5);
                            
                            Console.WriteLine("  Files:");
                            foreach (var file in parentFiles)
                            {
                                Console.WriteLine($"    - {Path.GetFileName(file)}");
                            }
                            
                            Console.WriteLine("  Directories:");
                            foreach (var dir in parentDirs)
                            {
                                Console.WriteLine($"    - {Path.GetFileName(dir)}/");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Could not list parent directory: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Error listing directory contents: {ex.Message}");
                Console.WriteLine($"  Stack trace: {ex.StackTrace}");
            }
        }

        private static async Task<OracleConnection> ConnectWithConnectionStringAsync()
        {
            var connectionString = _configuration.GetConnectionString("OracleConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Oracle connection string is not configured in appsettings.json");
            }

            Console.WriteLine($"OracleInit: Using connection string: {connectionString.Replace("Password=", "Password=****")}");
            return await OpenConnectionAsync(connectionString);
        }

        private static async Task<OracleConnection> ConnectWithWalletAsync()
        {
            try
            {
                var walletPath = Environment.GetEnvironmentVariable("TNS_ADMIN");
                if (string.IsNullOrEmpty(walletPath) || !Directory.Exists(walletPath))
                {
                    throw new DirectoryNotFoundException($"Wallet directory not found at: {walletPath}");
                }

                Console.WriteLine($"OracleInit: Using wallet at: {walletPath}");
                
                // List files in the wallet directory for debugging
                try
                {
                    var files = Directory.GetFiles(walletPath);
                    Console.WriteLine($"OracleInit: Found {files.Length} files in wallet directory:");
                    foreach (var file in files)
                    {
                        Console.WriteLine($"- {Path.GetFileName(file)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OracleInit: Error listing wallet files: {ex.Message}");
                }

                // Use the connection string from appsettings.json
                var connectionString = _configuration.GetSection("OracleSettings:ConnectionString").Value;
                if (string.IsNullOrEmpty(connectionString))
                {
                    // Fallback to hardcoded connection string if not in config
                    connectionString = "User Id=admin;" +
                                    "Connection Timeout=30;" +
                                    "Data Source=oracletest_medium;";
                }

                Console.WriteLine($"OracleInit: Using connection string: {connectionString.Replace("Password=", "Password=****")}");
                return await OpenConnectionAsync(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OracleInit: Error connecting with wallet: {ex.Message}");
                throw;
            }
        }

        private static async Task<OracleConnection> OpenConnectionAsync(string connectionString)
        {
            try
            {
                Console.WriteLine($"OracleInit: Attempting to open connection...");
                Console.WriteLine($"OracleInit: Connection string: {connectionString.Replace("Password=", "Password=****")}");
                
                var connection = new OracleConnection(connectionString);
                
                // Set additional connection properties
                connection.ConnectionString = connectionString;
                
                // Log connection details
                Console.WriteLine($"OracleInit: Connection state before OpenAsync: {connection.State}");
                Console.WriteLine($"OracleInit: Connection string: {connection.ConnectionString.Replace("Password=", "Password=****")}");
                
                // Open the connection with a timeout
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await connection.OpenAsync(cts.Token);
                
                Console.WriteLine($"OracleInit: Connection state after OpenAsync: {connection.State}");
                Console.WriteLine($"OracleInit: Server version: {connection.ServerVersion}");
                
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening database connection: {ex.Message}");
                throw new Exception("Could not establish a connection to the database.", ex);
            }
        }
    }
}