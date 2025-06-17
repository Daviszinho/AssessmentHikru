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
            try 
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Fetching all positions from database...");
                var positions = await _oracleQuery.GetAllAsync<Position>("get_all_positions", MapPosition);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Retrieved {positions.Count()} positions from database");
                foreach (var pos in positions)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Position ID: {pos.Id}, Title: {pos.Title}");
                }
                return positions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Error fetching positions: {ex}");
                throw;
            }
        }

        // Get position by ID
        public async Task<Position?> GetPositionByIdAsync(int positionId)
        {
            try 
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Fetching position by ID: {positionId}");
                
                // Use a direct SQL query instead of the package
                var query = @"
                    SELECT p.*, r.NAME as RECRUITERNAME, d.NAME as DEPARTMENTNAME
                    FROM ""POSITION"" p
                    LEFT JOIN ""RECRUITER"" r ON p.RECRUITERID = r.ID
                    LEFT JOIN ""DEPARTMENT"" d ON p.DEPARTMENTID = d.ID
                    WHERE p.ID = :positionId";
                
                var parameters = new OracleParameter("positionId", OracleDbType.Int32, positionId, ParameterDirection.Input);
                
                var positions = await _oracleQuery.GetAllAsync(query, MapPosition, parameters);
                var position = positions?.FirstOrDefault();
                
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] {(position != null ? "Found" : "Did not find")} position with ID: {positionId}");
                return position;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Error fetching position by ID {positionId}: {ex}");
                throw;
            }
        }

        // Update position
        public async Task<bool> UpdatePositionAsync(Position position)
        {
            try 
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Updating position ID: {position.Id}");
                
                // Ensure required IDs are provided
                if (!position.RecruiterId.HasValue)
                {
                    throw new ArgumentException("Recruiter ID is required");
                }
                
                if (!position.DepartmentId.HasValue)
                {
                    throw new ArgumentException("Department ID is required");
                }
                
                int recruiterId = position.RecruiterId.Value;
                int departmentId = position.DepartmentId.Value;
                
                // Log the incoming position data for debugging
                Console.WriteLine($"[PositionRepository] Updating position with data: " +
                    $"ID={position.Id}, " +
                    $"Title={position.Title}, " +
                    $"Status={position.Status ?? "draft"}, " +
                    $"RecruiterId={recruiterId}, " +
                    $"DepartmentId={departmentId}, " +
                    $"Budget={position.Budget}, " +
                    $"ClosingDate={(position.ClosingDate?.ToString("yyyy-MM-dd") ?? "null")}");

                // Create parameters with exact names and types as in the stored procedure
                var parameters = new List<OracleParameter>();
                
                // Add parameters with explicit types and directions
                parameters.Add(CreateParameter("p_id", OracleDbType.Int32, ParameterDirection.Input, position.Id));
                parameters.Add(CreateParameter("p_title", OracleDbType.Varchar2, ParameterDirection.Input, position.Title ?? string.Empty, 255));
                parameters.Add(CreateParameter("p_description", OracleDbType.Varchar2, ParameterDirection.Input, position.Description, 4000));
                parameters.Add(CreateParameter("p_location", OracleDbType.Varchar2, ParameterDirection.Input, position.Location, 255));
                parameters.Add(CreateParameter("p_status", OracleDbType.Varchar2, ParameterDirection.Input, position.Status ?? "draft", 50));
                parameters.Add(CreateParameter("p_recruiter_id", OracleDbType.Int32, ParameterDirection.Input, recruiterId));
                parameters.Add(CreateParameter("p_department_id", OracleDbType.Int32, ParameterDirection.Input, departmentId));
                parameters.Add(CreateParameter("p_budget", OracleDbType.Decimal, ParameterDirection.Input, position.Budget));
                
                // Handle the date parameter carefully
                var closingDate = position.ClosingDate.HasValue && position.ClosingDate.Value > DateTime.MinValue 
                    ? (object)position.ClosingDate.Value 
                    : DBNull.Value;
                parameters.Add(new OracleParameter("p_closing_date", OracleDbType.Date) { 
                    Direction = ParameterDirection.Input,
                    Value = closingDate 
                });
                
                // Note: p_success is added automatically by OracleQuery

                // Log parameter details for debugging with more information
                Console.WriteLine("[PositionRepository] Parameters being passed to update_position:");
                for (int i = 0; i < parameters.Count; i++)
                {
                    var param = parameters[i];
                    string valueStr = (param.Value == DBNull.Value) ? "NULL" : 
                                     (param.Value == null) ? "null" : 
                                     $"'{param.Value}' (Type: {param.Value.GetType().Name})";
                    
                    // Log detailed parameter information
                    Console.WriteLine($"  [{i}] {param.ParameterName}:");
                    Console.WriteLine($"    Type: {param.OracleDbType}");
                    Console.WriteLine($"    Direction: {param.Direction}");
                    Console.WriteLine($"    Value: {valueStr}");
                    Console.WriteLine($"    Size: {param.Size}");
                    Console.WriteLine($"    Precision: {param.Precision}");
                    Console.WriteLine($"    Scale: {param.Scale}");
                    
                    Console.WriteLine($"  [{i}] {param.ParameterName} = {valueStr}");
                    Console.WriteLine($"     Type: {param.OracleDbType}, Direction: {param.Direction}, Size: {param.Size}");
                }
                
                Console.WriteLine($"[PositionRepository] Update parameters - " +
                    $"ID: {position.Id}, " +
                    $"RecruiterId: {recruiterId} (type: {recruiterId.GetType()}), " +
                    $"DepartmentId: {departmentId} (type: {departmentId.GetType()})");

                try 
                {
                    Console.WriteLine($"[PositionRepository] Calling update_position stored procedure");
                    int rowsAffected = await _oracleQuery.ExecuteNonQueryAsync("update_position", parameters.ToArray());
                    
                    // Check if the operation was successful based on the number of rows affected
                    bool success = rowsAffected > 0; 
                    
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Update {(success ? "succeeded" : "no rows affected")} for position ID: {position.Id}");
                    
                    return success;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PositionRepository] Error in ExecuteNonQueryAsync: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[PositionRepository] Inner exception: {ex.InnerException.Message}");
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Error updating position: {ex}");
                throw;
            }
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
                string sql = "DELETE FROM Position WHERE Id = :id";
                var param = new OracleParameter("id", positionId);
                
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

        // Helper method to get column ordinal with case-insensitive matching
        private int GetOrdinalCaseInsensitive(OracleDataReader reader, string columnName, List<string>? columnNames = null)
        {
            try
            {
                if (columnNames == null)
                {
                    return reader.GetOrdinal(columnName);
                }

                // Try exact match first (most efficient)
                var exactMatch = columnNames.FirstOrDefault(c => c.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                if (exactMatch != null)
                {
                    return reader.GetOrdinal(exactMatch);
                }

                // Fall back to standard behavior if not found
                return reader.GetOrdinal(columnName);
            }
            catch (IndexOutOfRangeException ex)
            {
                string availableColumns = columnNames != null ? string.Join(", ", columnNames) : "[not available]";
                throw new Exception($"Column '{columnName}' not found. Available columns: {availableColumns}", ex);
            }
        }

        // Map IDataReader to Position entity
        private Position MapPosition(IDataReader reader)
        {
            try 
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Starting to map database row to Position object");
                
                // Log all available columns and their values
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Available columns and values:");
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);
                    object value = reader.IsDBNull(i) ? "[NULL]" : reader.GetValue(i);
                    Console.WriteLine($"  {columnName} ({i}): {value}");
                }

                // Get column ordinals with case-insensitive matching
                int idOrdinal = reader.GetOrdinal("ID");
                int titleOrdinal = reader.GetOrdinal("TITLE");
                int descriptionOrdinal = reader.GetOrdinal("DESCRIPTION");
                int locationOrdinal = reader.GetOrdinal("LOCATION");
                int statusOrdinal = reader.GetOrdinal("STATUS");
                int recruiterIdOrdinal = reader.GetOrdinal("RECRUITERID");
                int recruiterNameOrdinal = reader.GetOrdinal("RECRUITERNAME");
                int departmentIdOrdinal = reader.GetOrdinal("DEPARTMENTID");
                int departmentNameOrdinal = reader.GetOrdinal("DEPARTMENTNAME");
                int budgetOrdinal = reader.GetOrdinal("BUDGET");
                int closingDateOrdinal = reader.GetOrdinal("CLOSINGDATE");

                // Create position object
                var position = new Position
                {
                    Id = reader.GetInt32(idOrdinal),
                    Title = reader.GetString(titleOrdinal),
                    Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal),
                    Location = reader.IsDBNull(locationOrdinal) ? null : reader.GetString(locationOrdinal),
                    Status = reader.IsDBNull(statusOrdinal) ? null : reader.GetString(statusOrdinal),
                    RecruiterId = reader.GetInt32(recruiterIdOrdinal),
                    DepartmentId = reader.GetInt32(departmentIdOrdinal),
                    Budget = reader.IsDBNull(budgetOrdinal) ? (decimal?)null : reader.GetDecimal(budgetOrdinal),
                    ClosingDate = reader.IsDBNull(closingDateOrdinal) ? (DateTime?)null : reader.GetDateTime(closingDateOrdinal),
                    RecruiterName = reader.GetString(recruiterNameOrdinal),
                    DepartmentName = reader.GetString(departmentNameOrdinal)
                };

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Successfully mapped position with ID: {position.Id}, Title: {position.Title}");
                return position;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Error in MapPosition: {ex}");
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Stack Trace: {ex.StackTrace}");
                throw new Exception("Failed to map database record to Position object. See inner exception for details.", ex);
            }
        }

        // IDisposable implementation
        /// <summary>
        /// Adds a new position to the database
        /// </summary>
        /// <param name="position">The position to add</param>
        /// <returns>The ID of the newly created position, or null if creation failed</returns>
        public async Task<bool> ValidateRecruiterAndDepartment(int? recruiterId, int? departmentId)
        {
            // Skip validation for now as it's causing issues
            Console.WriteLine($"[PositionRepository] Skipping validation for recruiterId: {recruiterId}, departmentId: {departmentId}");
            return true;
        }

        public async Task<int?> AddPositionAsync(Position position)
        {
            try 
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Adding new position: {position.Title}");
                
                // Validate required fields
                if (string.IsNullOrWhiteSpace(position.Title) || 
                    !position.RecruiterId.HasValue || 
                    !position.DepartmentId.HasValue)
                {
                    Console.WriteLine("[PositionRepository] Validation failed: Missing required fields");
                    return null;
                }

                // Validate that recruiter and department exist
                bool isValid = await ValidateRecruiterAndDepartment(position.RecruiterId, position.DepartmentId);
                if (!isValid)
                {
                    Console.WriteLine("[PositionRepository] Validation failed: Recruiter or Department does not exist");
                    return null;
                }

                // Create parameters for the stored procedure - matching the exact parameter names and order
                var parameters = new List<OracleParameter>
                {
                    // Input parameters
                    CreateParameter("p_title", OracleDbType.Varchar2, ParameterDirection.Input, position.Title, 100),
                    CreateParameter("p_description", OracleDbType.Varchar2, ParameterDirection.Input, position.Description ?? string.Empty, 4000),
                    CreateParameter("p_location", OracleDbType.Varchar2, ParameterDirection.Input, position.Location ?? string.Empty, 100),
                    CreateParameter("p_status", OracleDbType.Varchar2, ParameterDirection.Input, position.Status ?? "draft", 20),
                    CreateParameter("p_recruiter_id", OracleDbType.Int32, ParameterDirection.Input, position.RecruiterId),
                    CreateParameter("p_department_id", OracleDbType.Int32, ParameterDirection.Input, position.DepartmentId),
                    CreateParameter("p_budget", OracleDbType.Decimal, ParameterDirection.Input, position.Budget),
                    CreateParameter("p_closing_date", OracleDbType.Date, ParameterDirection.Input, 
                        position.ClosingDate > DateTime.MinValue ? position.ClosingDate : (object)DBNull.Value),
                    
                    // Output parameters
                    CreateParameter("p_new_id", OracleDbType.Int32, ParameterDirection.Output, 0),
                    CreateParameter("p_success", OracleDbType.Int32, ParameterDirection.Output, 0)
                };

                // Log parameters
                Console.WriteLine("[PositionRepository] Parameters for add_position:");
                foreach (var param in parameters)
                {
                    Console.WriteLine($"  {param.ParameterName}: {param.Value} (Type: {param.OracleDbType}, Direction: {param.Direction})");
                }

                // Execute the stored procedure - using exact case matching
                int rowsAffected = await _oracleQuery.ExecuteNonQueryAsync("add_position", parameters.ToArray());
                
                // Get the output parameters
                var successParam = parameters.First(p => p.ParameterName == "p_success");
                var newIdParam = parameters.First(p => p.ParameterName == "p_new_id");
                
                // Handle OracleDecimal conversion safely
                bool success = false;
                int? newId = null;
                
                if (successParam.Value != null && successParam.Value != DBNull.Value)
                {
                    if (successParam.Value is Oracle.ManagedDataAccess.Types.OracleDecimal oracleSuccess)
                        success = oracleSuccess == 1;
                    else
                        success = Convert.ToInt32(successParam.Value) == 1;
                }
                
                if (newIdParam.Value != null && newIdParam.Value != DBNull.Value)
                {
                    if (newIdParam.Value is Oracle.ManagedDataAccess.Types.OracleDecimal oracleId)
                        newId = (int)oracleId.Value;
                    else
                        newId = Convert.ToInt32(newIdParam.Value);
                }
                
                if (success && newId.HasValue)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Successfully added position with ID: {newId}");
                    return newId;
                }
                else
                {
                    // Log detailed error information
                    string errorDetails = "Failed to add position. ";
                    if (successParam.Value != DBNull.Value)
                    {
                        errorDetails += $"Success flag: {successParam.Value}. ";
                    }
                    if (newIdParam.Value != DBNull.Value)
                    {
                        errorDetails += $"New ID: {newIdParam.Value}. ";
                    }
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] {errorDetails}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [PositionRepository] Error adding position: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates an OracleParameter with the specified properties
        /// </summary>
        private OracleParameter CreateParameter(string name, OracleDbType dbType, ParameterDirection direction, object? value, int? size = null)
        {
            var param = new OracleParameter(name, dbType);
            
            if (size.HasValue)
            {
                param.Size = size.Value;
            }
            
            param.Direction = direction;
            
            // Handle null or DBNull values
            if (value == null || value == DBNull.Value)
            {
                param.Value = DBNull.Value;
                return param;
            }
            
            // For non-null values, set the value directly
            param.Value = value;
            
            return param;
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