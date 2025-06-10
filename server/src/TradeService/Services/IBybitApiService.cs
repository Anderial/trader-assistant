using TradeService.Models;

namespace TradeService.Services;

public interface IBybitApiService
{
    Task<List<TradingPair>> GetTradingPairsAsync(TradingPairType type);
    Task<bool> TestConnectionAsync();
    Task<TradingPairMarketData?> GetMarketDataAsync(string symbol, TradingPairType type);
}