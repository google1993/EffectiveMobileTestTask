using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using EMTestTask.Services;
using EMTestTask.Models;

namespace EffectiveMobileTestTask.Controllers
{
    [ApiController]
    [Route("adv")]
    public class AdvertizingController(
        ILogger<AdvertizingController> log,
        IAdvertisingService advService,
        StatisticBotService statisticBotService
        ) : ControllerBase
    {
        private readonly ILogger<AdvertizingController> _log = log;
        private readonly IAdvertisingService _advService = advService;
        private readonly StatisticBotService _statisticBotService = statisticBotService;

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            _log.LogInformation("Upload File: {}", file.FileName);

            var result = new AdvertisingServiceUpload();
            if (file == null || file.Length == 0)
            {
                _log.LogWarning("Upload File: File is not provided or empty");
                result.Result = -1;
                result.Message = "File is not provided or empty";
                _ = Task.Run(() => _statisticBotService.UploadFile(result));
                return Ok(new { result });
            }
            try
            {
                _log.LogInformation("Upload File: {}", file.FileName);
                using (var stream = file.OpenReadStream())
                {
                    result = _advService.LoadFromFile(stream);
                }
            }
            catch (Exception e)
            {
                result.Result = -1;
                result.Message = (e.Message ?? "No message") + "\n" + (e.StackTrace ?? "No stack trace");
                _log.LogWarning("Can't correct upload file: {}", result.Message);
            }
            _ = Task.Run(() => _statisticBotService.UploadFile(result));
            return Ok(new { result });
        }

        [HttpGet("search")]
        public IActionResult Search(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                _log.LogWarning("Search: Location is required");
                _ = Task.Run(() => _statisticBotService.Search("", []));
                return Ok(new { Result = -1, Message = "Location is required" });
            }
            _log.LogInformation("Search: {}", location);
            var sites = _advService.FindAgents(location);
            _ = Task.Run(() => _statisticBotService.Search(location, sites));
            return Ok(new { Result = 0, Sites = sites });
        }

        [HttpGet("reset")]
        public IActionResult Reset()
        {
            var result = _advService.Reset();
            _ = Task.Run(() => _statisticBotService.Reset(result));
            return Ok(new { result });
        }
    }
}
