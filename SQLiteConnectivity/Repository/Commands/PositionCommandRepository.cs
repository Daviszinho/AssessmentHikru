using System.Data;
using Microsoft.Data.Sqlite;
using Lib.Repository.Entities;
using Lib.Repository.Repository.Commands;
using System.IO;
using System.Threading.Tasks;

namespace SQLiteConnectivity.Repository.Commands
{
    public class PositionCommandRepository : IPositionCommandRepository, IDisposable
    {
        private readonly string _connectionString;
        private SqliteConnection? _connection;

        public PositionCommandRepository(string connectionString)
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

        public async Task<int?> AddPositionAsync(Position position)
        {
            using (var connection = GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO Position (Title, DepartmentId, Description, Location, Status, RecruiterId, Budget, ClosingDate, CreatedAt, UpdatedAt)
                    VALUES (@Title, @DepartmentId, @Description, @Location, @Status, @RecruiterId, @Budget, @ClosingDate, @CreatedAt, @UpdatedAt);
                    SELECT last_insert_rowid();";

                command.Parameters.AddWithValue("@Title", position.Title);
                command.Parameters.AddWithValue("@DepartmentId", position.DepartmentId);
                command.Parameters.AddWithValue("@Description", position.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Location", position.Location ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Status", position.Status ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@RecruiterId", position.RecruiterId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Budget", position.Budget ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ClosingDate", position.ClosingDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedAt", position.CreatedAt);
                command.Parameters.AddWithValue("@UpdatedAt", position.UpdatedAt);

                var result = await command.ExecuteScalarAsync();
                if (result != null && int.TryParse(result.ToString(), out int id))
                {
                    return id;
                }
                return null;
            }
        }


        public async Task<bool> UpdatePositionAsync(Position position)
        {
            using (var connection = GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE Position 
                    SET Title = @Title,
                        DepartmentId = @DepartmentId,
                        Description = @Description,
                        Location = @Location,
                        Status = @Status,
                        RecruiterId = @RecruiterId,
                        Budget = @Budget,
                        ClosingDate = @ClosingDate,
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", position.Id);
                command.Parameters.AddWithValue("@Title", position.Title);
                command.Parameters.AddWithValue("@DepartmentId", position.DepartmentId);
                command.Parameters.AddWithValue("@Description", position.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Location", position.Location ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Status", position.Status ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@RecruiterId", position.RecruiterId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Budget", position.Budget ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ClosingDate", position.ClosingDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        public async Task<bool> RemovePositionAsync(int positionId)
        {
            using (var connection = GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Position WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", positionId);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }


        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
