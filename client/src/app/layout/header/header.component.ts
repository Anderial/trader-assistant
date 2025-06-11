import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <header class="header">
      <div class="header-left">
        <div class="logo">
          <i class="fa fa-chart-line"></i>
          <span>Торговый Ассистент</span>
        </div>
        
        <nav class="main-nav">
          <a routerLink="/trading-pairs" class="nav-link" routerLinkActive="active">📊 Торговые пары</a>
          <a routerLink="/analysis" class="nav-link" routerLinkActive="active">📈 Анализ</a>
        </nav>
      </div>

      <div class="header-center">
        <div class="balance-info">
          <div class="total-balance">
            <span class="balance-label">Общий баланс</span>
            <span class="balance-value">0.00 USDT</span>
          </div>
        </div>
      </div>

      <div class="header-right">
        <div class="trading-controls">
          <div class="trading-mode">
            <div class="mode-switch paper-mode">
              <span class="mode-label">Симуляция</span>
            </div>
          </div>
        </div>
      </div>
    </header>
  `,
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  // Простая заглушка - все функции временно отключены
  constructor() {
    console.log('Header component loaded - simplified version');
  }
} 