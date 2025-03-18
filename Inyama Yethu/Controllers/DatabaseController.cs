using Microsoft.AspNetCore.Mvc;

namespace Inyama_Yethu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(IServiceProvider serviceProvider, ILogger<DatabaseController> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpGet("initialize")]
        public async Task<IActionResult> Initialize()
        {
            try
            {
                await DatabaseInitializer.InitializeAsync(_serviceProvider, _logger);
                return Ok(new { success = true, message = "Database initialized successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing database");
                return StatusCode(500, new { success = false, message = $"Error initializing database: {ex.Message}" });
            }
        }
    }
} 