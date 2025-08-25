using System.Collections.Concurrent;

namespace AdService.Services
{
    public class AdIndexService
    {
        private volatile ConcurrentDictionary<string, HashSet<string>> _index =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly object _reloadLock = new();

        public void LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            var newIndex = AdIndexFileParser.BuildIndexFromFile(filePath);
            lock (_reloadLock)
            {
                _index = newIndex;
            }
        }

        public IReadOnlyList<string> Search(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return Array.Empty<string>();

            location = NormalizeLocation(location);

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var prefix in EnumeratePrefixes(location))
            {
                if (_index.TryGetValue(prefix, out var set))
                {
                    foreach (var name in set)
                        result.Add(name);
                }
            }

            return result.OrderBy(s => s).ToArray();
        }

        internal static string NormalizeLocation(string loc)
        {
            loc = loc.Trim();

            while (loc.Contains("//"))
                loc = loc.Replace("//", "/");

            if (!loc.StartsWith('/'))
                loc = "/" + loc;

            if (loc.Length > 1 && loc.EndsWith('/'))
                loc = loc.TrimEnd('/');

            return loc;
        }

        private static IEnumerable<string> EnumeratePrefixes(string loc)
        {
            if (string.IsNullOrEmpty(loc) || loc == "/")
                yield break;

            int pos = 0;
            while (true)
            {
                pos = loc.IndexOf('/', pos + 1);
                if (pos == -1) break;
                yield return loc[..pos];
            }
            yield return loc;
        }
    }

    internal static class AdIndexFileParser
    {
        public static ConcurrentDictionary<string, HashSet<string>> BuildIndexFromFile(string filePath)
        {
            var newIndex = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var rawLine in File.ReadAllLines(filePath))
            {
                if (!TryParseLine(rawLine, out var name, out var locations))
                    continue;

                foreach (var loc in locations)
                {
                    var set = newIndex.GetOrAdd(loc, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                    set.Add(name);
                }
            }

            return newIndex;
        }

        private static bool TryParseLine(string? rawLine, out string name, out string[] locations)
        {
            name = string.Empty;
            locations = Array.Empty<string>();

            var line = rawLine?.Trim();
            if (string.IsNullOrEmpty(line)) return false;

            int colon = line.IndexOf(':');
            if (colon <= 0 || colon == line.Length - 1) return false;

            name = line[..colon].Trim();
            var right = line[(colon + 1)..].Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(right))
                return false;

            locations = right.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                             .Where(s => s.StartsWith('/'))
                             .Select(AdIndexService.NormalizeLocation)
                             .Distinct(StringComparer.OrdinalIgnoreCase)
                             .ToArray();

            return locations.Length > 0;
        }
    }
}
