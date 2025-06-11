export interface TradingPair {
  id: string;
  symbol: string;
  baseAsset: string;
  quoteAsset: string;
  exchange: ExchangeType;
  type: TradingPairType;
  status: TradingPairStatus;
  createdAt: string;
  lastUpdated?: string;
  config: TradingConfig;
  lastPrice?: number;
  priceChange24h?: number;
  volume24h?: number;
  isActive: boolean;
}

export enum ExchangeType {
  Bybit = 1,
  Binance = 2,
  OKX = 3,
  Mock = 99
}

export enum TradingPairType {
  Spot = 1,
  Futures = 2,
  Options = 3
}

export enum TradingPairStatus {
  Inactive = 0,
  DataCollection = 1,
  Training = 2,
  PaperTrading = 3,
  ReadyForLive = 4,
  LiveTrading = 5,
  Paused = 6,
  Error = 7
}

export interface TradingConfig {
  riskPercentage: number;
  maxPositionSize: number;
  stopLossPercentage: number;
  enabledStrategies: StrategyType[];
  autoTransitionToLive: boolean;
  minTradingDays: number;
  minSharpeRatio: number;
  maxDrawdownPercentage: number;
  historicalDataDays: number;
  enabledTimeFrames: TimeFrame[];
  createdAt: string;
  updatedAt?: string;
}

export enum StrategyType {
  Scalping = 1,
  SwingTrading = 2,
  MeanReversion = 3,
  Momentum = 4,
  Arbitrage = 5
}

export enum TimeFrame {
  M1 = 1,
  M5 = 5,
  M15 = 15,
  M30 = 30,
  H1 = 60,
  H4 = 240,
  D1 = 1440
}

export interface TradingPairMarketData {
  symbol: string;
  type: TradingPairType;
  baseAsset: string;
  quoteAsset: string;
  currentPrice?: number;
  volume24h?: number;
  priceChange24h?: number;
  priceChangePercent24h?: number;
  highPrice24h?: number;
  lowPrice24h?: number;
  isActive: boolean;
  lastUpdateTime?: string;
}

export interface OperationResult<T> {
  successful: boolean;
  responseObject?: T;
  errors?: Array<{
    errorCode: string;
    message?: string;
  }>;
} 