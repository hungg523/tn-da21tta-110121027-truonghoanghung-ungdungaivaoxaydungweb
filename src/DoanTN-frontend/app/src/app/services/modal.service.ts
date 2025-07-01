import { Injectable } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { LoginModalComponent } from '../components/modals/login-modal/login-modal.component';
import { RegisterModalComponent } from '../components/modals/register-modal/register-modal.component';
import { ForgotPasswordModalComponent } from '../components/modals/forgot-password-modal/forgot-password-modal.component';
import { ChangePasswordModalComponent } from '../components/modals/change-password-modal/change-password-modal.component';

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  constructor(private modalService: NgbModal) {}

  openLoginModal() {
    this.closeAll();
    return this.modalService.open(LoginModalComponent, {
      centered: true,
      backdrop: 'static',
      keyboard: false
    });
  }

  openRegisterModal() {
    this.closeAll();
    return this.modalService.open(RegisterModalComponent, {
      centered: true,
      backdrop: 'static',
      keyboard: false
    });
  }

  openForgotPasswordModal() {
    this.closeAll();
    return this.modalService.open(ForgotPasswordModalComponent, {
      centered: true,
      backdrop: 'static',
      keyboard: false
    });
  }

  openChangePasswordModal() {
    this.closeAll();
    return this.modalService.open(ChangePasswordModalComponent, {
      centered: true,
      backdrop: 'static',
      keyboard: false
    });
  }

  closeAll() {
    this.modalService.dismissAll();
  }
} 