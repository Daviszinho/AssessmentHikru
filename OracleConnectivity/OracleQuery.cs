using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace Hikru.Assessment.OracleConnectivity
{
    /// <summary>
    /// Provides methods to execute Oracle database queries and stored procedures
    /// </summary>
    public class OracleQuery : IDisposable
    {
        private OracleConnection? _connection;
        private bool _disposed = false;
        private string _connectionString;
        private readonly string? _configFile;
        private readonly string? _connectionName;
        private readonly IConfiguration? _configuration;
        private readonly IHostEnvironment? _environment;

        /// <summary>
        /// Initializes a new instance of the OracleQuery class
        /// </summary>
        public OracleQuery(IConfiguration configuration, IHostEnvironment? environment = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _environment = environment;
            _connectionString = string.Empty;
            _configFile = string.Empty;
            _connectionName = string.Empty;
            
            // Get connection string from configuration
            _connectionString = _configuration.GetConnectionString("OracleConnection") ?? 
                              throw new InvalidOperationException("Oracle connection string is not configured");
            
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Initialized with connection string: {_connectionString.Replace("Password=DavisOracle25!", "Password=*****")}");
        }
        
        private async Task<OracleConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                return _connection;
            
            // In production, use the connection string directly if available
            if (!string.IsNullOrEmpty(_connectionString))
            {
                _connection = new OracleConnection(_connectionString);
                await _connection.OpenAsync(cancellationToken);
                return _connection;
            }
            
            // Fall back to OracleInit for development
            _connection = await OracleInit.ConnectToOracleAsync();
            return _connection;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Executes a stored procedure that returns a single result
        /// </summary>
        /// <typeparam name="T">The type of the result</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure</param>
        /// <param name="id">The ID parameter for the query</param>
        /// <param name="mapper">Function to map the data reader to the result type</param>
        /// <returns>A single result of type T or default if not found</returns>
        public async Task<T?> GetByIdAsync<T>(string storedProcedureName, int id, Func<OracleDataReader, T> mapper)
            where T : class
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Starting GetByIdAsync for {storedProcedureName} with ID: {id}");
            
            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name cannot be empty", nameof(storedProcedureName));

            try 
            {
                using (var connection = await GetConnectionAsync())
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Connection established");
                    
                    using (var command = new OracleCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        
                        // Add the ID parameter - using named parameter
                        var idParam = new OracleParameter("p_id", OracleDbType.Int32)
                        {
                            Value = id,
                            Direction = ParameterDirection.Input
                        };
                        command.Parameters.Add(idParam);
                        
                        // Add the output cursor parameter
                        var resultCursor = new OracleParameter
                        {
                            ParameterName = "p_cursor",
                            OracleDbType = OracleDbType.RefCursor,
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(resultCursor);

                        // Log the command details
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Command: {storedProcedureName}");
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Parameters:");
                        foreach (OracleParameter param in command.Parameters)
                        {
                            Console.WriteLine($"  {param.ParameterName} = {param.Value} (Type: {param.OracleDbType}, Direction: {param.Direction})");
                        }

                        using (var reader = (OracleDataReader)await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Found record with ID: {id}");
                                var result = mapper(reader);
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Mapped result: {Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented)}");
                                return result;
                            }
                            else
                            {
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] No record found with ID: {id}");
                                return default;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error in GetByIdAsync for ID {id}: {ex.Message}";
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] {errorMessage}");
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Inner Exception: {ex.InnerException.Message}");
                }
                throw new Exception(errorMessage, ex);
            }
        }

        /// <summary>
        /// Executes a stored procedure that returns a collection of results
        /// </summary>
        /// <typeparam name="T">The type of the result items</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure</param>
        /// <param name="mapper">Function to map each data reader row to the result type</param>
        /// <param name="parameters">Optional parameters for the stored procedure</param>
        /// <returns>A collection of type T</returns>
        public async Task<IEnumerable<T>> GetAllAsync<T>(string storedProcedureName, Func<IDataReader, T> mapper, params OracleParameter[] parameters)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Starting GetAllAsync for stored procedure: {storedProcedureName}");
            var results = new List<T>();
            OracleConnection? connection = null;
            OracleCommand? command = null;
            OracleDataReader? reader = null;

            try
            {
                // Log connection attempt
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Getting database connection...");
                var connectionStartTime = DateTime.Now;
                
                connection = await GetConnectionAsync();
                
                var connectionTime = (DateTime.Now - connectionStartTime).TotalMilliseconds;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Connection established in {connectionTime}ms. Connection state: {connection.State}");
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Connection string: {connection?.ConnectionString}");

                // Create and configure the command for the procedure
                command = new OracleCommand("get_all_positions", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.BindByName = true;
                command.InitialLONGFetchSize = -1; // For handling CLOBs if needed
                
                // Add the output parameter for the cursor
                var cursorParam = new OracleParameter
                {
                    ParameterName = "p_cursor",
                    OracleDbType = OracleDbType.RefCursor,
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(cursorParam);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Executing procedure: get_all_positions");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                try
                {
                    // Execute the procedure
                    await command.ExecuteNonQueryAsync();
                    stopwatch.Stop();
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Procedure executed in {stopwatch.ElapsedMilliseconds}ms");
                    
                    // Get the cursor from the output parameter
                    var cursor = (OracleRefCursor)cursorParam.Value;
                    if (cursor == null)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] ERROR: Cursor is null after execution");
                        return results;
                    }
                    
                    // Get the data reader from the cursor
                    reader = cursor.GetDataReader();
                    stopwatch.Stop();
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Stored procedure executed successfully in {stopwatch.ElapsedMilliseconds}ms");
                    
                    if (reader == null)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] WARNING: ExecuteReader returned null");
                        return results;
                    }

                    // Log result set schema
                    var schemaTable = reader.GetSchemaTable();
                    if (schemaTable != null)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Result set schema:");
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            Console.WriteLine($"  Column: {row["ColumnName"]}, Type: {row["DataType"]}, Size: {row["ColumnSize"]}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] No schema information available for result set");
                    }

                    // Read and map results
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Reading results...");
                    int rowCount = 0;
                    var readStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    
                    try 
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                var item = mapper(reader);
                                results.Add(item);
                                rowCount++;
                                
                                // Log first few rows for debugging
                                if (rowCount <= 5) // Log first 5 rows
                                {
                                    var rowData = new System.Text.StringBuilder();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        if (i > 0) rowData.Append(", ");
                                        var value = reader.IsDBNull(i) ? "[NULL]" : reader[i]?.ToString() ?? "[NULL]";
                                        rowData.Append($"{reader.GetName(i)}={value}");
                                    }
                                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Row {rowCount}: {rowData}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Error mapping row {rowCount + 1}: {ex}");
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Stack trace: {ex.StackTrace}");
                                
                                // Log the problematic row data
                                try 
                                {
                                    var rowData = new System.Text.StringBuilder();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        if (i > 0) rowData.Append(", ");
                                        var value = reader.IsDBNull(i) ? "[NULL]" : reader[i]?.ToString() ?? "[NULL]";
                                        rowData.Append($"{reader.GetName(i)}={value}");
                                    }
                                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Problematic row data: {rowData}");
                                }
                                catch (Exception innerEx)
                                {
                                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Failed to log row data: {innerEx.Message}");
                                }
                                
                                throw;
                            }
                        }
                        
                        readStopwatch.Stop();
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Successfully read {rowCount} rows in {readStopwatch.ElapsedMilliseconds}ms");
                    }
                    catch (Exception ex) when (ex is not OracleException)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Error while reading results: {ex}");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Error in stored procedure execution: {ex}");
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Stack trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Inner exception: {ex.InnerException}");
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Error in GetAllAsync: {ex}");
                throw;
            }
            finally
            {
                // Clean up resources
                if (reader != null && !reader.IsClosed)
                {
                    try 
                    {
                        await reader.CloseAsync();
                        await reader.DisposeAsync();
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Reader closed and disposed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Error closing reader: {ex.Message}");
                    }
                }
                
                command?.Dispose();
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Command disposed");
                
                if (connection != null && connection.State != ConnectionState.Closed)
                {
                    try 
                    {
                        await connection.CloseAsync();
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Database connection closed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] Error closing connection: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [OracleQuery] GetAllAsync completed. Returning {results.Count} results");
            return results;
        }

        /// <summary>
        /// Executes a stored procedure that doesn't return any result set (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure</param>
        /// <param name="parameters">Optional parameters for the stored procedure</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> ExecuteNonQueryAsync(
            string storedProcedureName,
            params OracleParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name cannot be empty", nameof(storedProcedureName));

            using (var connection = await GetConnectionAsync())
            {
                using (var command = new OracleCommand(storedProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.BindByName = true; // Important: Use named parameters

                    // Clear any existing parameters
                    command.Parameters.Clear();
                    
                    // Add input parameters with explicit types
                    if (parameters != null && parameters.Length > 0)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param);
                        }
                    }

                    // Find the success parameter in the provided parameters
                    var successParam = parameters?.FirstOrDefault(p => 
                        p.ParameterName.Equals("p_success", StringComparison.OrdinalIgnoreCase));
                    
                    if (successParam == null)
                    {
                        // If not found, add it
                        successParam = new OracleParameter
                        {
                            ParameterName = "p_success",
                            OracleDbType = OracleDbType.Int32,
                            Direction = ParameterDirection.Output,
                            Size = 22  // Explicit size for Oracle NUMBER type
                        };
                        command.Parameters.Add(successParam);
                    }
                    
                    // Log the parameter types before execution
                    Console.WriteLine("[OracleQuery] Parameter types before execution:");
                    foreach (OracleParameter param in command.Parameters)
                    {
                        Console.WriteLine($"  {param.ParameterName}: Type={param.OracleDbType}, .NET Type={param.Value?.GetType().FullName ?? "null"}");
                    }

                    // Log the command details
                    Console.WriteLine($"[OracleQuery] === Executing Command ===");
                    Console.WriteLine($"[OracleQuery] Type: {command.CommandType}");
                    Console.WriteLine($"[OracleQuery] Text: {command.CommandText}");
                    Console.WriteLine($"[OracleQuery] Parameters ({command.Parameters.Count}):");
                    
                    // Log detailed parameter information
                    foreach (OracleParameter param in command.Parameters)
                    {
                        string direction = param.Direction == ParameterDirection.Input ? "IN" : 
                                         param.Direction == ParameterDirection.Output ? "OUT" : "INOUT";
                        string valueStr = param.Value == DBNull.Value ? "NULL" : 
                                         (param.Value?.ToString() ?? "null");
                        
                        Console.WriteLine($"  Parameter: {param.ParameterName}");
                        Console.WriteLine($"    Type: {param.OracleDbType}");
                        Console.WriteLine($"    Direction: {direction}");
                        Console.WriteLine($"    Size: {param.Size}");
                        Console.WriteLine($"    Precision: {param.Precision}");
                        Console.WriteLine($"    Scale: {param.Scale}");
                        Console.WriteLine($"    Value: {valueStr}");
                        
                        // Special handling for DateTime parameters
                        if (param.OracleDbType == OracleDbType.Date || 
                            param.OracleDbType == OracleDbType.TimeStamp ||
                            param.OracleDbType == OracleDbType.TimeStampLTZ ||
                            param.OracleDbType == OracleDbType.TimeStampTZ)
                        {
                            if (param.Value != null && param.Value != DBNull.Value)
                            {
                                Console.WriteLine($"    .NET Type: {param.Value.GetType().FullName}");
                            }
                        }
                    }

                    try
                    {
                        // Execute the command
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        
                        // Log detailed type information about the success parameter
                        Console.WriteLine($"[OracleQuery] Success parameter details:");
                        Console.WriteLine($"  Type: {successParam.Value?.GetType().FullName ?? "null"}");
                        Console.WriteLine($"  Value: {successParam.Value?.ToString() ?? "null"}");
                        
                        // Simple approach - just check if the value can be converted to "1" for success
                        int success = 0;
                        try 
                        {
                            string stringValue = successParam.Value?.ToString() ?? "0";
                            success = stringValue.Trim() == "1" ? 1 : 0;
                            Console.WriteLine($"[OracleQuery] Parsed success value: {success}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[OracleQuery] Error parsing success value: {ex.Message}");
                        }
                        
                        // Log the result
                        Console.WriteLine($"[OracleQuery] Execution completed. Rows affected: {rowsAffected}, Success flag: {success}");
                        
                        // In Oracle, ExecuteNonQuery returns -1 for successful execution of PL/SQL blocks
                        // So we'll rely on the success flag from the stored procedure
                        if (success != 1)
                        {
                            throw new Exception($"Stored procedure execution failed with success code: {success}");
                        }
                        
                        // Return the number of rows affected (or 1 if it's -1, which is common for PL/SQL blocks)
                        return rowsAffected >= 0 ? rowsAffected : 1;
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Error executing stored procedure {storedProcedureName}. ";
                        
                        try 
                        {
                            // Try to get the success value safely
                            string successValue = "N/A";
                            if (successParam?.Value != null)
                            {
                                if (successParam.Value is OracleDecimal oracleDecimal)
                                {
                                    successValue = oracleDecimal.Value.ToString();
                                }
                                else
                                {
                                    successValue = successParam.Value.ToString() ?? "N/A";
                                }
                            }
                            
                            errorMessage += $"Success: {successValue}. ";
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"[OracleQuery] Error getting success value: {innerEx.Message}");
                            errorMessage += "Success: Error reading value. ";
                        }
                        
                        errorMessage += $"Error: {ex.Message}";
                        Console.WriteLine($"[OracleQuery] {errorMessage}");
                        
                        // Include inner exception details if available
                        if (ex.InnerException != null)
                        {
                            errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                        }
                        
                        throw new Exception(errorMessage, ex);
                    }
                }
            }
        }

        
        /// <summary>
        /// Executes a direct SQL statement that doesn't return any result set (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="sql">The SQL statement to execute</param>
        /// <param name="parameters">Optional parameters for the SQL statement</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> ExecuteSqlAsync(
            string sql,
            params OracleParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL statement cannot be empty", nameof(sql));

            using (var connection = await GetConnectionAsync())
            {
                using (var command = new OracleCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // Add input parameters
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    // Execute the command
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
