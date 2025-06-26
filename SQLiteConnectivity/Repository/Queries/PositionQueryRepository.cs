
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
            Console.WriteLine("Dentro de PositionQueryRepository, Connection string: " + _connectionString);


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
            var position = new Position
            {
                Id = reader.GetInt32(reader.GetOrdinal("ID")),
                Title = reader.GetString(reader.GetOrdinal("TITLE")),
                Description = !reader.IsDBNull(reader.GetOrdinal("DESCRIPTION")) ? 
                    reader.GetString(reader.GetOrdinal("DESCRIPTION")) : null,
                Location = !reader.IsDBNull(reader.GetOrdinal("LOCATION")) ? 
                    reader.GetString(reader.GetOrdinal("LOCATION")) : null,
                DepartmentId = reader.GetInt32(reader.GetOrdinal("DEPARTMENTID")),
                RecruiterId = reader.IsDBNull(reader.GetOrdinal("RECRUITERID")) ? 
                    null : reader.GetInt32(reader.GetOrdinal("RECRUITERID")),
                Status = !reader.IsDBNull(reader.GetOrdinal("STATUS")) ? 
                    reader.GetString(reader.GetOrdinal("STATUS")) : null,
                Budget = reader.IsDBNull(reader.GetOrdinal("BUDGET")) ? 
                    null : reader.GetDecimal(reader.GetOrdinal("BUDGET")),
                ClosingDate = reader.IsDBNull(reader.GetOrdinal("CLOSINGDATE")) ? 
                    null : DateTime.Parse(reader.GetString(reader.GetOrdinal("CLOSINGDATE"))),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CREATEDAT"))),
                UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UPDATEDAT"))),
                // Asumiendo que IsActive no existe en la base de datos, lo establecemos como true por defecto
                IsActive = true,
                // Establecer Level como cadena vacía ya que no existe en la base de datos
                Level = ""
            };

            return position;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
