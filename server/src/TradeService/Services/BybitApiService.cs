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
            TradingPairType.LinearFutures => "linear",
            TradingPairType.InverseFutures => "inverse",
            TradingPairType.Option => "option",
            _ => "spot"
        };
    }

    private static TradingPair MapToTradingPair(ByBitTradingPair bybitPair, TradingPairType type)
    {
        return new TradingPair
        {
            Symbol = bybitPair.Symbol,
            BaseCoin = bybitPair.BaseCoin,
            QuoteCoin = bybitPair.QuoteCoin,
            Status = bybitPair.Status,
            Type = type,
            MinOrderQty = ParseDecimal(bybitPair.LotSizeFilter.MinOrderQty),
            MaxOrderQty = ParseDecimal(bybitPair.LotSizeFilter.MaxOrderQty),
            TickSize = ParseDecimal(bybitPair.PriceFilter.TickSize),
            LotSizeFilter = 0, // Заполним позже если нужно
            IsTrading = bybitPair.Status.Equals("Trading", StringComparison.OrdinalIgnoreCase),
            LastUpdated = DateTime.UtcNow,
            LastPrice = 0, // Получим в отдельном запросе
            Volume24H = 0, // Получим в отдельном запросе
            PriceChange24H = 0, // Получим в отдельном запросе
            PriceChangePercent24H = 0 // Получим в отдельном запросе
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