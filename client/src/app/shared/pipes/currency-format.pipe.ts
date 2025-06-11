import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'currencyFormat',
  standalone: true
})
export class CurrencyFormatPipe implements PipeTransform {
  transform(value: number | string | null | undefined, currency: string = 'USDT', decimals: number = 2): string {
    if (value === null || value === undefined) {
      return `0.00 ${currency}`;
    }

    const numValue = typeof value === 'string' ? parseFloat(value) : value;
    
    if (isNaN(numValue)) {
      return `0.00 ${currency}`;
    }

    // Определяем количество знаков после запятой в зависимости от величины числа
    let finalDecimals = decimals;
    
    if (Math.abs(numValue) >= 1000) {
      finalDecimals = 0;
    } else if (Math.abs(numValue) >= 100) {
      finalDecimals = 1;
    } else if (Math.abs(numValue) >= 1) {
      finalDecimals = 2;
    } else if (Math.abs(numValue) >= 0.01) {
      finalDecimals = 4;
    } else {
      finalDecimals = 8;
    }

    const formatted = numValue.toLocaleString('ru-RU', {
      minimumFractionDigits: finalDecimals,
      maximumFractionDigits: finalDecimals
    });

    return `${formatted} ${currency}`;
  }
} 