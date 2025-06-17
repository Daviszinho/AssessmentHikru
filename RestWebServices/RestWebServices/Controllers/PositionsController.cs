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
        private readonly PositionRepository _positionRepository;
        private string _timestamp => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]";

        public PositionsController(ILogger<PositionsController> logger, PositionRepository positionRepository)
        {
            _logger = logger;
            _positionRepository = positionRepository;
            Console.WriteLine($"{_timestamp} [INFO] PositionsController initialized");
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

        // POST: api/positions
        [HttpPost]
        public async Task<ActionResult<Position>> PostPosition([FromBody] Position position)
        {
            try
            {
                var success = await _positionRepository.AddPositionAsync(position);
                if (success)
                {
                    return CreatedAtAction(nameof(GetPosition), new { id = position.Id }, position);
                }
                return BadRequest(new { message = "Failed to create position" });
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, new { message = "Position creation is not yet implemented" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating position");
                return StatusCode(500, new { message = "An error occurred while creating the position", error = ex.Message });
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
