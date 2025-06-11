import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { TradingPair, TradingPairType } from '../models/trading-pair.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CommandService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Получить торговые пары через CommandController
   */
  async getTradingPairs(
    pairType?: TradingPairType,
    baseAsset?: string,
    quoteAsset?: string,
    activeOnly: boolean = true
  ): Promise<TradingPair[]> {
    try {
      let url = `${this.baseUrl}/command/trading-pairs?activeOnly=${activeOnly}`;
      
      if (pairType !== undefined) {
        url += `&pairType=${pairType}`;
      }
      
      if (baseAsset) {
        url += `&baseAsset=${baseAsset}`;
      }
      
      if (quoteAsset) {
        url += `&quoteAsset=${quoteAsset}`;
      }

      const result = await firstValueFrom(
        this.http.get<TradingPair[]>(url)
      );
      
      return result;
    } catch (error) {
      console.error('Error getting trading pairs via command:', error);
      throw error;
    }
  }
} 