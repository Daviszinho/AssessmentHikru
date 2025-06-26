
using System.Data;
using Microsoft.Data.Sqlite;
using Lib.Repository.Entities;
using Lib.Repository.Repository.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SQLiteConnectivity.Repository.Queries
{
    public class PositionQueryRepository : IPositionQueryRepository, IDisposable
    {
        private readonly string _connectionString;
        private SqliteConnection? _connection;

        public PositionQueryRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private SqliteConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection(_connectionString);
                _connection.Open();
            }
            else if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }

        public async Task<IEnumerable<Position>> GetAllPositionsAsync()
        {
            var positions = new List<Position>();
            
            using (var connection = GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Position";
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        positions.Add(MapToPosition(reader));
                    }
                }
            }
            
            return positions;
        }

        public async Task<Position?> GetPositionByIdAsync(int positionId)
        {
            using (var connection = GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Position WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", positionId);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapToPosition(reader);
                    }
                }
            }
            
            return null;
        }

        private Position MapToPosition(SqliteDataReader reader)
        {
            return new Position
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                Level = reader.GetString(reader.GetOrdinal("Level")),
                Description = !reader.IsDBNull(reader.GetOrdinal("Description")) ? 
                    reader.GetString(reader.GetOrdinal("Description")) : null,
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
