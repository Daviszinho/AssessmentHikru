using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Lib.Repository.Entities;
using Hikru.Assessment.OracleConnectivity;

namespace Lib.Repository.Repository
{
    public class PositionRepository : IDisposable
    {
        private readonly OracleQuery _oracleQuery;
        private bool _disposed = false;

        public PositionRepository()
        {
            _oracleQuery = new OracleQuery();
        }

        // Get all positions
        public async Task<IEnumerable<Position>> GetAllPositionsAsync()
        {
            return await _oracleQuery.GetAllAsync("position_pkg.get_all_positions", MapPosition);
        }

        // Get position by ID
        public async Task<Position?> GetPositionByIdAsync(int positionId)
        {
            return await _oracleQuery.GetByIdAsync("position_pkg.get_position_by_id", positionId, MapPosition);
        }

        // Add new position
        public Task<bool> AddPositionAsync(Position position)
        {
            // TODO: Implement AddPositionAsync using the appropriate OracleQuery method
            // This will need to be implemented once the OracleQuery class has the required method
            throw new NotImplementedException("AddPositionAsync is not yet implemented. Waiting for OracleQuery implementation.");
        }

        // Update position
        public Task<bool> UpdatePositionAsync(Position position)
        {
            // TODO: Implement UpdatePositionAsync using the appropriate OracleQuery method
            // This will need to be implemented once the OracleQuery class has the required method
            throw new NotImplementedException("UpdatePositionAsync is not yet implemented. Waiting for OracleQuery implementation.");
        }

        // Remove position
        public async Task<bool> RemovePositionAsync(int positionId)
        {
            if (positionId <= 0)
                throw new ArgumentException("Position ID must be greater than zero", nameof(positionId));

            try
            {
                Console.WriteLine($"[PositionRepository] Deleting position with ID: {positionId}");
                
                // Use named parameter format for Oracle
                string sql = "DELETE FROM Position WHERE PositionId = :positionId";
                var param = new OracleParameter("positionId", positionId);
                
                Console.WriteLine($"[PositionRepository] Executing: {sql}, ID: {positionId}");
                
                // Use the new ExecuteSqlAsync method for direct SQL execution
                int rowsAffected = await _oracleQuery.ExecuteSqlAsync(sql, param);
                bool success = rowsAffected > 0;
                
                Console.WriteLine($"[PositionRepository] Delete {(success ? "succeeded" : "did not affect any rows")} for ID: {positionId}");
                return success;
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error deleting position with ID {positionId}: {ex.Message}";
                Console.WriteLine($"[PositionRepository] {errorMsg}");
                Console.WriteLine($"[PositionRepository] Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[PositionRepository] Inner Exception: {ex.InnerException.Message}");
                }
                throw new Exception(errorMsg, ex);
            }
        }

        // Map OracleDataReader to Position entity
        private Position MapPosition(OracleDataReader reader)
        {
            return new Position
            {
                PositionId = reader.GetInt32(reader.GetOrdinal("POSITIONID")),
                Title = reader.GetString(reader.GetOrdinal("TITLE")),
                Description = reader.IsDBNull(reader.GetOrdinal("DESCRIPTION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPTION")),
                Location = reader.IsDBNull(reader.GetOrdinal("LOCATION")) ? null : reader.GetString(reader.GetOrdinal("LOCATION")),
                Status = reader.IsDBNull(reader.GetOrdinal("STATUS")) ? null : reader.GetString(reader.GetOrdinal("STATUS")),
                RecruiterId = reader.IsDBNull(reader.GetOrdinal("RECRUITERID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("RECRUITERID")),
                DepartmentId = reader.IsDBNull(reader.GetOrdinal("DEPARTMENTID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("DEPARTMENTID")),
                Budget = reader.IsDBNull(reader.GetOrdinal("BUDGET")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("BUDGET")),
                ClosingDate = reader.IsDBNull(reader.GetOrdinal("CLOSINGDATE")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CLOSINGDATE"))
            };
        }

        // IDisposable implementation
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
                    _oracleQuery?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}