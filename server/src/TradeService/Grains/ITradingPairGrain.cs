using TradeService.Models;

namespace TradeService.Grains;

[Alias(nameof(ITradingPairGrain))]
public interface ITradingPairGrain : IGrainWithStringKey
{
    [Alias(nameof(GetTradingPairsAsync))]
    Task<List<TradingPair>> GetTradingPairsAsync();

    [Alias(nameof(RefreshTradingPairsAsync))]
    Task<bool> RefreshTradingPairsAsync();

    [Alias(nameof(GetTradingPairsByTypeAsync))]
    Task<List<TradingPair>> GetTradingPairsByTypeAsync(TradingPairType type);

    [Alias(nameof(GetTradingPairAsync))]
    Task<TradingPair?> GetTradingPairAsync(string symbol);

    [Alias(nameof(GetLastUpdatedAsync))]
    Task<DateTime> GetLastUpdatedAsync();
}