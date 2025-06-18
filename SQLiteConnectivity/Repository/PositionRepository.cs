using System.Data;
using Microsoft.Data.Sqlite;
using Lib.Repository.Entities;
using Lib.Repository.Repository;

namespace SQLiteConnectivity.Repository
{
    public class PositionRepository : IPositionRepository, IDisposable
    {
        private readonly string _connectionString;
        private SqliteConnection? _connection;

        public PositionRepository(string connectionString)
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
            using (var command = new SqliteCommand("SELECT * FROM Position", connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    positions.Add(new Position
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("ID")),
                        Title = reader.GetString(reader.GetOrdinal("TITLE")),
                        Description = reader.IsDBNull(reader.GetOrdinal("DESCRIPTION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPTION")),
                        Location = reader.IsDBNull(reader.GetOrdinal("LOCATION")) ? null : reader.GetString(reader.GetOrdinal("LOCATION")),
                        DepartmentId = reader.IsDBNull(reader.GetOrdinal("DEPARTMENTID")) ? null : reader.GetInt32(reader.GetOrdinal("DEPARTMENTID")),
                        RecruiterId = reader.IsDBNull(reader.GetOrdinal("RECRUITERID")) ? null : reader.GetInt32(reader.GetOrdinal("RECRUITERID")),
                        Budget = reader.IsDBNull(reader.GetOrdinal("BUDGET")) ? null : reader.GetDecimal(reader.GetOrdinal("BUDGET")),
                        ClosingDate = reader.IsDBNull(reader.GetOrdinal("CLOSINGDATE")) ? null : reader.GetDateTime(reader.GetOrdinal("CLOSINGDATE")),
                        Status = reader.IsDBNull(reader.GetOrdinal("STATUS")) ? null : reader.GetString(reader.GetOrdinal("STATUS"))
                    });
                }
            }

            return positions;
        }

        public async Task<Position?> GetPositionByIdAsync(int positionId)
        {
            using (var connection = GetConnection())
            using (var command = new SqliteCommand("SELECT * FROM Position WHERE ID = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", positionId);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Position
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ID")),
                            Title = reader.GetString(reader.GetOrdinal("TITLE")),
                            Description = reader.IsDBNull(reader.GetOrdinal("DESCRIPTION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPTION")),
                            Location = reader.IsDBNull(reader.GetOrdinal("LOCATION")) ? null : reader.GetString(reader.GetOrdinal("LOCATION")),
                            DepartmentId = reader.IsDBNull(reader.GetOrdinal("DEPARTMENTID")) ? null : reader.GetInt32(reader.GetOrdinal("DEPARTMENTID")),
                            RecruiterId = reader.IsDBNull(reader.GetOrdinal("RECRUITERID")) ? null : reader.GetInt32(reader.GetOrdinal("RECRUITERID")),
                            Budget = reader.IsDBNull(reader.GetOrdinal("BUDGET")) ? null : reader.GetDecimal(reader.GetOrdinal("BUDGET")),
                            ClosingDate = reader.IsDBNull(reader.GetOrdinal("CLOSINGDATE")) ? null : reader.GetDateTime(reader.GetOrdinal("CLOSINGDATE")),
                            Status = reader.IsDBNull(reader.GetOrdinal("STATUS")) ? null : reader.GetString(reader.GetOrdinal("STATUS"))
                        };
                    }
                }
            }
            return null;
        }

        public async Task<int?> AddPositionAsync(Position position)
        {
            using (var connection = GetConnection())
            using (var command = new SqliteCommand(
                "INSERT INTO Position (Title, Description, Location, DepartmentID, RecruiterID, Budget, ClosingDate, Status) " +
                "VALUES (@Title, @Description, @Location, @DepartmentId, @RecruiterId, @Budget, @ClosingDate, @Status); " +
                "SELECT last_insert_rowid();", connection))
            {
                AddPositionParameters(command, position);
                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : null;
            }
        }

        public async Task<bool> UpdatePositionAsync(Position position)
        {
            using (var connection = GetConnection())
            using (var command = new SqliteCommand(
                "UPDATE Position SET Title = @Title, Description = @Description, " +
                "Location = @Location, DepartmentID = @DepartmentId, RecruiterID = @RecruiterId, " +
                "Budget = @Budget, ClosingDate = @ClosingDate, Status = @Status " +
                "WHERE ID = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", position.Id);
                AddPositionParameters(command, position);
                
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> RemovePositionAsync(int positionId)
        {
            using (var connection = GetConnection())
            using (var command = new SqliteCommand("DELETE FROM Position WHERE ID = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", positionId);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        private void AddPositionParameters(SqliteCommand command, Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            
            // Required fields
            command.Parameters.AddWithValue("@Title", position.Title ?? string.Empty);
            
            // Optional fields
            AddNullableParameter(command, "@Description", position.Description);
            AddNullableParameter(command, "@Location", position.Location);
            AddNullableParameter(command, "@DepartmentId", position.DepartmentId);
            AddNullableParameter(command, "@RecruiterId", position.RecruiterId);
            AddNullableParameter(command, "@Budget", position.Budget);
            
            // Handle ClosingDate - convert to ISO 8601 string format for SQLite
            if (position.ClosingDate.HasValue)
            {
                command.Parameters.AddWithValue("@ClosingDate", position.ClosingDate.Value.ToString("yyyy-MM-dd"));
            }
            else
            {
                command.Parameters.AddWithValue("@ClosingDate", DBNull.Value);
            }
            
            // Status is required in the database schema
            command.Parameters.AddWithValue("@Status", position.Status ?? "Open");
        }

        private static void AddNullableParameter<T>(SqliteCommand command, string parameterName, T? value) where T : struct
        {
            command.Parameters.AddWithValue(parameterName, value.HasValue ? (object)value.Value : DBNull.Value);
        }

        private static void AddNullableParameter(SqliteCommand command, string parameterName, string? value)
        {
            command.Parameters.AddWithValue(parameterName, string.IsNullOrEmpty(value) ? DBNull.Value : (object)value);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}