using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EMTestTask.Configs;
using EMTestTask.Models;

namespace EMTestTask.Services
{
    public interface IAdvertisingService
    {
        AdvertisingServiceUpload LoadFromFile(Stream fileStream);
        IEnumerable<string> FindSites(string location);
        AdvertisingServiceUpload Reset();
    }

    public class AdvertisingService : IAdvertisingService
    {
        private readonly ILogger _log;
        private readonly AdvertisingSetting _settings;

        private volatile Dictionary<string, HashSet<string>> _locationIndex;

        public AdvertisingService(
            ILogger<AdvertisingService> logger,
            IOptions<AdvertisingSetting> settings
            )
        {
            _log = logger;
            _settings = settings.Value;
            _locationIndex = [];
            Reset();
        }

        public AdvertisingServiceUpload LoadFromFile(Stream fileStream)
        {
            var result = new AdvertisingServiceUpload();

            var newIndex = new Dictionary<string, HashSet<string>>();

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

                    var siteName = parts[0].Trim();
                    var locations = parts[1].Split(',')
                        .Select(l => l.Trim())
                        .Where(l => !string.IsNullOrEmpty(l));

                    foreach (var loc in locations)
                    {
                        var location = loc.StartsWith('/') ? loc : "/" + loc;
                        if (!newIndex.TryGetValue(location, out var siteList))
                        {
                            siteList = new HashSet<string>();
                            newIndex[location] = siteList;
                        }
                        if (!siteList.Contains(siteName))
                        {
                            siteList.Add(siteName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result.Result = -1;
                result.Message = $"{e.Message} | {e.StackTrace}";
            }
            _locationIndex = newIndex;
            return result;
        }

        public IEnumerable<string> FindSites(string location)
        {
            var normalizedLocation = location.StartsWith('/') ? location : "/" + location;
            var prefixes = GetPrefixes(normalizedLocation);
            var currentIndex = _locationIndex;
            var resultSet = new HashSet<string>();

            foreach (var prefix in prefixes)
            {
                if (currentIndex.TryGetValue(prefix, out var sites))
                {
                    foreach (var site in sites)
                    {
                        resultSet.Add(site);
                    }
                }
            }
            return resultSet;
        }

        private List<string> GetPrefixes(string location)
        {
            var parts = location.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var prefixes = new List<string>();
            var current = new StringBuilder();

            for (int i = 0; i < parts.Length; i++)
            {
                current.Append('/');
                current.Append(parts[i]);
                prefixes.Add(current.ToString());
            }
            return prefixes;
        }

        public AdvertisingServiceUpload Reset()
        {
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
