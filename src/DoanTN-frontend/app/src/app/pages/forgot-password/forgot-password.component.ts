import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { ModalService } from '../../services/modal.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent implements OnInit {
  resetForm: FormGroup;
  isLoading = false;
  error: string | null = null;
  showPassword = false;
  resetToken: string | null = null;
  isResetMode = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private modalService: ModalService
  ) {
    // Khởi tạo form chỉ với email
    this.resetForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit(): void {
    // Kiểm tra token từ URL
    this.route.queryParams.subscribe(params => {
      if (params['token']) {
        this.resetToken = params['token'];
        this.isResetMode = true;
        
        // Reset form với chỉ password control
        this.resetForm = this.fb.group({
          newPassword: ['', [
            Validators.required,
            Validators.minLength(6),
            Validators.maxLength(255)
          ]]
        });
      }
    });

    this.modalService.openForgotPasswordModal().result.finally(() => {
      // Không cần navigate ở đây nữa vì đã navigate ngay sau khi mở modal
    });
    // Điều hướng ngay về trang chủ để không render lại page
    this.router.navigate(['/']);
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.resetForm.valid) {
      this.isLoading = true;
      this.error = null;

      if (this.isResetMode && this.resetToken) {
        // Gọi API reset password với token
        this.authService.resetPassword({
          token: this.resetToken,
          newPassword: this.resetForm.get('newPassword')?.value
        }).subscribe({
          next: () => {
            this.error = 'Đặt lại mật khẩu thành công! Vui lòng đăng nhập.';
            setTimeout(() => {
              this.router.navigate(['/login']);
            }, 2000);
          },
          error: (error) => {
            this.error = error;
            this.isLoading = false;
          },
          complete: () => {
            this.isLoading = false;
          }
        });
      } else {
        // Gọi API verify reset password với email
        this.authService.verifyResetPassword({
          email: this.resetForm.get('email')?.value
        }).subscribe({
          next: () => {
            this.error = 'Vui lòng kiểm tra email của bạn để tiếp tục quá trình đặt lại mật khẩu.';
            this.isLoading = false;
          },
          error: (error) => {
            this.error = error;
            this.isLoading = false;
          }
        });
      }
    } else {
      this.validateAllFormFields(this.resetForm);
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

  getErrorMessage(controlName: string): string {
    const control = this.resetForm.get(controlName);
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
      if (control.errors['maxlength']) {
        return 'Mật khẩu không được vượt quá 255 ký tự';
      }
    }
    return '';
  }
} 