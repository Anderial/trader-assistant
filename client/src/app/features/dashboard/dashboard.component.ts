import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TradingPairsComponent } from '../trading-pairs/trading-pairs.component';

interface FuturesPosition {
  symbol: string;
  side: string;
  size: number;
  entryPrice: number;
  markPrice: number;
  pnl: number;
  margin: number;
}

interface SpotPosition {
  asset: string;
  balance: number;
  price: number;
  value: number;
  change24h: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, TradingPairsComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  futuresPositions: FuturesPosition[] = [
    {
      symbol: 'BTCUSDT',
      side: 'Long',
      size: 0.5,
      entryPrice: 45000,
      markPrice: 44500,
      pnl: -250,
      margin: 2250
    },
    {
      symbol: 'ETHUSDT',
      side: 'Short',
      size: 2.0,
      entryPrice: 3200,
      markPrice: 3250,
      pnl: -100,
      margin: 1600
    },
    {
      symbol: 'ADAUSDT',
      side: 'Long',
      size: 1000,
      entryPrice: 0.50,
      markPrice: 0.52,
      pnl: 20,
      margin: 250
    }
  ];

  spotPositions: SpotPosition[] = [
    {
      asset: 'BTC',
      balance: 0.25,
      price: 44500,
      value: 11125,
      change24h: -0.025
    },
    {
      asset: 'ETH',
      balance: 3.5,
      price: 3250,
      value: 11375,
      change24h: 0.035
    },
    {
      asset: 'ADA',
      balance: 2000,
      price: 0.52,
      value: 1040,
      change24h: 0.085
    }
  ];

  constructor() {}

  ngOnInit(): void {
    // Component initialization
  }

  closePosition(symbol: string): void {
    console.log('Closing position for:', symbol);
    // Implementation will be added later
  }

  tradeAsset(asset: string): void {
    console.log('Trading asset:', asset);
    // Implementation will be added later
  }
}
