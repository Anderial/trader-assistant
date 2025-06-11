import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  timestamp: Date;
  isVisible: boolean;
  autoClose: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notifications$ = new BehaviorSubject<Notification[]>([]);
  private nextId = 1;

  getNotifications(): Observable<Notification[]> {
    return this.notifications$.asObservable();
  }

  showSuccess(title: string, message: string, autoClose = true): string {
    return this.addNotification('success', title, message, autoClose);
  }

  showError(title: string, message: string, autoClose = false): string {
    return this.addNotification('error', title, message, autoClose);
  }

  showWarning(title: string, message: string, autoClose = true): string {
    return this.addNotification('warning', title, message, autoClose);
  }

  showInfo(title: string, message: string, autoClose = true): string {
    return this.addNotification('info', title, message, autoClose);
  }

  removeNotification(id: string): void {
    const current = this.notifications$.value;
    const updated = current.filter(n => n.id !== id);
    this.notifications$.next(updated);
  }

  clearAll(): void {
    this.notifications$.next([]);
  }

  private addNotification(type: Notification['type'], title: string, message: string, autoClose: boolean): string {
    const id = `notification_${this.nextId++}`;
    
    const notification: Notification = {
      id,
      type,
      title,
      message,
      timestamp: new Date(),
      isVisible: true,
      autoClose
    };

    const current = this.notifications$.value;
    this.notifications$.next([...current, notification]);

    if (autoClose) {
      setTimeout(() => {
        this.removeNotification(id);
      }, environment.notificationDuration);
    }

    return id;
  }
} 