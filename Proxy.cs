using Interview.Models;
using System.Collections.Concurrent;

namespace Interview
{
    internal class Proxy
    {
        private readonly IList<ProviderModel> _providers;
        private readonly ConcurrentDictionary<string, List<ProviderStatsModel>> _stats;
        private readonly object _lock;
        private readonly ProxyStatModel _proxyStats;
        public Proxy()
        {
            _providers = new List<ProviderModel>()
            {
                new()
                {
                    Name = "Api 1",
                    ChanceOfError = 6,
                    RpmLimit = 15
                },
                new()
                {
                    Name = "Api 2",
                    ChanceOfError = 8,
                    RpmLimit = 10
                },
                new()
                {
                    Name = "Api 3",
                    ChanceOfError = 2,
                    RpmLimit = 10
                }
            };
            var dict = _providers.ToDictionary(key => key.Name, value => new List<ProviderStatsModel>());
            _stats = new ConcurrentDictionary<string, List<ProviderStatsModel>>(dict);
            _lock = new object();
            _proxyStats = new ProxyStatModel();
        }

        public async Task RunAsync()
        {
            var isSuccess = await GetLocationFromProviders(new List<ProviderModel>(_providers));

            if (isSuccess)
                _proxyStats.SuccessCount++;
            else
                _proxyStats.FailureCount++;
        }

        public void PrintStats()
        {
            foreach (var provider in _providers)
            {
                _stats.TryGetValue(provider.Name, out var stat);
                Console.WriteLine($"Stats for {provider.Name}: Total Calls: {stat.Count}, Total Error = {stat.Count(x => !x.IsSuccess)}");
            }

            Console.WriteLine("--------------------------------------------------");

            Console.WriteLine($"Number of times all API Failed: {_proxyStats.FailureCount}");
            Console.WriteLine($"Number of times for API Bad request: {_proxyStats.BadRequestCount}");
            Console.WriteLine($"Number of times when rate limit reached: {_proxyStats.RpmLimitReachedCount}");
            Console.WriteLine($"Number of times API Successfully returned location: {_proxyStats.SuccessCount}");
        }

        private async Task<bool> GetLocationFromProviders(IList<ProviderModel> providers)
        {
            if (providers.Count == 0) 
                return false;

            // Get best providers
            var bestProvider = GetBestProvider(providers);

            // Mocking Api State
            var random = new Random();
            var responseTime = random.Next(500, 3000); // Time for the response
            var timeStamp = DateTime.UtcNow; // Request Timestamp

            try
            {
                // Mocking calling the API
                await Task.Delay(responseTime);

                // Checking if API resulted in error or not
                var isSuccess = random.Next(1, 11) >= bestProvider.ChanceOfError;

                lock (_lock)
                {
                    // Recording the values
                    _stats.TryGetValue(bestProvider.Name, out var stat);

                    stat.Add(new ProviderStatsModel()
                    {
                        IsSuccess = isSuccess,
                        Timestamp = timeStamp,
                        ResponseTime = responseTime
                    });

                    _stats[bestProvider.Name] = stat;
                }

                if (isSuccess)
                    return true;
                else
                    throw new Exception();
            }
            catch
            {
                // Trying again with another provider
                _proxyStats.BadRequestCount++;
                providers.Remove(bestProvider);
                return await GetLocationFromProviders(providers);
            }
        }

        private ProviderModel GetBestProvider(IList<ProviderModel> providers)
        {
            ProviderModel bestProvider = providers.First();
            double maxScore = 0;
            var requestTimestamp = DateTime.UtcNow;

            foreach (var provider in providers)
            {
                _stats.TryGetValue(provider.Name, out var stat);
                double score = 0;

                if (stat == null || stat.Count == 0)
                    continue;

                var errRate = stat.Where(x => x.Timestamp - requestTimestamp < TimeSpan.FromSeconds(15))?.Average(x => x.IsSuccess ? 0 : 1);
                var avgResponseTime = stat.Where(x => x.Timestamp - requestTimestamp < TimeSpan.FromSeconds(15))?.Average(x => x.ResponseTime);
                var rpmCount = stat.Count(x => requestTimestamp - x.Timestamp  < TimeSpan.FromSeconds(10));

                if (rpmCount >= provider.RpmLimit)
                    _proxyStats.RpmLimitReachedCount++;

                // Algo to calculate the score of the api based on error rate, avg response time and rpm limit
                score = ((errRate * 0.7) + (avgResponseTime * 0.3)) * (rpmCount < provider.RpmLimit ? 1 : 0) ?? 0;

                if(maxScore < score)
                {
                    bestProvider = provider;
                    maxScore = score;
                }
            }

            return bestProvider;
        }
    }
}
