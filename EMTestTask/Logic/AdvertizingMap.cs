using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EMTestTask.Logic
{
    public class AdvertizingMap
    {
        private Dictionary<string, HashSet<string>> map = [];

        public void Add(string region, string agent)
        {
            var prefixes = GetPrefixes(region);

            var agents = new HashSet<string>();
            for (int i = 0; i < prefixes.Count; i++)
            {
                if (map.ContainsKey(prefixes[i]))
                {
                    agents = new HashSet<string>(map[prefixes[i]]);
                }
                else
                {
                    map.Add(prefixes[i], new HashSet<string>(agents));
                }
            }
            foreach (var item in map)
            {
                if (item.Key.StartsWith(region))
                {
                    item.Value.Add(agent);
                }
            }
        }

        public IEnumerable<string> FindAgents(string location)
        {
            var normalizedLocation = location.StartsWith('/') ? location : "/" + location;
            
            if (map.TryGetValue(normalizedLocation, out var agents))
            {
                return agents;
            }

            var prefixes = GetPrefixes(normalizedLocation);

            for (int i = prefixes.Count - 1; i >= 0; i--)
            {
                if (map.TryGetValue(prefixes[i], out agents))
                {
                    return agents;
                }
            }

            return [];
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
    }
}
