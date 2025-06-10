using Newtonsoft.Json;
using System.Globalization;
using TradeService.Models;

namespace TradeService.Services;

public class BybitApiService : IBybitApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BybitApiService> _logger;
    private readonly string _baseUrl = "https://api.bybit.com";

    public BybitApiService(HttpClient httpClient, ILogger<BybitApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TraderAssistant/1.0");
    }

    public async Task<List<TradingPair>> GetTradingPairsAsync(TradingPairType type)
    {
        try
        {
            var category = GetCategoryFromType(type);
            var endpoint = $"/v5/market/instruments-info?category={category}";

            _logger.LogInformation("Fetching trading pairs for category: {Category}", category);

            var response = await _httpClient.GetStringAsync(endpoint);
            var apiResponse = JsonConvert.DeserializeObject<TradingPairResponse>(response);

            if (apiResponse?.RetCode != 0)
            {
                _logger.LogError("Bybit API returned error: {ErrorCode} - {ErrorMessage}",
                    apiResponse?.RetCode, apiResponse?.RetMsg);
                return new List<TradingPair>();
            }

            var tradingPairs = apiResponse.Result.List
                .Where(p => p.Status.Equals("Trading", StringComparison.OrdinalIgnoreCase))
                .Select(p => MapToTradingPair(p, type))
                .ToList();

            _logger.LogInformation("Successfully fetched {Count} trading pairs for {Type}",
                tradingPairs.Count, type);

            return tradingPairs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch trading pairs for type: {Type}", type);
            return new List<TradingPair>();
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Testing connection to Bybit API");

            var response = await _httpClient.GetStringAsync("/v5/market/time");
            var result = !string.IsNullOrEmpty(response);

            _logger.LogInformation("Connection test result: {Result}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection test failed");
            return false;
        }
    }

    private static string GetCategoryFromType(TradingPairType type)
    {
        return type switch
        {
            TradingPairType.Spot => "spot",
            TradingPairType.Futures => "linear",
            TradingPairType.Options => "option",
            _ => "spot"
        };
    }

    private static TradingPair MapToTradingPair(ByBitTradingPair bybitPair, TradingPairType type)
    {
        return new TradingPair
        {
            Id = Guid.NewGuid().ToString(),
            Symbol = bybitPair.Symbol,
            BaseAsset = bybitPair.BaseCoin,
            QuoteAsset = bybitPair.QuoteCoin,
            Status = bybitPair.Status.Equals("Trading", StringComparison.OrdinalIgnoreCase) 
                ? TradingPairStatus.Inactive 
                : TradingPairStatus.Inactive,
            Type = type,
            Exchange = ExchangeType.Bybit,
            IsActive = bybitPair.Status.Equals("Trading", StringComparison.OrdinalIgnoreCase),
            CreatedAt = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow,
            LastPrice = null,
            Volume24h = null,
            PriceChange24h = null
        };
    }

    public async Task<TradingPairMarketData?> GetMarketDataAsync(string symbol, TradingPairType type)
    {
        try
        {
            var category = GetCategoryFromType(type);
            var endpoint = $"/v5/market/tickers?category={category}&symbol={symbol}";

            _logger.LogInformation("Fetching market data for {Symbol} in category: {Category}", symbol, category);

            var response = await _httpClient.GetStringAsync(endpoint);
            var apiResponse = JsonConvert.DeserializeObject<TickerResponse>(response);

            if (apiResponse?.RetCode != 0)
            {
                _logger.LogError("Bybit API returned error: {ErrorCode} - {ErrorMessage}",
                    apiResponse?.RetCode, apiResponse?.RetMsg);
                return null;
            }

            var ticker = apiResponse.Result.List.FirstOrDefault();
            if (ticker == null)
            {
                _logger.LogWarning("No ticker data found for {Symbol}", symbol);
                return null;
            }

            var marketData = MapToMarketData(ticker, symbol, type);
            
            _logger.LogInformation("Successfully fetched market data for {Symbol}: Price={Price}, Volume={Volume}",
                symbol, marketData.CurrentPrice, marketData.Volume24h);

            return marketData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch market data for {Symbol}:{Type}", symbol, type);
            return null;
        }
    }

    private static TradingPairMarketData MapToMarketData(BybitTicker ticker, string symbol, TradingPairType type)
    {
        // Определяем базовый и котируемый актив из символа
        var baseAsset = "";
        var quoteAsset = "";
        
        if (symbol.EndsWith("USDT"))
        {
            baseAsset = symbol.Substring(0, symbol.Length - 4);
            quoteAsset = "USDT";
        }
        else if (symbol.EndsWith("USDC"))
        {
            baseAsset = symbol.Substring(0, symbol.Length - 4);
            quoteAsset = "USDC";
        }
        else if (symbol.EndsWith("USD"))
        {
            baseAsset = symbol.Substring(0, symbol.Length - 3);
            quoteAsset = "USD";
        }
        else
        {
            // Для других случаев можно попробовать определить по длине
            baseAsset = symbol.Length > 6 ? symbol.Substring(0, symbol.Length - 3) : symbol;
            quoteAsset = symbol.Length > 6 ? symbol.Substring(symbol.Length - 3) : "";
        }

        return new TradingPairMarketData
        {
            Symbol = symbol,
            Type = type,
            BaseAsset = baseAsset,
            QuoteAsset = quoteAsset,
            CurrentPrice = ParseDecimal(ticker.LastPrice),
            Volume24h = ParseDecimal(ticker.Volume24h),
            PriceChange24h = ParseDecimal(ticker.PrevPrice24h) > 0 
                ? ParseDecimal(ticker.LastPrice) - ParseDecimal(ticker.PrevPrice24h) 
                : null,
            PriceChangePercent24h = ParseDecimal(ticker.Price24hPcnt) * 100, // Конвертируем в проценты
            HighPrice24h = ParseDecimal(ticker.HighPrice24h),
            LowPrice24h = ParseDecimal(ticker.LowPrice24h),
            IsActive = true,
            LastUpdateTime = DateTime.UtcNow
        };
    }

    private static decimal ParseDecimal(string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            return result;

        return 0;
    }
}