export interface PortfolioOverview {
  futuresBalance: number;
  spotBalance: number;
  totalBalance: number;
  usedMargin: number;
  unrealizedPnL: number;
  activePositions: number;
  marginRatio: number;
}

export interface FuturesPosition {
  symbol: string;
  contractType: string;
  direction: 'LONG' | 'SHORT';
  size: string;
  entryPrice: number;
  currentPrice: number;
  markPrice: number;
  leverage: string;
  marginUsed: number;
  liquidationPrice: number;
  fundingRate: string;
  nextFunding: string;
  pnL: number;
  pnLPercent: number;
  duration: string;
  confidence: number;
  isProfit: boolean;
}

export interface SpotPosition {
  symbol: string;
  fullName: string;
  amount: string;
  avgBuyPrice: number;
  currentPrice: number;
  totalValue: number;
  pnL: number;
  pnLPercent: number;
  allocation: number;
  isProfit: boolean;
}

export interface Trade {
  id: string;
  type: 'futures' | 'spot';
  typeName: string;
  symbol: string;
  direction: 'LONG' | 'SHORT' | 'BUY' | 'SELL';
  leverage: string;
  size: string;
  entryPrice: number;
  exitPrice: number;
  initialStake: number;
  finalStake: number;
  netProfit: number;
  grossPnL: number;
  fees: number;
  pnLPercent: number;
  duration: string;
  exitReason: string;
  timestamp: string;
  confidence: number;
  isProfit: boolean;
} 