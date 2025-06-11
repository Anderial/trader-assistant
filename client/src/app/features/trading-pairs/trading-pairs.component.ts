import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CommandService } from '../../core/services/command.service';
import { TradingPairsService } from '../../core/services/trading-pairs.service';
import { AnalysisService } from '../../core/services/analysis.service';
import { TradingPair, TradingPairType, TradingPairStatus, TradingPairMarketData } from '../../core/models/trading-pair.model';

@Component({
  selector: 'app-trading-pairs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './trading-pairs.component.html',
  styleUrls: ['./trading-pairs.component.css']
})
export class TradingPairsComponent implements OnInit {
  tradingPairs: TradingPair[] = [];
  filteredPairs: TradingPair[] = [];
  marketDataMap: Map<string, TradingPairMarketData> = new Map();
  selectedType: TradingPairType | 'all' = 'all';
  isLoading = false;
  isLoadingMarketData = false;
  lastUpdated: Date | null = null;
  errorMessage = '';

  // Пагинация
  currentPage = 1;
  pageSize = 6;
  pageSizeOptions = [6, 10, 25, 50];

  // Динамически определяемые типы на основе реальных данных
  availableTradingPairTypes: Array<{ value: TradingPairType | 'all', label: string }> = [
    { value: 'all', label: 'Все пары' }
  ];

  // Все возможные типы для маппинга
  private readonly allTradingPairTypes = [
    { value: TradingPairType.Spot, label: 'Спот' },
    { value: TradingPairType.Futures, label: 'Фьючерсы' },
    { value: TradingPairType.Options, label: 'Опционы' }
  ];

  constructor(
    private commandService: CommandService,
    private tradingPairsService: TradingPairsService,
    private analysisService: AnalysisService
  ) {}

  ngOnInit(): void {
    this.loadTradingPairs();
  }

  // Геттеры для пагинации
  get totalPages(): number {
    return Math.ceil(this.filteredPairs.length / this.pageSize);
  }

  get pagedPairs(): TradingPair[] {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    return this.filteredPairs.slice(startIndex, endIndex);
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    const totalPages = this.totalPages;
    
    if (totalPages <= maxVisiblePages) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
      let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);
      
      if (endPage - startPage + 1 < maxVisiblePages) {
        startPage = Math.max(1, endPage - maxVisiblePages + 1);
      }
      
