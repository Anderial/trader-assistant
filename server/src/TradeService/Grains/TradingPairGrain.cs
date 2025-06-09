using TradeService.Models;
using TradeService.Services;

namespace TradeService.Grains;

public class TradingPairGrain : Grain, ITradingPairGrain
{
    private readonly IBybitApiService _bybitApiService;
    private readonly ILogger<TradingPairGrain> _logger;

    private List<TradingPair> _cachedPairs = [];
    private DateTime _lastUpdated = DateTime.MinValue;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

    public TradingPairGrain(
        IBybitApiService bybitApiService,
        ILogger<TradingPairGrain> logger)
    {
        _bybitApiService = bybitApiService;
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TradingPairGrain {GrainId} activated", this.GetPrimaryKeyString());

        // Загружаем торговые пары при активации
        await RefreshTradingPairsAsync();

        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<List<TradingPair>> GetTradingPairsAsync()
    {
        // Проверяем, нужно ли обновить кэш
        if (DateTime.UtcNow - _lastUpdated > _cacheTimeout)
        {
            _logger.LogInformation("Cache expired, refreshing trading pairs");
            await RefreshTradingPairsAsync();
        }

        return _cachedPairs.ToList();
    }

    public async Task<bool> RefreshTradingPairsAsync()
    {
        try
        {
            _logger.LogInformation("Refreshing trading pairs from Bybit API");

            var allPairs = new List<TradingPair>();

            // Получаем спотовые пары
            var spotPairs = await _bybitApiService.GetTradingPairsAsync(TradingPairType.Spot);
            allPairs.AddRange(spotPairs);

            // Получаем фьючерсные пары
            var futuresPairs = await _bybitApiService.GetTradingPairsAsync(TradingPairType.LinearFutures);
            allPairs.AddRange(futuresPairs);

            _cachedPairs = allPairs;
            _lastUpdated = DateTime.UtcNow;

            _logger.LogInformation("Successfully refreshed {Count} trading pairs", _cachedPairs.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh trading pairs");
            return false;
        }
    }

    public async Task<List<TradingPair>> GetTradingPairsByTypeAsync(TradingPairType type)
    {
        var allPairs = await GetTradingPairsAsync();
        return allPairs.Where(p => p.Type == type).ToList();
    }

    public async Task<TradingPair?> GetTradingPairAsync(string symbol)
    {
        var allPairs = await GetTradingPairsAsync();
        return allPairs.FirstOrDefault(p => p.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
    }

    public Task<DateTime> GetLastUpdatedAsync()
    {
        return Task.FromResult(_lastUpdated);
    }
}