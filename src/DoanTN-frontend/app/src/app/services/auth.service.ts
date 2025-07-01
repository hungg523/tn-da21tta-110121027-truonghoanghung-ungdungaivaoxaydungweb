import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, BehaviorSubject, of, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { catchError } from 'rxjs/operators';

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

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrlV1 = `${environment.apiUrlV1}/auth`;
  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly USER_DATA_KEY = 'user_data';
  
  private isLoggedInSubject = new BehaviorSubject<boolean>(false);
  public isLoggedIn$ = this.isLoggedInSubject.asObservable();

  private isAuthenticated = false;

  constructor(private http: HttpClient) {
    this.checkAuthStatus();
  }

  private checkAuthStatus(): void {
    const accessToken = this.getAccessToken();
    this.isLoggedInSubject.next(!!accessToken);
  }

  login(credentials: { email: string; password: string }): Observable<any> {
    return this.http.post(`${this.apiUrlV1}/login`, credentials, {withCredentials: true})
      .pipe(
        catchError((error: HttpErrorResponse) => {
          let errorMessage = 'Đã có lỗi xảy ra. Vui lòng thử lại sau.';
          
          if (error.status === 400) {
            errorMessage = 'Email hoặc mật khẩu không chính xác';
          }
          else if (error.status === 404) {
            errorMessage = 'Tài khoản không tồn tại';
          }

          return throwError(() => errorMessage);
        })
      );
  }

  loginWithGoogle(): void {
    window.location.href = `${this.apiUrlV1}/login-google`;
  }

  loginWithFacebook(): void {
    window.location.href = `${this.apiUrlV1}/login-facebook`;
  }

  handleGoogleCallback(): Observable<AuthResponse> {
    return this.http.get<AuthResponse>(`${this.apiUrlV1}/oauth-google`, { withCredentials: true })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          let errorMessage = 'Đã có lỗi xảy ra khi đăng nhập bằng Google. Vui lòng thử lại sau.';
          
          if (error.status === 400) {
            errorMessage = 'Không thể xác thực tài khoản Google';
          }
          else if (error.status === 404) {
            errorMessage = 'Tài khoản Google không tồn tại';
          }

          return throwError(() => errorMessage);
        })
      );
  }

  handleFacebookCallback(): Observable<AuthResponse> {
    return this.http.get<AuthResponse>(`${this.apiUrlV1}/oauth-facebook`, { withCredentials: true })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          let errorMessage = 'Đã có lỗi xảy ra khi đăng nhập bằng Facebook. Vui lòng thử lại sau.';
          
          if (error.status === 400) {
            errorMessage = 'Không thể xác thực tài khoản Facebook';
          }
          else if (error.status === 404) {
            errorMessage = 'Tài khoản Facebook không tồn tại';
          }

          return throwError(() => errorMessage);
        })
      );
  }

  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrlV1}/register`, request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          let errorMessage = 'Đã có lỗi xảy ra. Vui lòng thử lại sau.';

          if (error.status === 409) {
            errorMessage = 'Email này đã được đăng ký. Vui lòng đăng nhập hoặc khôi phục mật khẩu.';
          } 
          else if (error.status === 400) {
            if (error.error?.errors) {
              const validationErrors = error.error.errors;
              if (validationErrors.Email) {
                errorMessage = 'Email không được để trống hoặc không hợp lệ';
              } else if (validationErrors.Password) {
                errorMessage = 'Mật khẩu không được để trống';
              } else if (validationErrors.ConfirmPassword) {
                errorMessage = 'Mật khẩu xác nhận không khớp với mật khẩu';
              }
            } else {
              errorMessage = 'Thông tin đăng ký không hợp lệ';
            }
          }
          else if (error.status === 404) {
            errorMessage = 'Tài khoản không tồn tại trong hệ thống';
          }
          else if (error.status === 403) {
            errorMessage = 'Bạn không có quyền thực hiện hành động này';
          }
          else if (error.status === 500) {
            errorMessage = 'Lỗi hệ thống. Vui lòng thử lại sau';
          }

          return throwError(() => errorMessage);
        })
      );
  }

  logout(): void {
    const accessToken = this.getAccessToken()
    if (!accessToken) {
      this.clearAuthData();
      window.location.href = '/';
      return;
    }
    this.isAuthenticated = false;
    this.http.post(`${this.apiUrlV1}/logout`, {}).subscribe({
      next: () => {
        this.clearAuthData();
        window.location.href = '/';
      },
      error: () => {
        // Nếu có lỗi khi gọi API logout, vẫn xóa dữ liệu local
        this.clearAuthData();
        window.location.href = '/';
      }
    });
  }

  refreshToken(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrlV1}/refresh-token`, {}, {withCredentials: true});
  }

  setAccessToken(token: string): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, token);
    this.isLoggedInSubject.next(true);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  private clearAuthData(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.USER_DATA_KEY);
    this.isLoggedInSubject.next(false);
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }

  getCurrentUser(): UserData | null {
    const userData = localStorage.getItem(this.USER_DATA_KEY);
    return userData ? JSON.parse(userData) : null;
  }

  setUserData(userData: UserData): void {
    localStorage.setItem(this.USER_DATA_KEY, JSON.stringify(userData));
  }

  verifyOtp(data: { email: string; otp: string }): Observable<any> {
    return this.http.put(`${this.apiUrlV1}/verify-otp`, data);
  }

  resendOtp(data: { email: string }): Observable<any> {
    return this.http.post(`${this.apiUrlV1}/resend-otp`, data);
  }

  verifyResetPassword(data: { email: string }): Observable<any> {
    return this.http.post(`${this.apiUrlV1}/verify-reset-password`, data).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'Đã có lỗi xảy ra. Vui lòng thử lại sau.';
        
        if (error.status === 400) {
          if (error.error?.details?.includes('Invalid email format')) {
            errorMessage = 'Email không đúng định dạng';
          } else if (error.error?.details?.includes('Email cannot be null') || 
                    error.error?.details?.includes('Email cannot be empty')) {
            errorMessage = 'Email không được để trống';
          } else if (error.error?.details?.includes('Email cannot exceed 255 characters')) {
            errorMessage = 'Email không được vượt quá 255 ký tự';
          }
        }
        
        return throwError(() => errorMessage);
      })
    );
  }

  resetPassword(data: { token: string; newPassword: string }): Observable<any> {
    return this.http.post(`${this.apiUrlV1}/reset-password`, data).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'Đã có lỗi xảy ra. Vui lòng thử lại sau.';

        if (error.status === 401) {
          errorMessage = 'Token không hợp lệ hoặc đã hết hạn';
        } else if (error.status === 400) {
          if (error.error?.details?.includes('Password cannot be null') || 
              error.error?.details?.includes('Password cannot be empty')) {
            errorMessage = 'Mật khẩu không được để trống';
          } else if (error.error?.details?.includes('Password cannot exceed 255 characters')) {
            errorMessage = 'Mật khẩu không được vượt quá 255 ký tự';
          }
        }

        return throwError(() => errorMessage);
      })
    );
  }

  changePassword(request: { newPassword: string; confirmPassword: string }): Observable<any> {
    return this.http.put(`${this.apiUrlV1}/change-password`, request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          let errorMessage = 'Đã có lỗi xảy ra. Vui lòng thử lại sau.';
          
          if (error.status === 400) {
            errorMessage = 'Mật khẩu không hợp lệ hoặc không khớp';
          } else if (error.status === 500) {
            errorMessage = 'Lỗi hệ thống. Vui lòng thử lại sau';
          }
  
          return throwError(() => errorMessage);
        })
      );
  }
  
}