export interface PriceTickData {
  symbol: string;
  price: number;
  volume: number;
  timestamp: Date;
  priceChange24h: number;
  priceChangePercent24h: number;
  highPrice24h: number;
  lowPrice24h: number;
}

export enum AnalysisStatus {
  Stopped = 0,
  Starting = 1,
  Running = 2,
  Stopping = 3,
  Error = 4
}

export interface PairAnalysisInfo {
  pairKey: string;
  symbol: string;
  type: TradingPairType;
  status: AnalysisStatus;
  startedAt: Date;
  lastUpdateAt?: Date;
  dataPointsCollected: number;
  currentPrice?: number;
  priceChangePercent?: number;
  errorMessage?: string;
}

export interface PriceAnalysisDetails {
  pairKey: string;
  fromTime: Date;
  toTime: Date;
  priceTicks: PriceTickData[];
  minPrice: number;
  maxPrice: number;
  averagePrice: number;
  totalVolume: number;
  tickCount: number;
}

export interface StartAnalysisRequest {
  symbol: string;
  type: TradingPairType;
}

export interface StopAnalysisRequest {
  pairKey: string;
}

// Импорт enum из базовых моделей
import { TradingPairType } from '../../core/models/trading-pair.model'; 