      for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
      }
    }
    
    return pages;
  }

  async loadTradingPairs(): Promise<void> {
    this.isLoading = true;
    this.errorMessage = '';
    
    try {
      const pairs = await this.commandService.getTradingPairs();
      this.tradingPairs = pairs || [];
        this.updateAvailableTypes();
        this.filterPairs();
      this.lastUpdated = new Date();
      
      // Автоматически загружаем рыночные данные для первой страницы
      await this.loadMarketDataForCurrentPage();
    } catch (error) {
      this.errorMessage = 'Ошибка соединения с сервером';
      console.error('Error loading trading pairs:', error);
    } finally {
      this.isLoading = false;
    }
  }

  async refreshTradingPairs(): Promise<void> {
    this.isLoading = true;
    this.errorMessage = '';
    
    try {
      await this.loadTradingPairs(); // Просто перезагружаем данные
    } catch (error) {
      this.errorMessage = 'Ошибка соединения с сервером';
      console.error('Error refreshing trading pairs:', error);
    } finally {
      this.isLoading = false;
    }
  }

  async loadLastUpdated(): Promise<void> {
    // Не нужно - CommandService всегда возвращает свежие данные
    this.lastUpdated = new Date();
  }

  onTypeChange(type: TradingPairType | 'all'): void {
    // Преобразуем строку в число для enum если нужно
    if (typeof type === 'string' && type !== 'all') {
      const numType = parseInt(type);
      if (!isNaN(numType)) {
        this.selectedType = numType as TradingPairType;
      } else {
        this.selectedType = type as 'all';
      }
    } else {
      this.selectedType = type;
    }
    console.log('Type changed to:', this.selectedType, 'Original:', type);
    this.filterPairs();
  }

  async onPageSizeChange(newPageSize: number): Promise<void> {
    this.pageSize = newPageSize;
    this.currentPage = 1; // Сбрасываем на первую страницу
    console.log('Page size changed to:', this.pageSize);
    await this.loadMarketDataForCurrentPage();
  }

  async goToPage(page: number): Promise<void> {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      console.log('Go to page:', this.currentPage);
      await this.loadMarketDataForCurrentPage();
    }
  }

  goToPreviousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  goToNextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  goToFirstPage(): void {
    this.currentPage = 1;
  }

  goToLastPage(): void {
    this.currentPage = this.totalPages;
  }

  private updateAvailableTypes(): void {
    // Получаем уникальные типы из загруженных торговых пар
    const uniqueTypes = new Set(this.tradingPairs.map(pair => pair.type));
    
    // Сбрасываем доступные типы и добавляем "Все пары"
    this.availableTradingPairTypes = [
      { value: 'all', label: 'Все пары' }
    ];

    // Добавляем только те типы, которые есть в данных
    uniqueTypes.forEach(type => {
      const typeInfo = this.allTradingPairTypes.find(t => t.value === type);
      if (typeInfo) {
        this.availableTradingPairTypes.push(typeInfo);
      }
    });

    console.log('Available types:', this.availableTradingPairTypes);
    console.log('Unique types in data:', Array.from(uniqueTypes));
  }

  private filterPairs(): void {
    if (this.selectedType === 'all') {
      this.filteredPairs = [...this.tradingPairs];
    } else {
      this.filteredPairs = this.tradingPairs.filter(pair => pair.type === this.selectedType);
    }
    
    // Сбрасываем на первую страницу при изменении фильтра
    this.currentPage = 1;
    
    console.log('Filtered pairs:', this.filteredPairs.length, 'Selected type:', this.selectedType);
  }

  getTypeLabel(type: TradingPairType): string {
    const typeInfo = this.allTradingPairTypes.find(t => t.value === type);
    return typeInfo?.label || type.toString();
  }

  getStatusLabel(status: TradingPairStatus): string {
    const statusLabels = {
      [TradingPairStatus.Inactive]: 'Неактивна',
      [TradingPairStatus.DataCollection]: 'Сбор данных',
      [TradingPairStatus.Training]: 'Обучение',
      [TradingPairStatus.PaperTrading]: 'Тестирование',
      [TradingPairStatus.ReadyForLive]: 'Готова',
      [TradingPairStatus.LiveTrading]: 'Торговля',
      [TradingPairStatus.Paused]: 'Пауза',
      [TradingPairStatus.Error]: 'Ошибка'
    };
    return statusLabels[status] || status.toString();
  }

  trackBySymbol(index: number, pair: TradingPair): string {
    return pair.symbol;
  }

  /**
   * Загрузить рыночные данные для текущей страницы
   */
  async loadMarketDataForCurrentPage(): Promise<void> {
    if (this.pagedPairs.length === 0) {
      return;
    }

    this.isLoadingMarketData = true;
    
    try {
      // Создаем ключи для текущей страницы
      const pairKeys = this.pagedPairs.map(pair => 
        this.tradingPairsService.createPairKey(pair.symbol, pair.type)
      );

      // Загружаем рыночные данные
      const marketDataList = await this.tradingPairsService.getMarketData(pairKeys);
      
      // Обновляем карту рыночных данных
      marketDataList.forEach(marketData => {
        const key = this.tradingPairsService.createPairKey(marketData.symbol, marketData.type);
        this.marketDataMap.set(key, marketData);
      });

      console.log('Loaded market data for', marketDataList.length, 'pairs');
    } catch (error) {
      console.error('Error loading market data:', error);
    } finally {
      this.isLoadingMarketData = false;
    }
  }

  /**
   * Получить рыночные данные для торговой пары
   */
  getMarketData(pair: TradingPair): TradingPairMarketData | null {
    const key = this.tradingPairsService.createPairKey(pair.symbol, pair.type);
    return this.marketDataMap.get(key) || null;
  }

  /**
   * Обновить рыночные данные для видимых пар
   */
  async refreshMarketData(): Promise<void> {
    await this.loadMarketDataForCurrentPage();
  }

  // === Методы анализа ===

  async startAnalysis(pair: TradingPair): Promise<void> {
    try {
      const success = await this.analysisService.startAnalysis({
        symbol: pair.symbol,
        type: pair.type
      });

      if (success) {
        console.log(`Анализ запущен для ${pair.symbol}`);
        // Можно показать уведомление об успехе
      } else {
        console.error(`Не удалось запустить анализ для ${pair.symbol}`);
      }
    } catch (error) {
      console.error('Error starting analysis:', error);
    }
  }

  async stopAnalysis(pair: TradingPair): Promise<void> {
    try {
      const pairKey = this.analysisService.createPairKey(pair.symbol, pair.type);
      const success = await this.analysisService.stopAnalysis({ pairKey });

      if (success) {
        console.log(`Анализ остановлен для ${pair.symbol}`);
        // Можно показать уведомление об успехе
      } else {
        console.error(`Не удалось остановить анализ для ${pair.symbol}`);
      }
    } catch (error) {
      console.error('Error stopping analysis:', error);
    }
  }

  createPairKey(pair: TradingPair): string {
    return this.analysisService.createPairKey(pair.symbol, pair.type);
  }
} 