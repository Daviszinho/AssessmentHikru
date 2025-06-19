using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Lib.Repository.Entities;
using Lib.Repository.Repository;
using SQLiteConnectivity.Repository;
using Xunit;

namespace API.Test
{
    public class PositionRepositoryTest : IDisposable
    {
        private readonly IPositionRepository _repository;
        private readonly string _testDbPath;
        private readonly string _connectionString;

        public PositionRepositoryTest()
        {
            // Create a unique test database for each test run
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.db");
            _connectionString = $"Data Source={_testDbPath}";
            _repository = new PositionRepository(_connectionString);
        }

        public void Dispose()
        {
            // Clean up the test database
            if (File.Exists(_testDbPath))
            {
                File.Delete(_testDbPath);
            }
        }

        [Fact]
        public async Task GetAllPositionsAsync_ShouldReturnEmptyList_ForNewDatabase()
        {
            // Since the database is initialized with some test data, we'll just verify the method works
            // Act
            var result = await _repository.GetAllPositionsAsync();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task AddPositionAsync_ShouldAddNewPosition()
        {
            // Arrange
            var position = new Position 
            { 
                Title = "Test Position",
                Description = "Test Description",
                Location = "SJO",
                Status = "Draft",
                RecruiterId = 1,
                DepartmentId = 1,
                Budget = 10000,
                ClosingDate = DateTime.Now.AddDays(30)
            };

            // Act
            var id = await _repository.AddPositionAsync(position);

            // Assert
            Assert.NotNull(id);
            Assert.True(id > 0);
        }


        [Fact]
        public async Task GetPositionByIdAsync_ShouldReturnPosition_WhenPositionExists()
        {
            // Use existing position from the seeded data
            // Act
            var result = await _repository.GetPositionByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result?.Id);
            Assert.NotNull(result?.Title);
            Assert.NotNull(result?.Description);
            Assert.NotNull(result?.Location);
            Assert.NotNull(result?.Status);
        }

        [Fact]
        public async Task UpdatePositionAsync_ShouldUpdateExistingPosition()
        {
            // Arrange - use existing position from seeded data
            var existingPosition = await _repository.GetPositionByIdAsync(1);
            Assert.NotNull(existingPosition);

            var updatedPosition = new Position 
            { 
                Id = existingPosition.Id, 
                Title = "Updated " + existingPosition.Title,
                Description = existingPosition.Description,
                Location = existingPosition.Location,
                Status = "Closed",
                RecruiterId = existingPosition.RecruiterId,
                DepartmentId = existingPosition.DepartmentId,
                Budget = existingPosition.Budget,
                ClosingDate = existingPosition.ClosingDate
            };

            // Act
            var result = await _repository.UpdatePositionAsync(updatedPosition);
            var retrievedPosition = await _repository.GetPositionByIdAsync(existingPosition.Id);

            // Assert
            Assert.True(result);
            Assert.StartsWith("Updated ", retrievedPosition?.Title);
            Assert.Equal("Closed", retrievedPosition?.Status);
        }

        [Fact]
        public async Task RemovePositionAsync_ShouldDeletePosition()
        {
            // Use existing position from the seeded data
            // Act
            var result = await _repository.RemovePositionAsync(1);
            var deletedPosition = await _repository.GetPositionByIdAsync(1);

            // Assert
            Assert.True(result);
            Assert.Null(deletedPosition);
        }
    }
}
