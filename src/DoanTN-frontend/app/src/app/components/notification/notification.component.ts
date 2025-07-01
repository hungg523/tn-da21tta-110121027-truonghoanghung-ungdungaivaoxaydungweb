import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../services/notification.service';
import { trigger, state, style, animate, transition } from '@angular/animations';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.css'],
  animations: [
    trigger('slideInOut', [
      state('true', style({
        transform: 'translateX(0)',
        opacity: 1
      })),
      state('false', style({
        transform: 'translateX(100%)',
        opacity: 0
      })),
      transition('false => true', animate('1000ms ease-out')),
      transition('true => false', animate('1000ms ease-in'))
    ])
  ]
})
export class NotificationComponent {
  constructor(public notificationService: NotificationService) {}
} 