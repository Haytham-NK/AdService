using Microsoft.AspNetCore.Mvc;
using AdService.Services;

namespace AdService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdsController : ControllerBase
    {
        private readonly AdIndexService _adIndexService;
        private readonly IWebHostEnvironment _environment;

        public AdsController(AdIndexService adIndexService, IWebHostEnvironment environment)
        {
            _adIndexService = adIndexService ?? throw new ArgumentNullException(nameof(adIndexService));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        [HttpPost("load")]
        public IActionResult Load([FromQuery] string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return BadRequest(new { error = "Specify parameter filePath." });
            }

            try
            {
                var fullPath = ResolveFilePath(filePath);
                _adIndexService.LoadFromFile(fullPath);
                return Ok(new { message = "File loaded.", path = fullPath });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { error = ex.Message, path = ex.FileName });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return BadRequest(new { error = "Specify parameter location." });
            }

            var result = _adIndexService.Search(location);
            return Ok(result);
        }

        private string ResolveFilePath(string filePath)
        {
            return Path.IsPathRooted(filePath)
                ? filePath
                : Path.Combine(_environment.ContentRootPath, filePath);
        }
    }
}
