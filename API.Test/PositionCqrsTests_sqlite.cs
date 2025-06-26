using System;
using System.IO;
using System.Threading.Tasks;
using Lib.Repository.Entities;
using Lib.Repository.Repository.Commands;
using Lib.Repository.Repository.Queries;
using SQLiteConnectivity.Repository.Commands;
using SQLiteConnectivity.Repository.Queries;
using Xunit;

namespace API.Test
{
    public class PositionCqrsTests : IDisposable
    {
        private readonly IPositionCommandRepository _commandRepository;
        private readonly IPositionQueryRepository _queryRepository;
        private readonly string _testDbPath;
        private readonly string _connectionString;

        public PositionCqrsTests()
        {
            // Create a unique test database for each test run
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.db");
            _connectionString = $"Data Source={_testDbPath}";
            
            // Initialize repositories
            _commandRepository = new PositionCommandRepository(_connectionString);
            _queryRepository = new PositionQueryRepository(_connectionString);
            
            // Initialize database
            InitializeTestDatabase();
        }

        private void InitializeTestDatabase()
        {
            // This would typically be done via migrations or a script
            // For testing, we'll create a simple table
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString))
            {
                connection.Open();
                
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Position (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        DepartmentId INTEGER NOT NULL,
                        Description TEXT,
                        Location TEXT,
                        Status TEXT,
                        RecruiterId INTEGER,
                        Budget DECIMAL,
                        ClosingDate DATETIME,
                        IsActive BOOLEAN NOT NULL DEFAULT 1,
                        CreatedAt DATETIME NOT NULL,
                        UpdatedAt DATETIME NOT NULL
                    );";
                command.ExecuteNonQuery();
            }
        }

        [Fact]
        public async Task AddPosition_ShouldReturnId()
        {
            // Arrange
            var position = new Position
            {
                Title = "Test Position",
                DepartmentId = 1,
                Description = "Test Description",
                IsActive = true
            };

            // Act
            var positionId = await _commandRepository.AddPositionAsync(position);

            // Assert
            Assert.True(positionId.HasValue);
            Assert.True(positionId.Value > 0);
        }

        [Fact]
        public async Task GetPositionById_ShouldReturnPosition()
        {
            // Arrange
            var position = new Position
            {
                Title = "Test Get Position",
                DepartmentId = 1,
                Description = "Test Get Description",
                IsActive = true
            };
            
            var positionId = await _commandRepository.AddPositionAsync(position);
            Assert.True(positionId.HasValue);

            // Act
            var result = await _queryRepository.GetPositionByIdAsync(positionId.Value);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(position.Title, result.Title);
            Assert.Equal(position.Description, result.Description);

        }

        [Fact]
        public async Task UpdatePosition_ShouldUpdateSuccessfully()
        {
            // Arrange
            var position = new Position
            {
                Title = "Original Title",
                DepartmentId = 1,

                IsActive = true
            };
            
            var positionId = await _commandRepository.AddPositionAsync(position);
            Assert.True(positionId.HasValue);

            // Act
            var updatedPosition = new Position
            {
                Id = positionId.Value,
                Title = "Updated Title",
                DepartmentId = 1,
                Description = "Updated Description",
                IsActive = true
            };
            
            var updateResult = await _commandRepository.UpdatePositionAsync(updatedPosition);
            
            // Assert
            Assert.True(updateResult);
            
            var result = await _queryRepository.GetPositionByIdAsync(positionId.Value);
            Assert.NotNull(result);
            Assert.Equal(updatedPosition.Title, result.Title);
            Assert.Equal(updatedPosition.Description, result.Description);

        }

        [Fact]
        public async Task RemovePosition_ShouldRemoveSuccessfully()
        {
            // Arrange
            var position = new Position
            {
                Title = "Position to Remove",
                DepartmentId = 1,
                IsActive = true
            };
            
            var positionId = await _commandRepository.AddPositionAsync(position);
            Assert.True(positionId.HasValue);

            // Act
            var removeResult = await _commandRepository.RemovePositionAsync(positionId.Value);
            
            // Assert
            Assert.True(removeResult);
            
            var result = await _queryRepository.GetPositionByIdAsync(positionId.Value);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllPositions_ShouldReturnAllPositions()
        {
            // Arrange - Add multiple positions
            for (int i = 0; i < 3; i++)
            {
                await _commandRepository.AddPositionAsync(new Position
                {
                    Title = $"Position {i}",
                    DepartmentId = i + 1,

                    IsActive = true
                });
            }

            // Act
            var positions = await _queryRepository.GetAllPositionsAsync();

            // Assert
            Assert.NotNull(positions);
            Assert.Equal(3, positions.Count());
        }

        public void Dispose()
        {
            // Clean up the test database
            if (File.Exists(_testDbPath))
            {
                try
                {
                    File.Delete(_testDbPath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}
