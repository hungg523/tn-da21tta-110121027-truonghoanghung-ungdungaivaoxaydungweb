import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResponse {
  accessToken: string;
}

export interface UserData {
  id: number;
  username: string;
  email: string;
  fullName?: string;
  avatar?: string;
  role: string;
}

export interface RegisterResponse {
  isSuccess: boolean;
  statusCode: number;
}

class AuthService {
  private apiUrlV1 = `${environment.apiUrlV1}/auth`;
  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly USER_DATA_KEY = 'user_data';
  private isAuthenticated = false;

  constructor() {
    this.checkAuthStatus();
  }

  private checkAuthStatus(): void {
    const accessToken = this.getAccessToken();
    this.isAuthenticated = !!accessToken;
  }

  async login(credentials: { email: string; password: string }): Promise<AuthResponse> {
    try {
      const request = new Request(`${this.apiUrlV1}/login-admin`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(credentials),
      });

      const response = await authInterceptor(request);

      if (!response.ok) {
        let errorMessage = 'Đã có lỗi xảy ra. Vui lòng thử lại sau.';
        
        if (response.status === 400) {
          errorMessage = 'Email hoặc mật khẩu không chính xác';
        } else if (response.status === 404) {
          errorMessage = 'Tài khoản không tồn tại';
        }

        throw new Error(errorMessage);
      }

      const data: AuthResponse = await response.json();
      this.setAccessToken(data.accessToken);
      return data;
    } catch (error) {
      throw error;
    }
  }

  async logout(): Promise<void> {
    try {
      const request = new Request(`${this.apiUrlV1}/logout`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });

      await authInterceptor(request);
    } finally {
      this.clearAuthData();
    }
  }

  async refreshToken(): Promise<AuthResponse> {
    const request = new Request(`${this.apiUrlV1}/refresh-token`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);

    if (!response.ok) {
      throw new Error('Failed to refresh token');
    }

    return await response.json();
  }

  async getProfile(): Promise<UserData> {
    const request = new Request(`${this.apiUrlV1}/profile`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);

    if (!response.ok) {
      throw new Error('Failed to get profile');
    }

    return await response.json();
  }

  setAccessToken(token: string): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(this.ACCESS_TOKEN_KEY, token);
    }
  }

  getAccessToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(this.ACCESS_TOKEN_KEY);
    }
    return null;
  }

  private clearAuthData(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(this.ACCESS_TOKEN_KEY);
      localStorage.removeItem(this.USER_DATA_KEY);
    }
    this.isAuthenticated = false;
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }

  getCurrentUser(): UserData | null {
    const userData = localStorage.getItem(this.USER_DATA_KEY);
    return userData ? JSON.parse(userData) : null;
  }
}

export const authService = new AuthService();