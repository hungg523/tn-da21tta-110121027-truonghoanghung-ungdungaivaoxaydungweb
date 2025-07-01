import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../services/loading.service';
import { Observable } from 'rxjs';
import { Router, NavigationStart, NavigationEnd, NavigationCancel, NavigationError } from '@angular/router';

@Component({
  selector: 'app-preloader',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div id="preloader" *ngIf="loading$ | async"></div>
  `,
  styles: [`
    #preloader {
      position: fixed;
      left: 0;
      top: 0;
      z-index: 99999;
      width: 100%;
      height: 100%;
      overflow: visible;
      background: rgba(255, 255, 255, 0.8);
    }
    #preloader:after {
      content: '';
      position: absolute;
      left: 50%;
      top: 50%;
      transform: translate(-50%, -50%);
      width: 40px;
      height: 40px;
      border: 3px solid #f3f3f3;
      border-radius: 50%;
      border-top: 3px solid #3498db;
      animation: spin 1s linear infinite;
    }
    @keyframes spin {
      0% { transform: translate(-50%, -50%) rotate(0deg); }
      100% { transform: translate(-50%, -50%) rotate(360deg); }
    }
  `]
})
export class PreloaderComponent implements OnInit, OnDestroy {
  loading$!: Observable<boolean>;

  constructor(
    private loadingService: LoadingService,
    private router: Router
  ) {
    this.loading$ = this.loadingService.isLoading$;
    
    // Mặc định ẩn loading
    this.loadingService.hide();

    // Theo dõi sự kiện chuyển trang
    this.router.events.subscribe(event => {
      if (event instanceof NavigationStart) {
        this.loadingService.show();
      } else if (
        event instanceof NavigationEnd ||
        event instanceof NavigationCancel ||
        event instanceof NavigationError
      ) {
        this.loadingService.hide();
      }
    });
  }

  ngOnInit() {
    // Ẩn loading khi component được khởi tạo
    this.loadingService.hide();
    
    // Ẩn loading khi trang load xong
    if (document.readyState === 'complete') {
      this.loadingService.hide();
    } else {
      window.addEventListener('load', () => {
        this.loadingService.hide();
      });
    }
  }

  ngOnDestroy() {
    // Đảm bảo loading bị ẩn khi component bị hủy
    this.loadingService.hide();
  }

  loadData() {
    this.loadingService.show();
    // Gọi API hoặc xử lý dữ liệu
    // ...
    this.loadingService.hide();
  }
} 