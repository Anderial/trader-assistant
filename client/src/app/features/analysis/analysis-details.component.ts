import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AnalysisService } from '../../core/services/analysis.service';
import { PriceAnalysisDetails, PriceTickData } from '../../shared/models/analysis.models';

@Component({
  selector: 'app-analysis-details',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="analysis-details-container">
      <!-- Header -->
      <div class="details-header">
        <div class="header-left">
          <button class="btn btn-back" (click)="goBack()">
            ← Назад к анализам
          </button>
          <h2>Детали анализа: {{ pairKey }}</h2>
        </div>
        <div class="header-controls">
          <select 
            class="time-range-selector"
            [(ngModel)]="selectedTimeRange"
            (ngModelChange)="onTimeRangeChange()">
            <option value="1h">Последний час</option>
            <option value="4h">Последние 4 часа</option>
            <option value="24h">Последние 24 часа</option>
            <option value="7d">Последняя неделя</option>
          </select>
          <button 
            class="btn btn-refresh"
            (click)="refreshData()"
            [disabled]="isLoading">
            <span class="icon-refresh" [class.spinning]="isLoading">⟳</span>
            Обновить
          </button>
        </div>
      </div>

      <!-- Stats Summary -->
      <div class="analysis-stats" *ngIf="analysisDetails">
        <div class="stat-item">
          <span class="stat-label">Точек данных:</span>
          <span class="stat-value">{{ analysisDetails.tickCount }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">Мин. цена:</span>
          <span class="stat-value">{{ analysisDetails.minPrice | number:'1.0-8' }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">Макс. цена:</span>
          <span class="stat-value">{{ analysisDetails.maxPrice | number:'1.0-8' }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">Сред. цена:</span>
          <span class="stat-value">{{ analysisDetails.averagePrice | number:'1.0-8' }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">Общий объем:</span>
          <span class="stat-value">{{ analysisDetails.totalVolume | number:'1.0-2' }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">Период:</span>
          <span class="stat-value">
            {{ analysisDetails.fromTime | date:'HH:mm:ss dd.MM' }} - 
            {{ analysisDetails.toTime | date:'HH:mm:ss dd.MM' }}
          </span>
        </div>
      </div>

      <!-- Error Message -->
      <div class="error-message" *ngIf="errorMessage">
        <span class="icon-error">⚠</span>
        {{ errorMessage }}
      </div>

      <!-- Loading -->
      <div class="loading-container" *ngIf="isLoading">
        <div class="spinner"></div>
        <span>Загрузка данных анализа...</span>
      </div>

      <!-- Price Chart Placeholder -->
      <div class="chart-container" *ngIf="!isLoading && analysisDetails">
        <h3>График изменения цены</h3>
        <div class="chart-placeholder">
          <p>📈 Здесь будет график цены в реальном времени</p>
          <p>Интеграция с Chart.js или D3.js планируется в следующих версиях</p>
        </div>
      </div>

      <!-- Price Ticks Table -->
      <div class="price-ticks-table" *ngIf="!isLoading && analysisDetails && analysisDetails.priceTicks.length > 0">
        <h3>Последние изменения цены</h3>
        <div class="table-container">
          <div class="table-header">
            <div class="col-time">Время</div>
            <div class="col-price">Цена</div>
            <div class="col-volume">Объем</div>
            <div class="col-change">Изменение 24ч</div>
            <div class="col-high">Макс 24ч</div>
            <div class="col-low">Мин 24ч</div>
          </div>
          
          <div class="table-body">
            <div 
              class="table-row"
              *ngFor="let tick of getRecentTicks(); trackBy: trackByTimestamp">
              <div class="col-time">
                {{ tick.timestamp | date:'HH:mm:ss.SSS' }}
              </div>
              <div class="col-price">
                {{ tick.price | number:'1.0-8' }}
              </div>
              <div class="col-volume">
                {{ tick.volume | number:'1.0-2' }}
              </div>
              <div class="col-change">
                <span 
                  [class.positive]="tick.priceChangePercent24h >= 0"
                  [class.negative]="tick.priceChangePercent24h < 0">
                  {{ tick.priceChangePercent24h | number:'1.0-2' }}%
                </span>
              </div>
              <div class="col-high">
                {{ tick.highPrice24h | number:'1.0-8' }}
              </div>
              <div class="col-low">
                {{ tick.lowPrice24h | number:'1.0-8' }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div class="empty-state" *ngIf="!isLoading && (!analysisDetails || analysisDetails.priceTicks.length === 0)">
        <span class="icon-empty">📊</span>
        <h3>Нет данных для отображения</h3>
        <p>Данные анализа пока не собраны или анализ еще не запущен</p>
      </div>
    </div>
  `,
  styles: [`
    .analysis-details-container {
      padding: 20px;
      background: #f8f9fa;
      min-height: 100vh;
    }

    .details-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: white;
      padding: 20px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      margin-bottom: 20px;
    }

    .header-left {
      display: flex;
      align-items: center;
      gap: 15px;
    }

    .header-left h2 {
      margin: 0;
      color: #333;
      font-size: 24px;
      font-weight: 600;
    }

    .header-controls {
      display: flex;
      gap: 15px;
      align-items: center;
    }

    .btn {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 16px;
      border: none;
      border-radius: 4px;
      font-size: 14px;
      cursor: pointer;
      transition: background-color 0.2s;
      text-decoration: none;
    }

    .btn-back {
      background: #6c757d;
      color: white;
    }

    .btn-back:hover {
      background: #5a6268;
    }

    .btn-refresh {
      background: #007bff;
      color: white;
    }

    .btn-refresh:hover:not(:disabled) {
      background: #0056b3;
    }

    .btn-refresh:disabled {
      background: #6c757d;
      cursor: not-allowed;
    }

    .time-range-selector {
      padding: 8px 12px;
      border: 1px solid #ddd;
      border-radius: 4px;
      background: white;
      font-size: 14px;
      min-width: 150px;
    }

    .icon-refresh.spinning {
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    .analysis-stats {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 20px;
      background: white;
      padding: 20px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      margin-bottom: 20px;
    }

    .stat-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .stat-label {
      font-size: 12px;
      color: #666;
      text-transform: uppercase;
      font-weight: 500;
    }

    .stat-value {
      font-size: 16px;
      color: #333;
      font-weight: 600;
    }

    .error-message {
      background: #f8d7da;
      color: #721c24;
      padding: 12px 16px;
      border-radius: 4px;
      margin-bottom: 20px;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .loading-container {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 60px 20px;
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      gap: 12px;
    }

    .spinner {
      width: 24px;
      height: 24px;
      border: 3px solid #f3f3f3;
      border-top: 3px solid #007bff;
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    .chart-container {
      background: white;
      padding: 20px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      margin-bottom: 20px;
    }

    .chart-container h3 {
      margin: 0 0 15px 0;
      color: #333;
      font-size: 18px;
      font-weight: 600;
    }

    .chart-placeholder {
      height: 300px;
      background: #f8f9fa;
      border: 2px dashed #dee2e6;
      border-radius: 8px;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      color: #6c757d;
    }

    .price-ticks-table {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      overflow: hidden;
    }

    .price-ticks-table h3 {
      margin: 0;
      padding: 20px 20px 15px 20px;
      color: #333;
      font-size: 18px;
      font-weight: 600;
    }

    .table-container {
      max-height: 400px;
      overflow-y: auto;
    }

    .table-header {
      display: grid;
      grid-template-columns: 120px 120px 100px 120px 120px 120px;
      background: #f8f9fa;
      border-bottom: 2px solid #dee2e6;
      font-weight: 600;
      color: #495057;
      font-size: 12px;
      text-transform: uppercase;
    }

    .table-header > div {
      padding: 15px 12px;
      border-right: 1px solid #dee2e6;
    }

    .table-header > div:last-child {
      border-right: none;
    }

    .table-body {
      max-height: 300px;
      overflow-y: auto;
    }

    .table-row {
      display: grid;
      grid-template-columns: 120px 120px 100px 120px 120px 120px;
      border-bottom: 1px solid #dee2e6;
      transition: background-color 0.2s;
    }

    .table-row:hover {
      background: #f8f9fa;
    }

    .table-row > div {
      padding: 12px;
      border-right: 1px solid #dee2e6;
      display: flex;
      align-items: center;
      font-size: 13px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .table-row > div:last-child {
      border-right: none;
    }

    .positive {
      color: #28a745;
      font-weight: 600;
    }

    .negative {
      color: #dc3545;
      font-weight: 600;
    }

    .empty-state {
      text-align: center;
      padding: 60px 20px;
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .empty-state .icon-empty {
      font-size: 48px;
      display: block;
      margin-bottom: 16px;
    }

    .empty-state h3 {
      color: #666;
      margin-bottom: 8px;
    }

    .empty-state p {
      color: #999;
      font-size: 14px;
    }
  `]
})
export class AnalysisDetailsComponent implements OnInit {
  pairKey: string = '';
  analysisDetails: PriceAnalysisDetails | null = null;
  isLoading = false;
  errorMessage = '';
  selectedTimeRange = '1h';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private analysisService: AnalysisService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.pairKey = params.get('pairKey') || '';
      if (this.pairKey) {
        this.loadAnalysisDetails();
      }
    });
  }

  async loadAnalysisDetails(): Promise<void> {
    this.isLoading = true;
    this.errorMessage = '';

    try {
      const timeRange = this.getTimeRangeInMilliseconds();
      const toTime = new Date();
      const fromTime = new Date(toTime.getTime() - timeRange);

      this.analysisDetails = await this.analysisService.getAnalysisDetails(
        this.pairKey, 
        fromTime, 
        toTime
      );

      if (!this.analysisDetails) {
        this.errorMessage = 'Не удалось загрузить данные анализа';
      }
    } catch (error) {
      this.errorMessage = 'Ошибка загрузки данных анализа';
      console.error('Error loading analysis details:', error);
    } finally {
      this.isLoading = false;
    }
  }

  async refreshData(): Promise<void> {
    await this.loadAnalysisDetails();
  }

  async onTimeRangeChange(): Promise<void> {
    await this.loadAnalysisDetails();
  }

  goBack(): void {
    this.router.navigate(['/analysis']);
  }

  getRecentTicks(): PriceTickData[] {
    if (!this.analysisDetails) return [];
    
    // Показываем последние 20 записей
    return this.analysisDetails.priceTicks
      .slice(-20)
      .reverse(); // Новые сверху
  }

  private getTimeRangeInMilliseconds(): number {
    switch (this.selectedTimeRange) {
      case '1h': return 60 * 60 * 1000;
      case '4h': return 4 * 60 * 60 * 1000;
      case '24h': return 24 * 60 * 60 * 1000;
      case '7d': return 7 * 24 * 60 * 60 * 1000;
      default: return 60 * 60 * 1000;
    }
  }

  trackByTimestamp(index: number, tick: PriceTickData): string {
    return tick.timestamp.toString();
  }
} 