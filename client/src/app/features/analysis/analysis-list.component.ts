import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AnalysisService } from '../../core/services/analysis.service';
import { PairAnalysisInfo, AnalysisStatus } from '../../shared/models/analysis.models';

@Component({
  selector: 'app-analysis-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="analysis-list-container">
      <!-- Header -->
      <div class="analysis-header">
        <h2>Анализ торговых пар</h2>
        <div class="header-controls">
          <button 
            class="btn btn-refresh"
            (click)="refreshAnalysisList()"
            [disabled]="isLoading">
            <span class="icon-refresh" [class.spinning]="isLoading">⟳</span>
            Обновить
          </button>
        </div>
      </div>

      <!-- Stats -->
      <div class="analysis-stats">
        <div class="stat-item">
          <span class="stat-label">Всего анализов:</span>
          <span class="stat-value">{{ runningAnalysis.length }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">Активных:</span>
          <span class="stat-value">{{ getActiveCount() }}</span>
        </div>
        <div class="stat-item" *ngIf="lastUpdated">
          <span class="stat-label">Последнее обновление:</span>
          <span class="stat-value">{{ lastUpdated | date:'HH:mm:ss dd.MM.yyyy' }}</span>
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
        <span>Загрузка анализов...</span>
      </div>

      <!-- Analysis List -->
      <div class="analysis-grid" *ngIf="!isLoading && runningAnalysis.length > 0">
        <div 
          class="analysis-card"
          *ngFor="let analysis of runningAnalysis; trackBy: trackByPairKey"
          [class.running]="analysis.status === AnalysisStatus.Running"
          [class.error]="analysis.status === AnalysisStatus.Error">
          
          <div class="card-header">
            <h3 class="pair-symbol">{{ analysis.symbol }}</h3>
            <span 
              class="status-badge"
              [class]="getStatusClass(analysis.status)">
              {{ getStatusLabel(analysis.status) }}
            </span>
          </div>

          <div class="card-body">
            <div class="info-row">
              <span class="info-label">Тип:</span>
              <span class="info-value">{{ getTypeLabel(analysis.type) }}</span>
            </div>

            <div class="info-row">
              <span class="info-label">Запущен:</span>
              <span class="info-value">{{ analysis.startedAt | date:'HH:mm dd.MM.yyyy' }}</span>
            </div>

            <div class="info-row">
              <span class="info-label">Данных собрано:</span>
              <span class="info-value">{{ analysis.dataPointsCollected }}</span>
            </div>

            <div class="info-row" *ngIf="analysis.currentPrice">
              <span class="info-label">Текущая цена:</span>
              <span class="info-value">{{ analysis.currentPrice | number:'1.0-8' }}</span>
            </div>

            <div class="info-row" *ngIf="analysis.priceChangePercent !== undefined && analysis.priceChangePercent !== null">
              <span class="info-label">Изменение 24ч:</span>
              <span 
                class="info-value"
                [class.positive]="analysis.priceChangePercent >= 0"
                [class.negative]="analysis.priceChangePercent < 0">
                {{ analysis.priceChangePercent | number:'1.0-2' }}%
              </span>
            </div>

            <div class="info-row" *ngIf="analysis.errorMessage">
              <span class="info-label">Ошибка:</span>
              <span class="info-value error-text">{{ analysis.errorMessage }}</span>
            </div>
          </div>

          <div class="card-actions">
            <button 
              class="btn btn-details"
              (click)="viewDetails(analysis)">
              Детали
            </button>
            <button 
              class="btn btn-stop"
              (click)="stopAnalysis(analysis)"
              [disabled]="analysis.status !== AnalysisStatus.Running">
              Остановить
            </button>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div class="empty-state" *ngIf="!isLoading && runningAnalysis.length === 0">
        <span class="icon-empty">📊</span>
        <h3>Анализы не запущены</h3>
        <p>Перейдите к торговым парам и запустите анализ интересующих вас пар</p>
        <button class="btn btn-primary" (click)="goToTradingPairs()">
          Перейти к торговым парам
        </button>
      </div>
    </div>
  `,
  styles: [`
    .analysis-list-container {
      padding: 20px;
      background: #f8f9fa;
      min-height: 100vh;
    }

    .analysis-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: white;
      padding: 20px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      margin-bottom: 20px;
    }

    .analysis-header h2 {
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

    .icon-refresh.spinning {
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    .analysis-stats {
      display: flex;
      gap: 30px;
      background: white;
      padding: 15px 20px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      margin-bottom: 20px;
      flex-wrap: wrap;
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
      font-size: 18px;
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

    .analysis-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 20px;
    }

    .analysis-card {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      padding: 20px;
      transition: all 0.2s;
    }

    .analysis-card:hover {
      box-shadow: 0 4px 8px rgba(0,0,0,0.15);
    }

    .analysis-card.running {
      border-left: 4px solid #28a745;
    }

    .analysis-card.error {
      border-left: 4px solid #dc3545;
    }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 15px;
    }

    .pair-symbol {
      margin: 0;
      font-size: 18px;
      font-weight: 600;
      color: #333;
    }

    .status-badge {
      padding: 4px 8px;
      border-radius: 12px;
      font-size: 11px;
      font-weight: 500;
      text-transform: uppercase;
    }

    .status-badge.text-gray-500 {
      background: #f8f9fa;
      color: #6c757d;
    }

    .status-badge.text-yellow-500 {
      background: #fff3cd;
      color: #856404;
    }

    .status-badge.text-green-500 {
      background: #d4edda;
      color: #155724;
    }

    .status-badge.text-orange-500 {
      background: #fff3e0;
      color: #e65100;
    }

    .status-badge.text-red-500 {
      background: #f8d7da;
      color: #721c24;
    }

    .card-body {
      margin-bottom: 15px;
    }

    .info-row {
      display: flex;
      justify-content: space-between;
      margin-bottom: 8px;
    }

    .info-label {
      font-size: 13px;
      color: #666;
    }

    .info-value {
      font-size: 13px;
      color: #333;
      font-weight: 500;
    }

    .info-value.positive {
      color: #28a745;
    }

    .info-value.negative {
      color: #dc3545;
    }

    .info-value.error-text {
      color: #dc3545;
      font-size: 12px;
    }

    .card-actions {
      display: flex;
      gap: 8px;
    }

    .btn-details {
      background: #17a2b8;
      color: white;
      flex: 1;
    }

    .btn-details:hover {
      background: #138496;
    }

    .btn-stop {
      background: #dc3545;
      color: white;
      flex: 1;
    }

    .btn-stop:hover:not(:disabled) {
      background: #c82333;
    }

    .btn-stop:disabled {
      background: #6c757d;
      cursor: not-allowed;
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
      margin-bottom: 20px;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-primary:hover {
      background: #0056b3;
    }
  `]
})
export class AnalysisListComponent implements OnInit, OnDestroy {
  runningAnalysis: PairAnalysisInfo[] = [];
  isLoading = false;
  errorMessage = '';
  lastUpdated: Date | null = null;
  
  // Expose enum to template
  AnalysisStatus = AnalysisStatus;

  private refreshInterval?: number;

  constructor(
    private analysisService: AnalysisService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadRunningAnalysis();
    // Автоматическое обновление каждые 30 секунд
    this.refreshInterval = window.setInterval(() => {
      this.loadRunningAnalysis();
    }, 30000);
  }

  ngOnDestroy(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
  }

  async loadRunningAnalysis(): Promise<void> {
    this.isLoading = true;
    this.errorMessage = '';

    try {
      this.runningAnalysis = await this.analysisService.getRunningAnalysis();
      this.lastUpdated = new Date();
    } catch (error) {
      this.errorMessage = 'Ошибка загрузки списка анализов';
      console.error('Error loading running analysis:', error);
    } finally {
      this.isLoading = false;
    }
  }

  async refreshAnalysisList(): Promise<void> {
    await this.loadRunningAnalysis();
  }

  async stopAnalysis(analysis: PairAnalysisInfo): Promise<void> {
    try {
      const success = await this.analysisService.stopAnalysis({
        pairKey: analysis.pairKey
      });

      if (success) {
        console.log(`Анализ остановлен для ${analysis.symbol}`);
        // Обновляем список после остановки
        await this.loadRunningAnalysis();
      } else {
        console.error(`Не удалось остановить анализ для ${analysis.symbol}`);
      }
    } catch (error) {
      console.error('Error stopping analysis:', error);
    }
  }

  viewDetails(analysis: PairAnalysisInfo): void {
    this.router.navigate(['/analysis', analysis.pairKey]);
  }

  goToTradingPairs(): void {
    this.router.navigate(['/trading-pairs']);
  }

  getActiveCount(): number {
    return this.runningAnalysis.filter(a => a.status === AnalysisStatus.Running).length;
  }

  getStatusLabel(status: AnalysisStatus): string {
    return this.analysisService.getStatusLabel(status);
  }

  getStatusClass(status: AnalysisStatus): string {
    return this.analysisService.getStatusClass(status);
  }

  getTypeLabel(type: number): string {
    const typeLabels = {
      1: 'Спот',
      2: 'Фьючерсы',
      3: 'Опционы'
    };
    return typeLabels[type as keyof typeof typeLabels] || 'Неизвестно';
  }

  trackByPairKey(index: number, analysis: PairAnalysisInfo): string {
    return analysis.pairKey;
  }
} 