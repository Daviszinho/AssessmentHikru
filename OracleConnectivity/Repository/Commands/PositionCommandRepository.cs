using System;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using Lib.Repository.Entities;
using Lib.Repository.Repository.Commands;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace OracleConnectivity.Repository.Commands
{
    public class PositionCommandRepository : IPositionCommandRepository, IDisposable
    {
        private readonly OracleConnection _connection;
        private bool _disposed = false;
        private readonly IConfiguration? _configuration;
        private readonly IHostEnvironment? _environment;

        public PositionCommandRepository(string connectionString, IConfiguration? configuration = null, IHostEnvironment? environment = null)
        {
            _connection = new OracleConnection(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
            _configuration = configuration;
            _environment = environment;
        }

        public PositionCommandRepository(IConfiguration configuration, IHostEnvironment? environment = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _environment = environment;
            var connectionString = _configuration.GetConnectionString("OracleConnection") ?? 
                throw new ArgumentNullException("OracleConnection string is not configured");
            _connection = new OracleConnection(connectionString);
        }

        public async Task<int?> AddPositionAsync(Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));

            try
            {
                await _connection.OpenAsync();
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Position (Title, DepartmentId, Description, IsActive, CreatedAt, UpdatedAt)
                    VALUES (:title, :departmentId, :description, :isActive, :createdAt, :updatedAt)
                    RETURNING Id INTO :id";

                command.Parameters.Add("title", OracleDbType.Varchar2).Value = position.Title;
                command.Parameters.Add("departmentId", OracleDbType.Int32).Value = position.DepartmentId;
                command.Parameters.Add("description", OracleDbType.Varchar2).Value = position.Description is not null ? (object)position.Description : DBNull.Value;
                command.Parameters.Add("isActive", OracleDbType.Byte).Value = position.IsActive ? 1 : 0;
                command.Parameters.Add("createdAt", OracleDbType.TimeStamp).Value = DateTime.UtcNow;
                command.Parameters.Add("updatedAt", OracleDbType.TimeStamp).Value = DateTime.UtcNow;
                
                // For Oracle, we'll use a sequence or RETURNING clause
                command.CommandText = @"
                    INSERT INTO Position (Title, DepartmentId, Description, IsActive, CreatedAt, UpdatedAt)
                    VALUES (:title, :departmentId, :description, :isActive, :createdAt, :updatedAt)
                    RETURNING Id INTO :id";

                // For Oracle, we'll use a sequence to get the next ID
                command.CommandText = @"
                    INSERT INTO Position (Id, Title, DepartmentId, Description, IsActive, CreatedAt, UpdatedAt)
                    VALUES (POSITION_SEQ.NEXTVAL, :title, :departmentId, :description, :isActive, :createdAt, :updatedAt)
                    RETURNING Id INTO :newId";

                var idParam = new OracleParameter("newId", OracleDbType.Decimal, ParameterDirection.Output);
                command.Parameters.Add(idParam);

                await command.ExecuteNonQueryAsync();
                
                // Get the ID from the output parameter
                if (idParam.Value != DBNull.Value && idParam.Value != null)
                {
                    return Convert.ToInt32(Convert.ToDecimal(idParam.Value));
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error adding position: {ex.Message}");
                return null;
            }
            finally
            {
                if (_connection.State != System.Data.ConnectionState.Closed)
                    await _connection.CloseAsync();
            }
        }


        public async Task<bool> UpdatePositionAsync(Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            if (position.Id <= 0) throw new ArgumentException("Valid Position ID is required for update");

            try
            {
                await _connection.OpenAsync();
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Position 
                    SET Title = :title, 
                        DepartmentId = :departmentId, 
                        Description = :description, 
                        IsActive = :isActive,
                        UpdatedAt = :updatedAt
                    WHERE Id = :id";

                command.Parameters.Add("title", OracleDbType.Varchar2).Value = position.Title;
                command.Parameters.Add("departmentId", OracleDbType.Int32).Value = position.DepartmentId;
                command.Parameters.Add("description", OracleDbType.Varchar2).Value = (object?)position.Description ?? DBNull.Value;
                command.Parameters.Add("isActive", OracleDbType.Byte).Value = position.IsActive ? 1 : 0;
                command.Parameters.Add("updatedAt", OracleDbType.TimeStamp).Value = DateTime.UtcNow;
                command.Parameters.Add("id", OracleDbType.Int32).Value = position.Id;

                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error updating position: {ex.Message}");
                return false;
            }
            finally
            {
                if (_connection.State != System.Data.ConnectionState.Closed)
                    await _connection.CloseAsync();
            }
        }

        public async Task<bool> RemovePositionAsync(int id)
        {
            try
            {
                await _connection.OpenAsync();
                using var command = _connection.CreateCommand();
                command.CommandText = "DELETE FROM Position WHERE Id = :id";
                command.Parameters.Add("id", OracleDbType.Int32).Value = id;

                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error removing position: {ex.Message}");
                return false;
            }
            finally
            {
                if (_connection.State != System.Data.ConnectionState.Closed)
                    await _connection.CloseAsync();
            }
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
