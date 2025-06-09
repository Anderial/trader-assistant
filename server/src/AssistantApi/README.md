# Trader Assistant API

API для автономного торгового ассистента с ML-анализом криптовалют.

## 🚀 Запуск

```bash
dotnet run
```

- **Swagger UI**: https://localhost:5001/swagger
- **API Base URL**: https://localhost:5001/api

## 📋 Endpoints

### Portfolio Controller (`/api/portfolio`)

#### GET `/api/portfolio/overview`
Получить общий обзор портфолио
- **Возвращает**: `OperationResult<PortfolioOverview, ApiErrorCode>`

#### GET `/api/portfolio/positions/futures`  
Получить список активных фьючерсных позиций
- **Возвращает**: `OperationResult<List<FuturesPosition>, ApiErrorCode>`

#### GET `/api/portfolio/positions/spot`
Получить список активных спотовых позиций  
- **Возвращает**: `OperationResult<List<SpotPosition>, ApiErrorCode>`

### Trading Controller (`/api/trading`)

#### GET `/api/trading/history`
Получить историю сделок
- **Query Parameters**: 
  - `limit` (int): количество записей (1-1000, по умолчанию 50)
  - `offset` (int): смещение (по умолчанию 0)
- **Возвращает**: `OperationResult<List<Trade>, ApiErrorCode>`

#### GET `/api/trading/mode`
Получить текущий режим торговли
- **Возвращает**: `OperationResult<TradingMode, ApiErrorCode>`

#### POST `/api/trading/mode/switch`
Переключить режим торговли (paper/live)
- **Body**: `SwitchModeRequest`
- **Возвращает**: `OperationResult<ApiErrorCode>`

#### POST `/api/trading/pause`
Приостановить торговлю
- **Возвращает**: `OperationResult<ApiErrorCode>`

#### POST `/api/trading/resume`  
Возобновить торговлю
- **Возвращает**: `OperationResult<ApiErrorCode>`

#### POST `/api/trading/emergency-stop`
Emergency Stop - закрыть все позиции
- **Возвращает**: `OperationResult<ApiErrorCode>`

### Positions Controller (`/api/positions`)

#### POST `/api/positions/close`
Закрыть позицию
- **Body**: `ClosePositionRequest`
- **Возвращает**: `OperationResult<ApiErrorCode>`

#### POST `/api/positions/close-all`
Закрыть все позиции
- **Query Parameters**:
  - `type` (string): тип позиций (futures/spot/all, необязательно)
- **Возвращает**: `OperationResult<ApiErrorCode>`

### System Controller (`/api/system`)

#### GET `/api/system/status`
Получить статус системы
- **Возвращает**: `OperationResult<SystemStatus, ApiErrorCode>`

#### GET `/api/system/performance`
Получить метрики производительности
- **Возвращает**: `OperationResult<PerformanceMetrics, ApiErrorCode>`

#### GET `/api/system/balance-chart`
Получить данные для графика баланса
- **Query Parameters**:
  - `period` (string): период (24h/7d/30d, по умолчанию 24h)
- **Возвращает**: `OperationResult<BalanceChartData, ApiErrorCode>`

## 📊 Модели данных

### PortfolioOverview
```csharp
{
    "futuresBalance": 8247.85,
    "spotBalance": 15220.79,
    "totalBalance": 23468.64,
    "usedMargin": 1581.13,
    "unrealizedPnL": 287.42,
    "activePositions": 7,
    "marginRatio": 15.5
}
```

### FuturesPosition
```csharp
{
    "symbol": "BTCUSDT",
    "contractType": "PERPETUAL",
    "direction": "LONG",
    "size": "0.25 BTC",
    "entryPrice": 43250.00,
    "currentPrice": 43485.00,
    "markPrice": 43482.50,
    "leverage": "10x",
    "marginUsed": 1087.25,
    "liquidationPrice": 39325.00,
    "fundingRate": "+0.0125%",
    "nextFunding": "2ч 15м",
    "pnL": 58.75,
    "pnLPercent": 0.54,
    "duration": "12 мин",
    "confidence": 0.82,
    "isProfit": true
}
```

## 🔧 HTTP Status Codes

- **200 OK**: Успешный запрос
- **400 Bad Request**: Ошибка валидации (см. бизнес-ошибки в ответе)
- **500 Internal Server Error**: Внутренняя ошибка сервера

## 🎯 Контракт ответа

Все ответы возвращаются в формате `OperationResult`:

```csharp
{
    "successful": true,
    "responseObject": { ... },  // данные при successful: true
    "errors": null
}
```

При ошибке:
```csharp
{
    "successful": false,
    "responseObject": null,
    "errors": [
        {
            "errorCode": "Validation_Failed",
            "message": "Описание ошибки"
        }
    ]
}
```

## 🏷️ Бизнес-коды ошибок

- `Unknown_Error` - Неизвестная ошибка
- `Validation_Failed` - Ошибка валидации
- `Resource_Not_Found` - Ресурс не найден
- `Trading_System_Unavailable` - Система торговли недоступна
- `Invalid_Trading_Mode` - Неверный режим торговли
- `Position_Not_Found` - Позиция не найдена
- `Insufficient_Balance` - Недостаточно средств
- `Position_Already_Closed` - Позиция уже закрыта

## 🔄 Режимы работы

- **paper** - Режим обучения (симуляция)
- **live** - Реальная торговля

В режиме `paper` все данные симулированы и безопасны для тестирования. 