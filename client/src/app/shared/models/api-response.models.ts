export enum ApiErrorCode {
  // Общие ошибки
  Unknown_Error = 'Unknown_Error',
  Validation_Failed = 'Validation_Failed',
  Resource_Not_Found = 'Resource_Not_Found',
  
  // Ошибки торговли
  Trading_System_Unavailable = 'Trading_System_Unavailable',
  Invalid_Trading_Mode = 'Invalid_Trading_Mode',
  Position_Not_Found = 'Position_Not_Found',
  Insufficient_Balance = 'Insufficient_Balance',
  Position_Already_Closed = 'Position_Already_Closed',
  
  // Ошибки позиций
  Invalid_Position_Type = 'Invalid_Position_Type',
  Position_Cannot_Be_Closed = 'Position_Cannot_Be_Closed',
  Liquidation_Risk_Too_High = 'Liquidation_Risk_Too_High',
  
  // Ошибки системы
  Database_Connection_Failed = 'Database_Connection_Failed',
  External_API_Unavailable = 'External_API_Unavailable',
  ML_Model_Not_Responding = 'ML_Model_Not_Responding',
  WebSocket_Connection_Lost = 'WebSocket_Connection_Lost',
  
  // Ошибки конфигурации
  Invalid_Risk_Settings = 'Invalid_Risk_Settings',
  Strategy_Configuration_Invalid = 'Strategy_Configuration_Invalid',
  Trading_Pair_Not_Supported = 'Trading_Pair_Not_Supported'
}

export interface ErrorDetail {
  errorCode: ApiErrorCode;
  message?: string;
}

export interface OperationResult<T> {
  successful: boolean;
  responseObject?: T;
  errors?: ErrorDetail[];
}

export interface PaginatedRequest {
  limit: number;
  offset: number;
}

export interface SwitchModeRequest {
  mode: 'paper' | 'live';
  confirmation: boolean;
}

export interface ClosePositionRequest {
  type: 'futures' | 'spot';
  symbol: string;
  reason: string;
} 