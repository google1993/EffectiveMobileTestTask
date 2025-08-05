using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EMTestTask.Configs;
using EMTestTask.Models;
using EMTestTask.Logic;

namespace EMTestTask.Services
{
    public interface IAdvertisingService
    {
        AdvertisingServiceUpload LoadFromFile(Stream fileStream);
        IEnumerable<string> FindAgents(string location);
        AdvertisingServiceUpload Reset();
    }

    public class AdvertisingService : IAdvertisingService
    {
        private readonly ILogger _log;
        private readonly AdvertisingSetting _settings;

        private volatile AdvertizingMap _map;

        public AdvertisingService(
            ILogger<AdvertisingService> logger,
            IOptions<AdvertisingSetting> settings
            )
        {
            _log = logger;
            _settings = settings.Value;
            _map = new AdvertizingMap();
            Reset();
        }

        public AdvertisingServiceUpload LoadFromFile(Stream fileStream)
        {
            _log.LogInformation($"Start load new map.");

            var result = new AdvertisingServiceUpload();

            var newMap = new AdvertizingMap();

            try
            {
                using var reader = new StreamReader(fileStream);

                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    result.LinesTotal++;
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        result.LinesErrorNums.Add(result.LinesTotal);
                        continue;
                    }
                    var parts = line.Split(':', 2);
                    if (parts.Length < 2)
                    {
                        result.LinesErrorNums.Add(result.LinesTotal);
                        continue;
                    }

                    var agentName = parts[0].Trim();
                    var locations = parts[1].Split(',')
                        .Select(l => l.Trim())
                        .Where(l => !string.IsNullOrEmpty(l));

                    foreach (var loc in locations)
                    {
                        newMap.Add(loc, agentName);
                    }
                }
            }
            catch (Exception e)
            {
                result.Result = -1;
                result.Message = $"{e.Message} | {e.StackTrace}";
                _log.LogError("Can't correct load map: {}", result.Message);
            }
            _map = newMap;
            if (result.Result >= 0)
            {
                _log.LogInformation($"Load new map complete.");
            }
            return result;
        }

        public IEnumerable<string> FindAgents(string location)
        {
            _log.LogInformation("Search by location: {}.", location);
            var result = _map.FindAgents(location);
            _log.LogInformation("Search result: {}.", result);
            return result;
        }

        public AdvertisingServiceUpload Reset()
        {
            _log.LogInformation($"Reset map to default.");
            var result = new AdvertisingServiceUpload();
            try
            {
                using var fileStream = new FileStream(_settings.MapFile, FileMode.Open, FileAccess.Read);
                result = LoadFromFile(fileStream);
            }
            catch (Exception e)
            {
                string errMsg = (e.Message ?? "No message") + "\n" + (e.StackTrace ?? "No stack trace");
                _log.LogWarning("{Msg}", errMsg);
            }
            return result;
        }
    }
}
