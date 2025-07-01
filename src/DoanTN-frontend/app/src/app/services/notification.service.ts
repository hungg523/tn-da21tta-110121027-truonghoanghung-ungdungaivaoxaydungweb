import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type NotificationType = 'success' | 'error' | 'info';

export interface Notification {
  message: string;
  type: NotificationType;
  show: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notification = new BehaviorSubject<Notification>({
    message: '',
    type: 'success',
    show: false
  });

  notification$ = this.notification.asObservable();

  show(message: string, type: NotificationType = 'success'): void {
    this.notification.next({
      message,
      type,
      show: true
    });

    setTimeout(() => {
      this.hide();
    }, 2000);
  }

  hide(): void {
    const currentNotification = this.notification.value;
    if (currentNotification.show) {
      this.notification.next({
        ...currentNotification,
        show: false
      });
    }
  }
} 