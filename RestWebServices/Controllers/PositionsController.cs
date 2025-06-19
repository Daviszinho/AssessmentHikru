using Microsoft.AspNetCore.Mvc;
using Lib.Repository.Entities;
using Lib.Repository.Repository;
using Microsoft.Extensions.Logging;
using System.Data;
using Microsoft.Data.Sqlite;

namespace RestWebServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        private readonly ILogger<PositionsController> _logger;
        private readonly IPositionRepository _positionRepository;
        private string _timestamp => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]";

        public PositionsController(ILogger<PositionsController> logger, IPositionRepository positionRepository)
        {
            _logger = logger;
            _positionRepository = positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
            _logger.LogInformation("PositionsController initialized");
        }

        // GET: api/positions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Position>>> GetPositions()
        {
            try
            {
                _logger.LogInformation($"{_timestamp} [INFO] Getting all positions");
                var positions = await _positionRepository.GetAllPositionsAsync();
                _logger.LogInformation($"{_timestamp} [INFO] Retrieved {positions?.Count() ?? 0} positions");
                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_timestamp} [ERROR] Error getting positions");
                return StatusCode(500, new { message = "An error occurred while retrieving positions", error = ex.Message });
            }
        }

        // GET: api/positions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Position>> GetPosition(int id)
        {
            _logger.LogInformation($"{_timestamp} [INFO] Getting position with ID: {id}");
            
            try
            {
                var position = await _positionRepository.GetPositionByIdAsync(id);
                
                if (position == null)
                {
                    _logger.LogWarning($"{_timestamp} [WARN] Position with ID {id} not found");
                    return NotFound(new { message = $"Position with ID {id} not found" });
                }

                _logger.LogInformation($"{_timestamp} [INFO] Retrieved position with ID: {id}");
                return position;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_timestamp} [ERROR] Error getting position with ID {id}");
                return StatusCode(500, new { message = $"An error occurred while retrieving position with ID {id}", error = ex.Message });
            }
        }

        // POST: api/positions
        [HttpPost]
        public async Task<ActionResult<Position>> CreatePosition([FromBody] Position position)
        {
            _logger.LogInformation($"{_timestamp} [INFO] Creating new position");
            
            if (position == null)
            {
                _logger.LogError($"{_timestamp} [ERROR] Position data is null");
                return BadRequest(new { message = "Position data is required" });
            }

            try
            {
                _logger.LogInformation($"{_timestamp} [INFO] Starting to create new position");
                
                // Basic validation
                if (position == null)
                {
                    _logger.LogError($"{_timestamp} [ERROR] Position data is null");
                    return BadRequest(new { message = "Position data is required" });
                }

                // Log the incoming position data (be careful with sensitive data in production)
                _logger.LogDebug($"{_timestamp} [DEBUG] Position data: {System.Text.Json.JsonSerializer.Serialize(position)}");

                // Clear the ID to ensure we're creating a new record
                position.Id = 0;
                
                _logger.LogInformation($"{_timestamp} [INFO] Calling repository to add position");
                var newId = await _positionRepository.AddPositionAsync(position);
                
                if (newId.HasValue)
                {
                    _logger.LogInformation($"{_timestamp} [INFO] Successfully created position with ID: {newId}");
                    
                    // Retrieve the created position to return it
                    _logger.LogDebug($"{_timestamp} [DEBUG] Retrieving created position with ID: {newId}");
                    var createdPosition = await _positionRepository.GetPositionByIdAsync(newId.Value);
                    
                    if (createdPosition == null)
                    {
                        _logger.LogError($"{_timestamp} [ERROR] Failed to retrieve created position with ID: {newId}");
                        return StatusCode(500, new { message = "Failed to retrieve created position" });
                    }
                    
                    _logger.LogInformation($"{_timestamp} [INFO] Successfully retrieved created position with ID: {newId}");
                    return CreatedAtAction(nameof(GetPosition), new { id = newId }, createdPosition);
                }
                
                _logger.LogError($"{_timestamp} [ERROR] Failed to create position - repository returned null ID");
                return StatusCode(500, new { message = "Failed to create position - no ID returned" });
            }
            catch (SqliteException sqlEx) when (sqlEx.SqliteErrorCode == 19) // SQLITE_CONSTRAINT
            {
                _logger.LogError(sqlEx, $"{_timestamp} [ERROR] Constraint violation while creating position");
                return BadRequest(new { message = "A database constraint was violated", error = sqlEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_timestamp} [ERROR] Error creating position");
                return StatusCode(500, new { message = "An error occurred while creating the position", error = ex.Message });
            }
        }

        // PUT: api/positions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePosition(int id, [FromBody] Position position)
        {
            _logger.LogInformation($"{_timestamp} [INFO] Updating position with ID: {id}");
            
            if (position == null)
            {
                _logger.LogWarning($"{_timestamp} [WARN] Update position called with null position data");
                return BadRequest(new { message = "Position data is required" });
            }
            
            if (id != position.Id)
            {
                _logger.LogWarning($"{_timestamp} [WARN] ID in URL ({id}) does not match ID in the request body ({position.Id})");
                return BadRequest(new { message = "ID in URL does not match ID in the request body" });
            }

            try
            {
                _logger.LogDebug($"{_timestamp} [DEBUG] Position update data: {System.Text.Json.JsonSerializer.Serialize(position)}");
                
                _logger.LogInformation($"{_timestamp} [INFO] Calling repository to update position");
                var success = await _positionRepository.UpdatePositionAsync(position);
                
                if (!success)
                {
                    _logger.LogWarning($"{_timestamp} [WARN] Position with ID {id} not found or update failed");
                    return NotFound(new { message = "Position not found or update failed" });
                }
                
                _logger.LogInformation($"{_timestamp} [INFO] Successfully updated position with ID: {id}");
                return Ok(position);
            }
            catch (SqliteException sqlEx) when (sqlEx.SqliteErrorCode == 19) // SQLITE_CONSTRAINT
            {
                _logger.LogError(sqlEx, $"{_timestamp} [ERROR] Constraint violation while updating position with ID {id}");
                return BadRequest(new { message = "A database constraint was violated", error = sqlEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_timestamp} [ERROR] Error updating position with ID {id}");
                return StatusCode(500, new { message = "An error occurred while updating the position", error = ex.Message });
            }
        }

        // DELETE: api/positions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePosition(int id)
        {
            _logger.LogInformation($"{_timestamp} [INFO] Attempting to delete position with ID: {id}");
            
            try
            {
                _logger.LogInformation($"{_timestamp} [INFO] Calling repository to delete position with ID: {id}");
                var success = await _positionRepository.RemovePositionAsync(id);
                
                if (success)
                {
                    _logger.LogInformation($"{_timestamp} [INFO] Successfully deleted position with ID: {id}");
                    return NoContent();
                }
                
                _logger.LogWarning($"{_timestamp} [WARN] Position with ID {id} not found for deletion");
                return NotFound(new { message = $"Position with ID {id} not found" });
            }
            catch (NotImplementedException ex)
            {
                _logger.LogError(ex, $"{_timestamp} [ERROR] Delete operation not implemented");
                return StatusCode(501, new { message = "Position deletion is not yet implemented", error = ex.Message });
            }
            catch (SqliteException sqlEx) when (sqlEx.SqliteErrorCode == 19) // SQLITE_CONSTRAINT
            {
                _logger.LogError(sqlEx, $"{_timestamp} [ERROR] Constraint violation while deleting position with ID {id}");
                return BadRequest(new { message = "Cannot delete position due to existing references", error = sqlEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_timestamp} [ERROR] Error deleting position with ID {id}");
                return StatusCode(500, new { message = $"An error occurred while deleting the position with ID {id}", error = ex.Message });
            }
        }
    }
}
