import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { CommonModule } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ModalService } from '../../../services/modal.service';

@Component({
  selector: 'app-login-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login-modal.component.html',
  styleUrls: ['./login-modal.component.css']
})
export class LoginModalComponent implements OnInit {
  loginForm: FormGroup;
  isLoading = false;
  error: string | null = null;
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    public activeModal: NgbActiveModal,
    private modalService: ModalService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.activeModal.close();
    }
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.error = null;
  
      this.authService.login(this.loginForm.value).subscribe({
        next: (response) => {
          if (response.accessToken) {
            this.authService.setAccessToken(response.accessToken);
            this.activeModal.close();
            window.location.href = '/';
          }
          this.isLoading = false;
        },
        error: (err) => {
          if (err.status === 400 && err.details) {
            if (err.details[0] === 'Password is incorrect.') {
              this.error = 'Mật khẩu không đúng, vui lòng thử lại.';
            } else {
              this.error = err.details[0];
            }
            this.loginForm.patchValue({ password: '' });
          } else {
            this.error = err.message || 'Đã xảy ra lỗi khi đăng nhập. Vui lòng thử lại.';
          }
          this.isLoading = false;
        }
      });
    } else {
      this.validateAllFormFields(this.loginForm);
    }
  }

  loginWithGoogle(): void {
    this.isLoading = true;
    this.error = null;
    this.authService.loginWithGoogle();
  }

  loginWithFacebook(): void {
    this.isLoading = true;
    this.error = null;
    this.authService.loginWithFacebook();
  }

  openRegisterModal(): void {
    this.activeModal.close();
    this.modalService.openRegisterModal();
  }

  openForgotPasswordModal(): void {
    this.activeModal.close();
    this.modalService.openForgotPasswordModal();
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
        return `${controlName === 'email' ? 'Email' : 'Mật khẩu'} không được để trống`;
      }
      if (control.errors['email']) {
        return 'Email không hợp lệ';
      }
      if (control.errors['minlength']) {
        return 'Mật khẩu phải có ít nhất 6 ký tự';
      }
    }
    return '';
  }
} 