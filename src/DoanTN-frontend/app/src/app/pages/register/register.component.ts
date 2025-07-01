import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { interval, Subscription } from 'rxjs';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit, OnDestroy {
  registerForm: FormGroup;
  otpForm: FormGroup;
  otpValues: string[] = ['', '', '', '', '', ''];
  isLoading = false;
  error: string | null = null;
  showPassword = false;
  showConfirmPassword = false;
  showOtpForm = false;
  registeredEmail = '';
  remainingAttempts = 5;
  resendTimer: number = 0;
  private timerSubscription?: Subscription;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validator: this.passwordMatchValidator });

    this.otpForm = this.fb.group({
      otp1: ['', [Validators.required, Validators.pattern(/^[0-9A-Z]$/)]],
      otp2: ['', [Validators.required, Validators.pattern(/^[0-9A-Z]$/)]],
      otp3: ['', [Validators.required, Validators.pattern(/^[0-9A-Z]$/)]],
      otp4: ['', [Validators.required, Validators.pattern(/^[0-9A-Z]$/)]],
      otp5: ['', [Validators.required, Validators.pattern(/^[0-9A-Z]$/)]],
      otp6: ['', [Validators.required, Validators.pattern(/^[0-9A-Z]$/)]],
    });
  }

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/']);
    }
  }

  ngOnDestroy(): void {
    if (this.timerSubscription) {
      this.timerSubscription.unsubscribe();
    }
  }

  togglePassword(field: 'password' | 'confirmPassword'): void {
    if (field === 'password') {
      this.showPassword = !this.showPassword;
    } else {
      this.showConfirmPassword = !this.showConfirmPassword;
    }
  }

  onRegister(): void {
    if (this.registerForm.valid) {
      this.isLoading = true;
      this.error = null;
      
      this.authService.register(this.registerForm.value).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.error = 'Đăng ký thành công! Vui lòng nhập mã OTP được gửi đến email của bạn.';
            this.showOtpForm = true;
            this.registeredEmail = this.registerForm.get('email')?.value;
            this.registerForm.reset();
            this.startResendTimer(); // Bắt đầu đếm ngược ngay khi đăng ký thành công
          }
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          if (error.status === 409) {
            if (error.error.includes('Email has activated')) {
              this.error = 'Email này đã được đăng ký. Vui lòng đăng nhập hoặc khôi phục mật khẩu.';
            }
          } else if (error.status === 400) {
            if (error.error.includes('Email cannot be empty')) {
              this.error = 'Email không được để trống';
            } else if (error.error.includes('Password cannot be null')) {
              this.error = 'Mật khẩu không được để trống';
            } else if (error.error.includes('ConfirmPassword must match Password')) {
              this.error = 'Mật khẩu xác nhận không khớp';
            } else {
              this.error = 'Thông tin đăng ký không hợp lệ';
            }
          } else {
            this.error = 'Có lỗi xảy ra. Vui lòng thử lại sau.';
          }
        }
      });
    } else {
      this.validateAllFormFields(this.registerForm);
    }
  }

  onOtpInput(event: any, index: number, nextInput?: any): void {
    const value = event.target.value;
    const otpControls = ['otp1', 'otp2', 'otp3', 'otp4', 'otp5', 'otp6'];
    
    // Chỉ cho phép nhập số và chữ
    if (!/^[0-9a-zA-Z]$/.test(value) && value !== '') {
      event.target.value = '';
      this.otpForm.get(otpControls[index])?.setValue('');
      return;
    }

    // Cập nhật giá trị OTP
    const upperValue = value.toUpperCase();
    this.otpValues[index] = upperValue;
    event.target.value = upperValue;
    this.otpForm.get(otpControls[index])?.setValue(upperValue);

    // Tự động focus vào ô tiếp theo
    if (value !== '' && nextInput) {
      nextInput.focus();
    }
  }

  onOtpKeyDown(event: any, index: number): void {
    const otpControls = ['otp1', 'otp2', 'otp3', 'otp4', 'otp5', 'otp6'];
    
    // Xử lý phím Backspace
    if (event.key === 'Backspace' && !event.target.value) {
      // Xóa giá trị hiện tại
      this.otpForm.get(otpControls[index])?.setValue('');
      this.otpValues[index] = '';
      
      // Focus vào ô trước đó
      const prevInput = event.target.previousElementSibling;
      if (prevInput) {
        prevInput.focus();
      }
    }
  }

  isOtpValid(): boolean {
    return this.otpForm.valid;
  }

  onVerifyOtp(): void {
    if (this.otpForm.valid) {
      this.isLoading = true;
      this.error = null;
      
      // Lấy giá trị OTP từ form
      const otpValues = Object.values(this.otpForm.value);
      const otp = otpValues.join('');
      
      this.authService.verifyOtp({ 
        email: this.registeredEmail, 
        otp: otp 
      }).subscribe({
        next: (response) => {
          if (response.isSuccess && response.statusCode === 200) {
            this.error = 'Xác thực thành công! Vui lòng đăng nhập.';
            this.showOtpForm = false;
            this.otpForm.reset();
            this.otpValues = ['', '', '', '', '', ''];
            // Chuyển về trang đăng nhập sau 2 giây
            setTimeout(() => {
              this.router.navigate(['/login']);
            }, 2000);
          }
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          if (error.status === 409) {
            this.error = 'Mã OTP không chính xác.';
            this.remainingAttempts--;
            if (this.remainingAttempts === 0) {
              this.error = 'Bạn đã hết số lần thử. Vui lòng gửi lại mã OTP mới.';
            }
          } else {
            this.error = 'Có lỗi xảy ra. Vui lòng thử lại sau.';
          }
        }
      });
    }
  }

  startResendTimer() {
    this.resendTimer = 90; // Đặt thời gian đếm ngược là 90 giây
    
    // Hủy timer cũ nếu có
    if (this.timerSubscription) {
      this.timerSubscription.unsubscribe();
    }
    
    // Tạo timer mới
    this.timerSubscription = interval(1000).subscribe(() => {
      if (this.resendTimer > 0) {
        this.resendTimer--;
      } else {
        this.timerSubscription?.unsubscribe();
      }
    });
  }

  resendOtp(): void {
    if (this.resendTimer === 0) {
      this.isLoading = true;
      this.error = null;

      this.authService.resendOtp({ email: this.registeredEmail }).subscribe({
        next: (response) => {
          if (response.isSuccess && response.statusCode === 200) {
            this.error = 'Mã OTP mới đã được gửi đến email của bạn.';
            this.startResendTimer(); // Bắt đầu đếm ngược khi gửi OTP thành công
          } else {
            this.error = 'Không thể gửi lại mã OTP. Vui lòng thử lại sau.';
          }
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          if (error.status === 409) {
            this.error = 'Email này đã được kích hoạt. Vui lòng đăng nhập.';
            this.showOtpForm = false;
            this.router.navigate(['/login']);
          } else {
            this.error = 'Không thể gửi lại mã OTP. Vui lòng thử lại sau.';
          }
        }
      });
    }
  }

  private validateAllFormFields(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach(field => {
      const control = formGroup.get(field);
      if (control instanceof FormGroup) {
        this.validateAllFormFields(control);
      } else {
        control?.markAsTouched();
      }
    });
  }

  private passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null
      : { mismatch: true };
  }

  getErrorMessage(controlName: string, form: FormGroup): string {
    const control = form.get(controlName);
    if (control?.errors) {
      if (control.errors['required']) {
        return `${controlName === 'email' ? 'Email' : 'Mật khẩu'} không được để trống`;
      }
      if (control.errors['email']) {
        return 'Email không hợp lệ';
      }
      if (control.errors['minlength']) {
        return 'Mật khẩu phải có ít nhất 6 ký tự';
      }
      if (control.errors['mismatch']) {
        return 'Mật khẩu xác nhận không khớp';
      }
    }
    return '';
  }
}
