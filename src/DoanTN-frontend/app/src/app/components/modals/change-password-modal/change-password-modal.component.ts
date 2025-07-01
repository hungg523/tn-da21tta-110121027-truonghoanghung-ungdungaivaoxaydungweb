import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { CommonModule } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { NotificationService } from '../../../services/notification.service';

@Component({
  selector: 'app-change-password-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="modal-header">
      <h4 class="modal-title">Đổi mật khẩu</h4>
      <button type="button" class="btn-close" aria-label="Close" (click)="activeModal.dismiss('Cross click')"></button>
    </div>

    <div class="modal-body">
      <form [formGroup]="changePasswordForm" (ngSubmit)="onSubmit()" class="change-password-form">
        <div class="form-group">
          <label for="newPassword">Mật khẩu mới</label>
          <div class="input-wrapper">
            <input 
              [type]="showPassword ? 'text' : 'password'"
              id="newPassword" 
              formControlName="newPassword"
              placeholder="Nhập mật khẩu mới"
              [class.error]="changePasswordForm.get('newPassword')?.invalid && changePasswordForm.get('newPassword')?.touched"
            >
            <i class="bx bx-lock-alt"></i>
            <i class="bx toggle-password" 
               [class.bx-show]="!showPassword" 
               [class.bx-hide]="showPassword" 
               (click)="togglePassword()">
            </i>
          </div>
          <div class="error-message" *ngIf="changePasswordForm.get('newPassword')?.invalid && changePasswordForm.get('newPassword')?.touched">
            Mật khẩu phải có ít nhất 6 ký tự
          </div>
        </div>

        <div class="form-group">
          <label for="confirmPassword">Xác nhận mật khẩu mới</label>
          <div class="input-wrapper">
            <input 
              [type]="showPassword ? 'text' : 'password'"
              id="confirmPassword" 
              formControlName="confirmPassword"
              placeholder="Nhập lại mật khẩu mới"
              [class.error]="changePasswordForm.get('confirmPassword')?.invalid && changePasswordForm.get('confirmPassword')?.touched"
            >
            <i class="bx bx-lock-alt"></i>
          </div>
          <div class="error-message" *ngIf="changePasswordForm.get('confirmPassword')?.invalid && changePasswordForm.get('confirmPassword')?.touched">
            Mật khẩu không khớp
          </div>
        </div>

        <button type="submit" class="submit-btn" [disabled]="changePasswordForm.invalid || isLoading">
          <span *ngIf="!isLoading">Đổi mật khẩu</span>
          <div class="spinner" *ngIf="isLoading">
            <div class="bounce1"></div>
            <div class="bounce2"></div>
            <div class="bounce3"></div>
          </div>
        </button>
      </form>
    </div>
  `,
  styles: [`
    .change-password-form {
      display: flex;
      flex-direction: column;
      gap: 24px;
      padding: 20px 0;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .form-group label {
      color: #2c3e50;
      font-weight: 500;
      font-size: 14px;
    }

    .input-wrapper {
      position: relative;
    }

    .input-wrapper i {
      position: absolute;
      left: 15px;
      top: 50%;
      transform: translateY(-50%);
      color: #95a5a6;
    }

    .input-wrapper .toggle-password {
      left: auto;
      right: 15px;
      cursor: pointer;
    }

    input {
      width: 100%;
      padding: 14px 15px 14px 45px;
      border: 2px solid #ecf0f1;
      border-radius: 10px;
      font-size: 16px;
      transition: all 0.3s ease;
      background-color: #f8f9fa;
    }

    input:focus {
      outline: none;
      border-color: #3498db;
      background-color: white;
      box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
    }

    input.error {
      border-color: #e74c3c;
    }

    .error-message {
      color: #e74c3c;
      font-size: 12px;
      margin-top: 2px;
    }

    .submit-btn {
      width: 100%;
      padding: 16px;
      background: linear-gradient(135deg, #3498db 0%, #2980b9 100%);
      color: white;
      border: none;
      border-radius: 10px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
      transition: all 0.3s ease;
    }

    .submit-btn:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 5px 15px rgba(52, 152, 219, 0.3);
    }

    .submit-btn:disabled {
      background: #bdc3c7;
      cursor: not-allowed;
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
  `]
})
export class ChangePasswordModalComponent implements OnInit {
  changePasswordForm: FormGroup;
  isLoading = false;
  error: string | null = null;
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    public activeModal: NgbActiveModal
  ) {
    this.changePasswordForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, {
      validator: this.passwordMatchValidator
    });
  }

  ngOnInit(): void {}

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmPassword')?.value
      ? null
      : { mismatch: true };
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.changePasswordForm.valid) {
      this.isLoading = true;
      this.error = null;

      const { newPassword, confirmPassword } = this.changePasswordForm.value;

      if (newPassword !== confirmPassword) {
        this.error = 'Mật khẩu xác nhận không khớp';
        this.isLoading = false;
        return;
      }

      this.authService.changePassword({ newPassword, confirmPassword }).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.notificationService.show('Mật khẩu đã được thay đổi thành công.');
            this.activeModal.close();
          } else {
            this.notificationService.show('Đã có lỗi xảy ra. Vui lòng thử lại sau.');
          }
          this.isLoading = false;
        },
        error: (err) => {
          this.notificationService.show(err);
          this.isLoading = false;
        }
      });
    } else {
      this.validateAllFormFields(this.changePasswordForm);
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
} 