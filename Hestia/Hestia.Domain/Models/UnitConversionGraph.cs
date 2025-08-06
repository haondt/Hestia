using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Persistence.Models;

namespace Hestia.Domain.Models
{
    public class UnitConversionGraph
    {
        private readonly IReadOnlyDictionary<NormalizedString, IReadOnlyDictionary<NormalizedString, decimal>> _graph;
        private readonly Dictionary<(NormalizedString from, NormalizedString to), Result<decimal>> _convertCache = [];
        private bool? _hasCycleCache;

        private UnitConversionGraph(IReadOnlyDictionary<NormalizedString, IReadOnlyDictionary<NormalizedString, decimal>> graph)
        {
            _graph = graph;
        }

        public DetailedResult<decimal, string> Convert(NormalizedString fromUnit, NormalizedString toUnit, decimal amount)
        {
            var failedReason = new DetailedResult<decimal, string>($"No conversion path found from '{fromUnit.Value}' to '{toUnit.Value}'");
            var cacheKey = (fromUnit, toUnit);
            if (_convertCache.TryGetValue(cacheKey, out var cachedMultiplierResult))
            {
                if (cachedMultiplierResult.IsSuccessful)
                    return new(amount * cachedMultiplierResult.Value);
                return failedReason;
            }

            if (fromUnit == toUnit)
            {
                _convertCache[cacheKey] = 1m;
                return new(amount);
            }

            var conversionPath = FindConversionPath(fromUnit, toUnit);
            if (!conversionPath.IsSuccessful)
            {
                _convertCache[cacheKey] = new();
                return failedReason;
            }

            decimal multiplier = 1m;
            for (int i = 0; i < conversionPath.Value.Count - 1; i++)
            {
                var from = conversionPath.Value[i];
                var to = conversionPath.Value[i + 1];
                multiplier *= _graph[from][to];
            }

            var finalMultiplierResult = DetailedResult<decimal, string>.Succeed(multiplier);
            _convertCache[cacheKey] = finalMultiplierResult;
            return DetailedResult<decimal, string>.Succeed(amount * multiplier);
        }

        public bool HasCycle()
        {
            if (_hasCycleCache.HasValue)
                return _hasCycleCache.Value;

            var visited = new HashSet<NormalizedString>();

            foreach (var node in _graph.Keys)
            {
                if (!visited.Contains(node))
                {
                    if (HasCycleDFS(node, new(), visited))
                    {
                        _hasCycleCache = true;
                        return true;
                    }
                }
            }

            _hasCycleCache = false;
            return false;
        }

        private bool HasCycleDFS(NormalizedString node, Optional<NormalizedString> parent, HashSet<NormalizedString> visited)
        {
            visited.Add(node);

            if (_graph.TryGetValue(node, out var neighbors))
            {
                foreach (var neighbor in neighbors.Keys)
                {
                    if (!visited.Contains(neighbor))
                    {
                        if (HasCycleDFS(neighbor, node, visited))
                            return true;
                    }
                    else if (!parent.HasValue || neighbor != parent.Value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Result<List<NormalizedString>> FindConversionPath(NormalizedString fromUnit, NormalizedString toUnit)
        {
            var queue = new Queue<List<NormalizedString>>();
            var visited = new HashSet<NormalizedString>();

            queue.Enqueue([fromUnit]);
            visited.Add(fromUnit);

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var currentUnit = path[^1];

                if (currentUnit == toUnit)
                    return path;

                if (_graph.ContainsKey(currentUnit))
                {
                    foreach (var neighbor in _graph[currentUnit].Keys)
                    {
                        if (visited.Contains(neighbor))
                            continue;

                        visited.Add(neighbor);
                        var newPath = new List<NormalizedString>(path) { neighbor };
                        queue.Enqueue(newPath);
                    }
                }
            }

            return new();
        }

        public static (UnitConversionGraph Graph, List<string> Warnings) FromConversions(IEnumerable<UnitConversionDataModel> conversions)
        {
            var mutableGraph = new Dictionary<NormalizedString, Dictionary<NormalizedString, decimal>>();
            var warnings = new List<string>();
            foreach (var conversion in conversions)
                if (AddConversion(mutableGraph, conversion.NormalizedFromUnit, conversion.NormalizedToUnit, conversion.Multiplier) is { HasValue: true, Value: var warning })
                    warnings.Add(warning);

            // Convert mutableGraph to IReadOnlyDictionary for the immutable UnitConversionGraph instance
            var readOnlyGraph = mutableGraph.ToDictionary(
                entry => entry.Key,
                entry => (IReadOnlyDictionary<NormalizedString, decimal>)entry.Value
            );

            var graph = new UnitConversionGraph(readOnlyGraph);
            return (graph, warnings);
        }

        public static Optional<string> AddConversion(Dictionary<NormalizedString, Dictionary<NormalizedString, decimal>> graph, NormalizedString fromUnit, NormalizedString toUnit, decimal multiplier)
        {
            var warning = new Optional<string>();

            if (graph.TryGetValue(fromUnit, out var existingConversions))
            {
                if (existingConversions.ContainsKey(toUnit))
                    warning = new($"Conversion from '{fromUnit.Value}' to '{toUnit.Value}' already exists.");
            }
            else
                graph[fromUnit] = existingConversions = [];
            existingConversions[toUnit] = multiplier;

            if (graph.TryGetValue(toUnit, out var reverseConversions))
            {
                if (reverseConversions.ContainsKey(fromUnit))
                    warning = new($"Conversion from '{toUnit.Value}' to '{fromUnit.Value}' already exists.");
            }
            else
                graph[toUnit] = reverseConversions = [];
            reverseConversions[fromUnit] = 1 / multiplier;

            return warning;
        }

        public List<string> GetCompatibleUnits(NormalizedString fromUnit)
        {
            var compatibleUnits = new HashSet<NormalizedString>();
            var queue = new Queue<NormalizedString>();
            var visited = new HashSet<NormalizedString>();

            queue.Enqueue(fromUnit);
            visited.Add(fromUnit);

            while (queue.Count > 0)
            {
                var currentUnit = queue.Dequeue();
                compatibleUnits.Add(currentUnit);

                if (_graph.TryGetValue(currentUnit, out var neighbors))
                {
                    foreach (var neighbor in neighbors.Keys)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return compatibleUnits.Select(u => u.Value.ToLower()).ToList();
        }
    }
}
