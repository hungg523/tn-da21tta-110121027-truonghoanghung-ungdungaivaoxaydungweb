import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-oauth-callback',
  template: `
    <div class="loading-container">
      <div class="spinner">
        <div class="bounce1"></div>
        <div class="bounce2"></div>
        <div class="bounce3"></div>
      </div>
      <p>Đang xử lý đăng nhập...</p>
    </div>
  `,
  styles: [`
    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100vh;
    }
    .spinner {
      display: flex;
      justify-content: center;
      gap: 4px;
      margin-bottom: 16px;
    }
    .spinner > div {
      width: 12px;
      height: 12px;
      background-color: #667eea;
      border-radius: 50%;
      animation: bounce 0.6s infinite;
    }
    .spinner .bounce2 {
      animation-delay: 0.2s;
    }
    .spinner .bounce3 {
      animation-delay: 0.4s;
    }
    @keyframes bounce {
      0%, 100% { transform: translateY(0); }
      50% { transform: translateY(-6px); }
    }
    p {
      color: #666;
      font-size: 16px;
    }
  `]
})
export class OAuthCallbackComponent implements OnInit {
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const provider = this.route.snapshot.data['provider'];

    this.route.queryParams.subscribe(params => {
      const accessToken = params['access_token'];
      console.log(accessToken);
      if (!provider) {
        this.router.navigate(['/login'], {
          queryParams: {
            error: 'Không xác định được nhà cung cấp đăng nhập.',
          },
        });
        return;
      }

      if (accessToken) {
        this.authService.setAccessToken(accessToken);
        this.router.navigate(['/']);
      } else {
        this.router.navigate(['/login'], {
          queryParams: {
            error:
              'Đã có lỗi xảy ra trong quá trình đăng nhập. Vui lòng thử lại.',
          },
        });
      }
    });
  }
} 