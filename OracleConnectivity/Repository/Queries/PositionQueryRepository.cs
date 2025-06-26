using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Lib.Repository.Entities;
using Lib.Repository.Repository.Queries;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace OracleConnectivity.Repository.Queries
{
    public class PositionQueryRepository : IPositionQueryRepository, IDisposable
    {
        private readonly OracleConnection _connection;
        private bool _disposed = false;
        private readonly IConfiguration? _configuration;
        private readonly IHostEnvironment? _environment;

        public PositionQueryRepository(string connectionString, IConfiguration? configuration = null, IHostEnvironment? environment = null)
        {
            _connection = new OracleConnection(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
            _configuration = configuration;
            _environment = environment;
        }

        public PositionQueryRepository(IConfiguration configuration, IHostEnvironment? environment = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _environment = environment;
            var connectionString = _configuration.GetConnectionString("OracleConnection");
            _connection = new OracleConnection(connectionString);
        }

        public async Task<IEnumerable<Position>> GetAllPositionsAsync()
        {
            var positions = new List<Position>();

            try
            {
                await _connection.OpenAsync();
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, Title, DepartmentId, Description, IsActive, CreatedAt, UpdatedAt
                    FROM Position";

                using var dbReader = await command.ExecuteReaderAsync();
                var reader = (OracleDataReader)dbReader;
                while (await reader.ReadAsync())
                {
                    var position = MapToPosition(reader);
                    if (position != null)
                    {
                        positions.Add(position);
                    }
                }

                return positions;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error getting all positions: {ex.Message}");
                throw;
            }
            finally
            {
                if (_connection.State != System.Data.ConnectionState.Closed)
                    await _connection.CloseAsync();
            }
        }

        public async Task<Position?> GetPositionByIdAsync(int id)
        {
            try
            {
                await _connection.OpenAsync();
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, Title, DepartmentId, Description, IsActive, CreatedAt, UpdatedAt
                    FROM Position
                    WHERE Id = :id";
                command.Parameters.Add("id", OracleDbType.Int32).Value = id;

                using var dbReader = await command.ExecuteReaderAsync();
                var reader = (OracleDataReader)dbReader;
                if (await reader.ReadAsync())
                {
                    return MapToPosition(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error getting position by ID {id}: {ex.Message}");
                throw;
            }
            finally
            {
                if (_connection.State != System.Data.ConnectionState.Closed)
                    await _connection.CloseAsync();
            }
        }

        private Position? MapToPosition(OracleDataReader reader)
        {
            return new Position
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                Description = !reader.IsDBNull(reader.GetOrdinal("Description")) 
                    ? reader.GetString(reader.GetOrdinal("Description")) 
                    : null,
                IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
