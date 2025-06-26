using Microsoft.AspNetCore.Mvc;
using Lib.Repository.Entities;
using Lib.Repository.Repository.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace RestWebServices.Controllers
{
    [Route("api/positions")]
    [ApiController]
    public class PositionsCommandController : ControllerBase
    {
        private readonly ILogger<PositionsCommandController> _logger;
        private readonly IPositionCommandRepository _commandRepository;
        private string _timestamp => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]";

        public PositionsCommandController(
            ILogger<PositionsCommandController> logger,
            IPositionCommandRepository commandRepository)
        {
            _logger = logger;
            _commandRepository = commandRepository ?? throw new ArgumentNullException(nameof(commandRepository));
            _logger.LogInformation("PositionsCommandController initialized");
        }

        // POST: api/positions
        [HttpPost]
        public async Task<IActionResult> CreatePosition([FromBody] Position position)
        {
            _logger.LogInformation($"{_timestamp} [INFO] Creating new position");
            
            try
            {
                var id = await _commandRepository.AddPositionAsync(position);
                if (id.HasValue)
                {
                    _logger.LogInformation($"{_timestamp} [INFO] Position created with ID: {id}");
                    // Obtener el objeto creado y retornarlo
                    var queryRepo = HttpContext.RequestServices.GetService(typeof(Lib.Repository.Repository.Queries.IPositionQueryRepository)) as Lib.Repository.Repository.Queries.IPositionQueryRepository;
                    if (queryRepo != null)
                    {
                        var createdPosition = await queryRepo.GetPositionByIdAsync(id.Value);
                        return Ok(createdPosition);
                    }
                    // Fallback: retornar solo el id
                    return Ok(new { id });
                }
                
                _logger.LogError($"{_timestamp} [ERROR] Failed to create position");
                return StatusCode(500, new { message = "Failed to create position" });
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
            
            if (id != position.Id)
            {
                return BadRequest("ID in URL does not match ID in the request body");
            }

            try
            {
                var success = await _commandRepository.UpdatePositionAsync(position);
                if (success)
                {
                    _logger.LogInformation($"{_timestamp} [INFO] Position with ID {id} updated successfully");
                    // Fetch the updated position from the query repository and return it
                    // (We need to resolve the query repository here)
                    var queryRepo = HttpContext.RequestServices.GetService(typeof(Lib.Repository.Repository.Queries.IPositionQueryRepository)) as Lib.Repository.Repository.Queries.IPositionQueryRepository;
                    if (queryRepo != null)
                    {
                        var updatedPosition = await queryRepo.GetPositionByIdAsync(id);
                        return Ok(updatedPosition);
                    }
                    // Fallback: return the input position if query repo is not available
                    return Ok(position);
                }
                
                _logger.LogWarning($"{_timestamp} [WARN] Position with ID {id} not found for update");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_timestamp} [ERROR] Error updating position with ID: {id}");
                return StatusCode(500, new { message = "An error occurred while updating the position", error = ex.Message });
            }
        }

        // DELETE: api/positions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePosition(int id)
        {
            _logger.LogInformation($"{_timestamp} [INFO] Deleting position with ID: {id}");
            
            try
            {
                var success = await _commandRepository.RemovePositionAsync(id);
                if (success)
                {
                    _logger.LogInformation($"{_timestamp} [INFO] Position with ID {id} deleted successfully");
                    return NoContent();
                }
                
                _logger.LogWarning($"{_timestamp} [WARN] Position with ID {id} not found for deletion");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_timestamp} [ERROR] Error deleting position with ID: {id}");
                return StatusCode(500, new { message = "An error occurred while deleting the position", error = ex.Message });
            }
        }

        // This method is referenced in the CreatedAtAction call
        private IActionResult GetPosition(int id)
        {
            // This is just a placeholder for the CreatedAtAction to reference
            // The actual implementation is in the query controller
            return Ok();
        }
    }
}
