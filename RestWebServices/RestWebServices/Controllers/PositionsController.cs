using Microsoft.AspNetCore.Mvc;
using Lib.Repository.Entities;
using Lib.Repository.Repository;

namespace RestWebServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        private readonly PositionRepository _positionRepository;

        public PositionsController(PositionRepository positionRepository)
        {
            _positionRepository = positionRepository;
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
        public async Task<ActionResult<Position>> PostPosition(Position position)
        {
            try
            {
                var success = await _positionRepository.AddPositionAsync(position);
                if (success)
                {
                    return CreatedAtAction(nameof(GetPosition), new { id = position.PositionId }, position);
                }
                return BadRequest("Failed to create position");
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, "Position creation is not yet implemented");
            }
        }

        // PUT: api/positions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPosition(int id, Position position)
        {
            if (id != position.PositionId)
            {
                return BadRequest("Position ID mismatch");
            }

            try
            {
                var success = await _positionRepository.UpdatePositionAsync(position);
                if (success)
                {
                    return NoContent();
                }
                return NotFound();
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, "Position update is not yet implemented");
            }
        }

        // DELETE: api/positions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePosition(int id)
        {
            try
            {
                var success = await _positionRepository.RemovePositionAsync(id);
                if (success)
                {
                    return NoContent();
                }
                return NotFound();
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, "Position deletion is not yet implemented");
            }
        }
    }
}
