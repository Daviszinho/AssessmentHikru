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
            using (var command = new SqliteCommand("SELECT * FROM PositionDetails", connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    positions.Add(new Position
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("PositionID")),
                        Title = reader.GetString(reader.GetOrdinal("PositionTitle")),
                        Description = reader.IsDBNull(reader.GetOrdinal("PositionDescription")) ? null : reader.GetString(reader.GetOrdinal("PositionDescription")),
                        Location = reader.IsDBNull(reader.GetOrdinal("PositionLocation")) ? null : reader.GetString(reader.GetOrdinal("PositionLocation")),
                        DepartmentId = reader.IsDBNull(reader.GetOrdinal("DepartmentID")) ? null : reader.GetInt32(reader.GetOrdinal("DepartmentID")),
                        RecruiterId = reader.IsDBNull(reader.GetOrdinal("RecruiterID")) ? null : reader.GetInt32(reader.GetOrdinal("RecruiterID")),
                        Budget = reader.IsDBNull(reader.GetOrdinal("PositionBudget")) ? null : reader.GetDecimal(reader.GetOrdinal("PositionBudget")),
                        ClosingDate = reader.IsDBNull(reader.GetOrdinal("PositionClosingDate")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("PositionClosingDate"))),
                        Status = reader.IsDBNull(reader.GetOrdinal("PositionStatus")) ? null : reader.GetString(reader.GetOrdinal("PositionStatus")),
                        DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentName")) ? null : reader.GetString(reader.GetOrdinal("DepartmentName")),
                        RecruiterName = reader.IsDBNull(reader.GetOrdinal("RecruiterName")) ? null : reader.GetString(reader.GetOrdinal("RecruiterName"))
                    });
                }
            }

            return positions;
        }

        public async Task<Position?> GetPositionByIdAsync(int positionId)
        {
            using (var connection = GetConnection())
            using (var command = new SqliteCommand("SELECT * FROM Positions WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", positionId);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Position
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
                            DepartmentId = reader.IsDBNull(reader.GetOrdinal("DepartmentId")) ? null : reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            RecruiterId = reader.IsDBNull(reader.GetOrdinal("RecruiterId")) ? null : reader.GetInt32(reader.GetOrdinal("RecruiterId")),
                            Budget = reader.IsDBNull(reader.GetOrdinal("Budget")) ? null : reader.GetDecimal(reader.GetOrdinal("Budget")),
                            ClosingDate = reader.IsDBNull(reader.GetOrdinal("ClosingDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ClosingDate")),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status")),
                            DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentName")) ? null : reader.GetString(reader.GetOrdinal("DepartmentName")),
                            RecruiterName = reader.IsDBNull(reader.GetOrdinal("RecruiterName")) ? null : reader.GetString(reader.GetOrdinal("RecruiterName"))
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
                "INSERT INTO Positions (Title, Description, Location, DepartmentId, RecruiterId, Budget, ClosingDate, Status, DepartmentName, RecruiterName) " +
                "VALUES (@Title, @Description, @Location, @DepartmentId, @RecruiterId, @Budget, @ClosingDate, @Status, @DepartmentName, @RecruiterName); " +
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
                "UPDATE Positions SET Title = @Title, Description = @Description, Location = @Location, " +
                "DepartmentId = @DepartmentId, RecruiterId = @RecruiterId, Budget = @Budget, " +
                "ClosingDate = @ClosingDate, Status = @Status, DepartmentName = @DepartmentName, " +
                "RecruiterName = @RecruiterName WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", position.Id);
                AddPositionParameters(command, position);
                
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> RemovePositionAsync(int positionId)
        {
            using (var connection = GetConnection())
            using (var command = new SqliteCommand("DELETE FROM Positions WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", positionId);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        private void AddPositionParameters(SqliteCommand command, Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            
            command.Parameters.AddWithValue("@Title", position.Title);
            AddNullableParameter(command, "@Description", position.Description);
            AddNullableParameter(command, "@Location", position.Location);
            AddNullableParameter(command, "@DepartmentId", position.DepartmentId);
            AddNullableParameter(command, "@RecruiterId", position.RecruiterId);
            AddNullableParameter(command, "@Budget", position.Budget);
            AddNullableParameter(command, "@ClosingDate", position.ClosingDate);
            AddNullableParameter(command, "@Status", position.Status);
            AddNullableParameter(command, "@DepartmentName", position.DepartmentName);
            AddNullableParameter(command, "@RecruiterName", position.RecruiterName);
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