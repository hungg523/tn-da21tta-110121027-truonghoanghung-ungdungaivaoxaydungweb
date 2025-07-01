import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { CommonModule } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ModalService } from '../../../services/modal.service';

@Component({
  selector: 'app-forgot-password-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="modal-header">
      <h4 class="modal-title">{{ isResetMode ? 'Đặt lại mật khẩu' : 'Quên mật khẩu' }}</h4>
      <button type="button" class="btn-close" aria-label="Close" (click)="activeModal.dismiss('Cross click')"></button>
    </div>

    <div class="modal-body">
      <form [formGroup]="resetForm" (ngSubmit)="onSubmit()" class="forgot-password-form">
        <!-- Email input - chỉ hiển thị khi không ở chế độ reset -->
        <div class="form-group" *ngIf="!isResetMode">
          <label for="email">Email</label>
          <div class="input-with-icon">
            <i class="bx bx-envelope"></i>
            <input type="email" 
                   id="email" 
                   formControlName="email" 
                   placeholder="Nhập email của bạn">
          </div>
          <div class="error-message" *ngIf="resetForm.get('email')?.touched && resetForm.get('email')?.invalid">
            {{ getErrorMessage('email') }}
          </div>
        </div>

        <!-- Password input - chỉ hiển thị khi ở chế độ reset -->
        <div class="form-group" *ngIf="isResetMode">
          <label for="newPassword">Mật khẩu mới</label>
          <div class="input-with-icon">
            <i class="bx bx-lock-alt"></i>
            <input [type]="showPassword ? 'text' : 'password'" 
                   id="newPassword" 
                   formControlName="newPassword" 
                   placeholder="Nhập mật khẩu mới">
            <i class="bx toggle-password" 
               [class.bx-show]="!showPassword" 
               [class.bx-hide]="showPassword" 
               (click)="togglePassword()">
            </i>
          </div>
          <div class="error-message" *ngIf="resetForm.get('newPassword')?.touched && resetForm.get('newPassword')?.invalid">
            {{ getErrorMessage('newPassword') }}
          </div>
        </div>

        <button type="submit" class="reset-button" [disabled]="isLoading || !resetForm.valid">
          <span *ngIf="!isLoading">{{ isResetMode ? 'Đặt lại mật khẩu' : 'Gửi yêu cầu' }}</span>
          <div class="spinner" *ngIf="isLoading">
            <div class="bounce1"></div>
            <div class="bounce2"></div>
            <div class="bounce3"></div>
          </div>
        </button>

        <div class="message-box" *ngIf="error" [class.error]="!error.includes('thành công')" [class.success]="error.includes('thành công')">
          {{ error }}
        </div>

        <div class="login-link">
          <a (click)="openLoginModal()">Quay lại đăng nhập</a>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .forgot-password-form {
      display: flex;
      flex-direction: column;
      gap: 24px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .form-group label {
      color: #1a1a1a;
      font-size: 14px;
      font-weight: 500;
    }

    .input-with-icon {
      position: relative;
      display: flex;
      align-items: center;
    }

    .input-with-icon input {
      width: 100%;
      padding: 12px 16px 12px 40px;
      border: 2px solid #e0e0e0;
      border-radius: 12px;
      font-size: 15px;
      transition: all 0.3s ease;
    }

    .input-with-icon input:focus {
      border-color: #667eea;
      outline: none;
    }

    .input-with-icon i {
      position: absolute;
      left: 12px;
      color: #666;
      font-size: 20px;
    }

    .input-with-icon .toggle-password {
      left: auto;
      right: 12px;
      cursor: pointer;
    }

    .reset-button {
      background: linear-gradient(to right, #667eea, #764ba2);
      color: white;
      border: none;
      border-radius: 12px;
      padding: 14px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s ease;
    }

    .reset-button:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
    }

    .reset-button:disabled {
      background: #e0e0e0;
      cursor: not-allowed;
      transform: none;
      box-shadow: none;
    }

    .spinner {
      display: flex;
      justify-content: center;
      gap: 4px;
    }

    .spinner > div {
      width: 8px;
      height: 8px;
      background-color: white;
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

    .message-box {
      text-align: center;
      padding: 12px;
      border-radius: 8px;
      font-size: 14px;
    }

    .message-box.error {
      background-color: #fee2e2;
      color: #dc2626;
      border: 1px solid #fecaca;
    }

    .message-box.success {
      background-color: #dcfce7;
      color: #16a34a;
      border: 1px solid #bbf7d0;
    }

    .login-link {
      text-align: center;
    }

    .login-link a {
      color: #667eea;
      text-decoration: none;
      font-weight: 500;
      cursor: pointer;
      transition: color 0.3s ease;
    }

    .login-link a:hover {
      color: #764ba2;
    }
  `]
})
export class ForgotPasswordModalComponent implements OnInit {
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
    public activeModal: NgbActiveModal,
    private modalService: ModalService
  ) {
    this.resetForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit(): void {}

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.resetForm.valid) {
      this.isLoading = true;
      this.error = null;

      if (this.isResetMode && this.resetToken) {
        this.authService.resetPassword({
          token: this.resetToken,
          newPassword: this.resetForm.get('newPassword')?.value
        }).subscribe({
          next: () => {
            this.error = 'Đặt lại mật khẩu thành công! Vui lòng đăng nhập.';
            setTimeout(() => {
              this.activeModal.close();
              this.modalService.openLoginModal();
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

  openLoginModal(): void {
    this.activeModal.close();
    this.modalService.openLoginModal();
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