using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System;
using System.Net.Http.Json;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text;
using EffectiveMobileTestTask.Configs;
using YamlDotNet.Serialization;

namespace EMTestTask.Services
{
    public interface IAdvertisingService
    {
        void LoadFromFile(Stream fileStream);
        IEnumerable<string> FindSites(string location);
        void Reset();
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

        public void LoadFromFile(Stream fileStream)
        {
            var newIndex = new Dictionary<string, HashSet<string>>();
            using (var reader = new StreamReader(fileStream))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(':', 2);
                    if (parts.Length < 2) continue;

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
            _locationIndex = newIndex;
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

        public void Reset()
        {
            try
            {
                using var fileStream = new FileStream(_settings.MapFile, FileMode.Open, FileAccess.Read);
                LoadFromFile(fileStream);
            }
            catch (Exception e)
            {
                string errMsg = (e.Message ?? "No message") + "\n" + (e.StackTrace ?? "No stack trace");
                _log.LogWarning("{Msg}", errMsg);
            }
        }
    }
}
