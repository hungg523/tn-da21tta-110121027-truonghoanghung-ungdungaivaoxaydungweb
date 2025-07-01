import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface Breadcrumb {
  label: string;
  link?: string;
}

@Component({
  selector: 'app-page-banner',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <section class="page-banner">
      <div class="container">
        <div class="page-banner-content">
          <nav class="breadcrumb">
            <a routerLink="/" class="breadcrumb-item">Trang chủ</a>
            <ng-container *ngFor="let item of breadcrumbs; let last = last">
              <span class="breadcrumb-separator">/</span>
              <ng-container *ngIf="!last && item.link">
                <a [routerLink]="item.link" class="breadcrumb-item">{{ item.label }}</a>
              </ng-container>
              <ng-container *ngIf="!last && !item.link">
                <span class="breadcrumb-item">{{ item.label }}</span>
              </ng-container>
              <ng-container *ngIf="last">
                <span class="breadcrumb-item active">{{ item.label }}</span>
              </ng-container>
            </ng-container>
          </nav>
        </div>
      </div>
    </section>
  `,
  styleUrls: ['./page-banner.component.css']
})
export class PageBannerComponent {
  @Input() title: string = '';
  @Input() breadcrumbs: Breadcrumb[] = [];
} 