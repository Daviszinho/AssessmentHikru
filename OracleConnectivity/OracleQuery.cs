using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Hikru.Assessment.OracleConnectivity
{
    /// <summary>
    /// Provides methods to execute Oracle database queries and stored procedures
    /// </summary>
    public class OracleQuery : IDisposable
    {
        private OracleConnection? _connection;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the OracleQuery class
        /// </summary>
        public OracleQuery()
        {
        }
        
        private async Task<OracleConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                return _connection;
                
            // Use OracleInit to get a connection
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
            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name cannot be empty", nameof(storedProcedureName));

            using (var connection = await GetConnectionAsync())
            {
                using (var command = new OracleCommand(storedProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    
                    // Add the ID parameter
                    command.Parameters.Add("ID", OracleDbType.Int32).Value = id;
                    
                    // Add the output cursor parameter
                    var resultCursor = new OracleParameter
                    {
                        ParameterName = "p_cursor",
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(resultCursor);

                    using (var reader = (OracleDataReader)await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return mapper(reader);
                        }
                    }
                }
            }


            return default;
        }

        /// <summary>
        /// Executes a stored procedure that returns a collection of results
        /// </summary>
        /// <typeparam name="T">The type of the result items</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure</param>
        /// <param name="mapper">Function to map each data reader row to the result type</param>
        /// <param name="parameters">Optional parameters for the stored procedure</param>
        /// <returns>A collection of type T</returns>
        public async Task<IEnumerable<T>> GetAllAsync<T>(
            string storedProcedureName, 
            Func<OracleDataReader, T> mapper,
            params OracleParameter[] parameters) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name cannot be empty", nameof(storedProcedureName));

            var results = new List<T>();

            using (var connection = await GetConnectionAsync())
            {
                using (var command = new OracleCommand(storedProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add output cursor parameter
                    var resultCursor = new OracleParameter
                    {
                        ParameterName = "p_cursor",
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(resultCursor);

                    // Add any additional parameters
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (var reader = (OracleDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(mapper(reader));
                        }
                    }
                }
            }


            return results;
        }
    }
}
