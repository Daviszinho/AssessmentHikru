using Microsoft.AspNetCore.Mvc;
using Lib.Repository.Entities;
using Lib.Repository.Repository;
using Microsoft.Extensions.Logging;

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
            var positions = await _positionRepository.GetAllPositionsAsync();
            return Ok(positions);
        }

        // GET: api/positions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Position>> GetPosition(int id)
        {
            var position = await _positionRepository.GetPositionByIdAsync(id);
            
            if (position == null)
            {
                return NotFound();
            }

            return position;
        }

        // POST: api/positions
        [HttpPost]
        public async Task<ActionResult<Position>> CreatePosition([FromBody] Position position)
        {
            try
            {
                Console.WriteLine($"{_timestamp} [INFO] Creating new position");
                
                // Basic validation
                if (position == null)
                {
                    Console.WriteLine($"{_timestamp} [ERROR] Position data is null");
                    return BadRequest(new { message = "Position data is required" });
                }

                // Clear the ID to ensure we're creating a new record
                position.Id = 0;
                
                // Call the repository to create the position
                var newId = await _positionRepository.AddPositionAsync(position);
                
                if (newId.HasValue)
                {
                    // Get the newly created position to return
                    var createdPosition = await _positionRepository.GetPositionByIdAsync(newId.Value);
                    if (createdPosition == null)
                    {
                        _logger.LogError("Failed to retrieve created position with ID: {PositionId}", newId);
                        return StatusCode(500, new { message = "Position created but could not be retrieved" });
                    }
                    _logger.LogInformation("Successfully created position with ID: {PositionId}", newId);
                    return CreatedAtAction(nameof(GetPosition), new { id = newId }, createdPosition);
                }
                
                _logger.LogError("Failed to create position");
                return StatusCode(500, new { message = "Failed to create position" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{_timestamp} [ERROR] Error creating position: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, "An error occurred while creating the position");
            }
        }

        // PUT: api/positions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePosition(int id, [FromBody] Position position)
        {
            if (id != position.Id)
            {
                return BadRequest("ID in URL does not match ID in the request body");
            }

            try
            {
                var success = await _positionRepository.UpdatePositionAsync(position);
                if (!success)
                {
                    return NotFound(new { message = "Position not found or update failed" });
                }
                return Ok(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating position with ID {PositionId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the position", error = ex.Message });
            }
        }

        // DELETE: api/positions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePosition(int id)
        {
            try
            {
                Console.WriteLine($"{_timestamp} [INFO] Attempting to delete position with ID: {id}");
                var success = await _positionRepository.RemovePositionAsync(id);
                if (success)
                {
                    Console.WriteLine($"{_timestamp} [SUCCESS] Successfully deleted position with ID: {id}");
                    return NoContent();
                }
                Console.WriteLine($"{_timestamp} [WARN] Position with ID {id} not found");
                return NotFound();
            }
            catch (NotImplementedException ex)
            {
                var errorMsg = "Position deletion is not yet implemented";
                Console.WriteLine($"{_timestamp} [ERROR] {errorMsg}");
                Console.WriteLine($"{_timestamp} [EXCEPTION] {ex}");
                return StatusCode(501, errorMsg);
            }
            catch (Exception ex)
            {
                var errorMsg = $"An error occurred while deleting position with ID {id}";
                Console.WriteLine($"{_timestamp} [ERROR] {errorMsg}");
                Console.WriteLine($"{_timestamp} [EXCEPTION] {ex}");
                return StatusCode(500, errorMsg);
            }
        }
    }
}
