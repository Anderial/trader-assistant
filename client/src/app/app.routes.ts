import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/trading-pairs',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(c => c.DashboardComponent),
    title: 'Торговый Ассистент - Панель управления'
  },
  {
    path: 'trading-pairs',
    loadComponent: () => import('./features/trading-pairs/trading-pairs.component').then(c => c.TradingPairsComponent),
    title: 'Торговые пары'
  },
  {
    path: 'analysis',
    loadComponent: () => import('./features/analysis/analysis-list.component').then(c => c.AnalysisListComponent),
    title: 'Анализ торговых пар'
  },
  {
    path: 'analysis/:pairKey',
    loadComponent: () => import('./features/analysis/analysis-details.component').then(c => c.AnalysisDetailsComponent),
    title: 'Детали анализа'
  },
  {
    path: '**',
    redirectTo: '/trading-pairs'
  }
];
