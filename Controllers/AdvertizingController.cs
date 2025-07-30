using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using EMTestTask.Services;
using System.IO;
using System.Text;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("adv")]
    public class AdvertizingController(
        ILogger<AdvertizingController> log,
        IAdvertisingService advService
        ) : ControllerBase
    {
        private readonly ILogger<AdvertizingController> _log = log;
        private readonly IAdvertisingService _advService = advService;

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is not provided or empty");

            using (var stream = file.OpenReadStream())
            {
                _advService.LoadFromFile(stream);
            }
            return Ok(new { Result = 0 });
        }

        [HttpGet("search")]
        public IActionResult Search(string location)
        {
            if (string.IsNullOrEmpty(location))
                return BadRequest("Location is required");

            var sites = _advService.FindSites(location);
            return Ok(sites);
        }

        [HttpGet("reset")]
        public IActionResult Reset()
        {
            _advService.Reset();
            return Ok(new { Result = 0 });
        }
    }
}
