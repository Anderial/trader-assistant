namespace AssistantApi.Contracts;

public enum ApiErrorCode
{
    // Общие ошибки
    Unknown_Error,
    Validation_Failed,
    Resource_Not_Found,

    // Ошибки торговли
    Trading_System_Unavailable,
    Invalid_Trading_Mode,
    Position_Not_Found,
    Insufficient_Balance,
    Position_Already_Closed,

    // Ошибки позиций
    Invalid_Position_Type,
    Position_Cannot_Be_Closed,
    Liquidation_Risk_Too_High,

    // Ошибки системы
    Database_Connection_Failed,
    External_API_Unavailable,
    ML_Model_Not_Responding,
    WebSocket_Connection_Lost,

    // Ошибки конфигурации
    Invalid_Risk_Settings,
    Strategy_Configuration_Invalid,
    Trading_Pair_Not_Supported
}