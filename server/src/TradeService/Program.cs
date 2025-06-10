using DistributedKit;
using TradeService.Services;

var builder = WebApplication.CreateBuilder(args);

// Настройка логирования
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Добавляем Orleans Silo с Rant.DistributedKit
builder.Host.UseDistributedKitService(services =>
{
    // Регистрируем сервисы в Orleans DI контейнере
    services.AddHttpClient<IBybitApiService, BybitApiService>();
    services.AddTransient<IBybitApiService, BybitApiService>();
    services.AddSingleton<IBybitWebSocketService, BybitWebSocketService>();
    services.AddSingleton<IWebSocketManager, TradeService.Services.WebSocketManager>();
    services.AddHostedService<TradeService.Services.WebSocketManager>();
});

var app = builder.Build();

// Настройка для продакшена
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

Console.WriteLine("=== TraderAssistant TraderService (Orleans Silo) ===");        
Console.WriteLine("Silo starting...");

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to start silo: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}
