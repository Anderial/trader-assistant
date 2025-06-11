export interface SystemStatus {
  webSocketConnections: WebSocketConnections;
  database: Database;
  bybitApi: BybitApi;
  mlModels: MLModels;
  system: SystemInfo;
}

export interface WebSocketConnections {
  active: number;
  total: number;
  status: string;
  latency: string;
}

export interface Database {
  status: string;
  latency: string;
  uptime: string;
}

export interface BybitApi {
  status: string;
  rateLimit: number;
  maxRate: number;
  responseTime: string;
}

export interface MLModels {
  active: number;
  total: number;
  accuracy: number;
  lastUpdate: string;
}

export interface SystemInfo {
  cpuUsage: number;
  memoryUsage: number;
  maxMemory: number;
  uptime: string;
}

export interface PerformanceMetrics {
  totalReturn: number;
  winRate: number;
  sharpeRatio: number;
  maxDrawdown: number;
  totalTrades: number;
  avgDuration: string;
}

export interface BalanceChartData {
  labels: string[];
  datasets: BalanceDataset[];
}

export interface BalanceDataset {
  label: string;
  data: number[];
  borderColor: string;
  backgroundColor: string;
  borderWidth: number;
  fill: boolean;
  tension: number;
}

export interface TradingMode {
  current: 'paper' | 'live';
  paperTradingStats?: PaperTradingStats;
}

export interface PaperTradingStats {
  totalSimulatedTrades: number;
  simulatedBalance: number;
  simulatedProfit: number;
  testingDuration: string;
} 