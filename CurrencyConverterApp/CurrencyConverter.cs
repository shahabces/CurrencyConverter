using Microsoft.Extensions.Caching.Memory;

namespace CurrencyConverterApp
{

    public class CurrencyConverter : ICurrencyConverter
    {
        private readonly Dictionary<string, Dictionary<string, double>> _graph;
        private readonly object _lockObject;

        // a dictionary as a simple cache
        private readonly Dictionary<string, double> _cache;

        public CurrencyConverter()
        {
            _graph = [];
            _lockObject = new();
            _cache = [];
        }

        public void ClearConfiguration()
        {
            lock (_lockObject)
            {
                _graph.Clear();
                _cache.Clear();
            }
        }

        public void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates)
        {
            lock (_lockObject)
            {
                foreach (var rate in conversionRates)
                {
                    // init conversion rates
                    AddOrUpdateConversionRate(rate.Item1, rate.Item2, rate.Item3);

                    // init reverse conversion rates for simplicity
                    AddOrUpdateConversionRate(rate.Item2, rate.Item1, 1 / rate.Item3);
                }
                _cache.Clear();
            }
        }

        private void AddOrUpdateConversionRate(string fromCurrency, string toCurrency, double conversionRate)
        {
            if (!_graph.ContainsKey(fromCurrency))
            {
                _graph[fromCurrency] = [];
            }

            _graph[fromCurrency][toCurrency] = conversionRate;
        }

        public double Convert(string fromCurrency, string toCurrency, double amount)
        {
            // check if exist in cache
            var cacheKey = $"{fromCurrency}:{toCurrency}";
            if (_cache.ContainsKey(cacheKey))
            {
                _cache.TryGetValue(cacheKey, out double exchangeRate);
                return amount * exchangeRate;
            }

            // continue if not exist in cache
            lock (_lockObject)
            {
                if (!_graph.ContainsKey(fromCurrency) || !_graph.ContainsKey(toCurrency))
                {
                    throw new Exception("Currency not found.");
                }

                Queue<string> queue = new();
                Dictionary<string, double> visited = new();
                queue.Enqueue(fromCurrency);
                visited[fromCurrency] = amount;

                while (queue.Count != 0)
                {
                    string currentCurrency = queue.Dequeue();
                    double currentAmount = visited[currentCurrency];

                    if (currentCurrency == toCurrency)
                    {
                        _cache[cacheKey] = currentAmount / amount;
                        _cache[cacheKey] = (1 / currentAmount) / amount;

                        return currentAmount;
                    }

                    foreach (var neighbor in _graph[currentCurrency])
                    {
                        string nextCurrency = neighbor.Key;
                        double exchangeRate = neighbor.Value;
                        double convertedAmount = currentAmount * exchangeRate;

                        if (!visited.ContainsKey(nextCurrency))
                        {
                            visited[nextCurrency] = convertedAmount;
                            queue.Enqueue(nextCurrency);
                        }
                    }
                }

                throw new Exception("Conversion path not found.");
            }
        }
    }
}
