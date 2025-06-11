import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { TradingPair, TradingPairMarketData, OperationResult, TradingPairType } from '../models/trading-pair.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TradingPairsService {
  private readonly baseUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) {}

  /**
   * Получить рыночные данные для указанных торговых пар
   */
  async getMarketData(pairKeys: string[]): Promise<TradingPairMarketData[]> {
    try {
      const url = `${this.baseUrl}/trading-pairs/market-data`;
      
      const result = await firstValueFrom(
        this.http.post<OperationResult<TradingPairMarketData[]>>(url, pairKeys)
      );
      
      if (result.successful && result.responseObject) {
        return result.responseObject;
      }
      
      console.error('Failed to get market data:', result.errors);
      return [];
    } catch (error) {
      console.error('Error getting market data:', error);
      throw error;
    }
  }

  /**
   * Получить все торговые пары
   */
  async getAllTradingPairs(): Promise<TradingPair[]> {
    try {
      const url = `${this.baseUrl}/trading-pairs`;
      
      const result = await firstValueFrom(
        this.http.get<OperationResult<TradingPair[]>>(url)
      );
      
      if (result.successful && result.responseObject) {
        return result.responseObject;
      }
      
      console.error('Failed to get trading pairs:', result.errors);
      return [];
    } catch (error) {
      console.error('Error getting trading pairs:', error);
      throw error;
    }
  }

  /**
   * Получить торговые пары по типу
   */
  async getTradingPairsByType(type: TradingPairType): Promise<TradingPair[]> {
    try {
      const url = `${this.baseUrl}/trading-pairs/type/${type}`;
      
      const result = await firstValueFrom(
        this.http.get<OperationResult<TradingPair[]>>(url)
      );
      
      if (result.successful && result.responseObject) {
        return result.responseObject;
      }
      
      console.error('Failed to get trading pairs by type:', result.errors);
      return [];
    } catch (error) {
      console.error('Error getting trading pairs by type:', error);
      throw error;
    }
  }

  /**
   * Обновить список торговых пар
   */
  async refreshTradingPairs(): Promise<boolean> {
    try {
      const url = `${this.baseUrl}/trading-pairs/refresh`;
      
      const result = await firstValueFrom(
        this.http.post<OperationResult<boolean>>(url, {})
      );
      
      return result.successful && result.responseObject === true;
    } catch (error) {
      console.error('Error refreshing trading pairs:', error);
      return false;
    }
  }

  /**
   * Создать ключ для торговой пары
   */
  createPairKey(symbol: string, type: TradingPairType): string {
    const typeString = TradingPairType[type];
    return `${symbol}:${typeString}`;
  }

  /**
   * Парсить ключ торговой пары
   */
  parsePairKey(pairKey: string): { symbol: string; type: TradingPairType } | null {
    const parts = pairKey.split(':');
    if (parts.length !== 2) {
      return null;
    }

    const symbol = parts[0];
    const typeString = parts[1];
    
    const type = (TradingPairType as any)[typeString];
    if (type === undefined) {
      return null;
    }

    return { symbol, type };
  }
} 