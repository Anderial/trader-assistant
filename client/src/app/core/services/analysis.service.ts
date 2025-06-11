import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  PairAnalysisInfo, 
  PriceAnalysisDetails, 
  StartAnalysisRequest, 
  StopAnalysisRequest 
} from '../../shared/models/analysis.models';
import { OperationResult } from '../models/trading-pair.model';

@Injectable({
  providedIn: 'root'
})
export class AnalysisService {
  private readonly baseUrl = `${environment.apiUrl}/analysis`;

  constructor(private http: HttpClient) {}

  /**
   * Запустить анализ торговой пары
   */
  async startAnalysis(request: StartAnalysisRequest): Promise<boolean> {
    try {
      const response = await this.http.post<OperationResult<boolean>>(`${this.baseUrl}/start`, request).toPromise();
      return !!(response?.successful && response.responseObject);
    } catch (error) {
      console.error('Error starting analysis:', error);
      return false;
    }
  }

  /**
   * Остановить анализ торговой пары
   */
  async stopAnalysis(request: StopAnalysisRequest): Promise<boolean> {
    try {
      const response = await this.http.post<OperationResult<boolean>>(`${this.baseUrl}/stop`, request).toPromise();
      return !!(response?.successful && response.responseObject);
    } catch (error) {
      console.error('Error stopping analysis:', error);
      return false;
    }
  }

  /**
   * Получить список запущенных анализов
   */
  async getRunningAnalysis(): Promise<PairAnalysisInfo[]> {
    try {
      const response = await this.http.get<OperationResult<PairAnalysisInfo[]>>(`${this.baseUrl}/running`).toPromise();
      return response?.successful ? response.responseObject || [] : [];
    } catch (error) {
      console.error('Error getting running analysis:', error);
      return [];
    }
  }

  /**
   * Получить детальные данные анализа за период
   */
  async getAnalysisDetails(pairKey: string, fromTime: Date, toTime: Date): Promise<PriceAnalysisDetails | null> {
    try {
      const encodedPairKey = encodeURIComponent(pairKey);
      const params = new HttpParams()
        .set('fromTime', fromTime.toISOString())
        .set('toTime', toTime.toISOString());

      const response = await this.http.get<OperationResult<PriceAnalysisDetails>>(
        `${this.baseUrl}/${encodedPairKey}/details`, 
        { params }
      ).toPromise();

      return response?.successful ? response.responseObject || null : null;
    } catch (error) {
      console.error('Error getting analysis details:', error);
      return null;
    }
  }

  /**
   * Получить данные анализа за последний час
   */
  async getRecentAnalysisDetails(pairKey: string): Promise<PriceAnalysisDetails | null> {
    const toTime = new Date();
    const fromTime = new Date(toTime.getTime() - 60 * 60 * 1000); // 1 час назад
    return this.getAnalysisDetails(pairKey, fromTime, toTime);
  }

  /**
   * Создать ключ торговой пары
   */
  createPairKey(symbol: string, type: number): string {
    const typeNames = ['Spot', 'Futures', 'Options'];
    return `${symbol}:${typeNames[type] || 'Spot'}`;
  }

  /**
   * Получить лейбл статуса анализа
   */
  getStatusLabel(status: number): string {
    const statusLabels = {
      0: 'Остановлен',
      1: 'Запускается',
      2: 'Работает',
      3: 'Останавливается',
      4: 'Ошибка'
    };
    return statusLabels[status as keyof typeof statusLabels] || 'Неизвестно';
  }

  /**
   * Получить CSS класс для статуса
   */
  getStatusClass(status: number): string {
    const statusClasses = {
      0: 'text-gray-500',      // Остановлен
      1: 'text-yellow-500',    // Запускается
      2: 'text-green-500',     // Работает
      3: 'text-orange-500',    // Останавливается
      4: 'text-red-500'        // Ошибка
    };
    return statusClasses[status as keyof typeof statusClasses] || 'text-gray-500';
  }
} 