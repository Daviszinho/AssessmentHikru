using Microsoft.AspNetCore.Mvc;
using Lib.Repository.Entities;
using Lib.Repository.Repository.Queries;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestWebServices.Controllers
{
    [Route("api/positions")]
    [ApiController]
    public class PositionsQueryController : ControllerBase
    {
        private readonly ILogger<PositionsQueryController> _logger;
        private readonly IPositionQueryRepository _queryRepository;
        private string _timestamp => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]";

        public PositionsQueryController(
            ILogger<PositionsQueryController> logger,
            IPositionQueryRepository queryRepository)
        {
            _logger = logger;
            _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
            _logger.LogInformation("PositionsQueryController initialized");
        }

        // GET: api/positions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Position>>> GetPositions()
        {
            try
            {
                _logger.LogInformation($"{_timestamp} [INFO] Getting all positions");
                //_logger.LogInformation("$[INFO] Getting all positions" + _queryRepository.GetConnectionString());
                var positions = await _queryRepository.GetAllPositionsAsync();
                _logger.LogInformation($"{_timestamp} [INFO] Retrieved {positions?.Count()} positions");
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
                var position = await _queryRepository.GetPositionByIdAsync(id);
                if (position == null)
                {
                    _logger.LogWarning($"{_timestamp} [WARN] Position with ID {id} not found");
                    return NotFound();
                }

                _logger.LogInformation($"{_timestamp} [INFO] Retrieved position with ID: {id}");
                return Ok(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_timestamp} [ERROR] Error getting position with ID: {id}");
                return StatusCode(500, new { message = $"An error occurred while retrieving position with ID {id}", error = ex.Message });
            }
        }
    }
}
