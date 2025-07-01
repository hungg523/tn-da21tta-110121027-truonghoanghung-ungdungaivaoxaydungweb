import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { NotificationService } from '../../services/notification.service';
import { ModalService } from '../../services/modal.service';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css']
})
export class ChangePasswordComponent implements OnInit {
  changePasswordForm: FormGroup;
  isLoading = false;
  error: string | null = null;
  successMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router,
    private modalService: ModalService
  ) {
    this.changePasswordForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, {
      validator: this.passwordMatchValidator
    });
  }

  ngOnInit(): void {
    this.modalService.openChangePasswordModal().result.finally(() => {
      // Không cần navigate ở đây nữa vì đã navigate ngay sau khi mở modal
    });
    // Điều hướng ngay về trang chủ để không render lại page
    this.router.navigate(['/']);
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmPassword')?.value
      ? null
      : { mismatch: true };
  }

  onSubmit(): void {
    if (this.changePasswordForm.valid) {
      this.isLoading = true;
      this.error = null;
      this.successMessage = null;

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
            this.router.navigate(['/']);
          } else {
            this.notificationService.show('Đã có lỗi xảy ra. Vui lòng thử lại sau.');
            this.router.navigate(['/']);
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

  getErrorMessage(controlName: string, form: FormGroup): string {
    const control = form.get(controlName);
    if (control?.errors) {
      if (control.errors['required']) {
        return `${controlName === 'newPassword' ? 'Mật khẩu mới' : 'Mật khẩu xác nhận'} không được để trống`;
      }
      if (control.errors['minlength']) {
        return 'Mật khẩu phải có ít nhất 6 ký tự';
      }
    }
    return '';
  }
}
