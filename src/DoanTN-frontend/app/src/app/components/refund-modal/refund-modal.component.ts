import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';

export interface RefundModalData {
  orderItemId: number;
  quantity: number;
}

@Component({
  selector: 'app-refund-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatRadioModule,
    MatDialogModule,
    MatButtonModule
  ],
  template: `
    <h2 mat-dialog-title>Yêu cầu trả hàng/hoàn tiền</h2>
    <mat-dialog-content>
      <form #refundForm="ngForm">
        <!-- Lý do trả hàng -->
        <mat-form-field appearance="outline" class="w-100">
          <mat-label>Lý do trả hàng</mat-label>
          <mat-select [(ngModel)]="selectedReason" name="reason" required>
            <mat-option *ngFor="let reason of reasons" [value]="reason.value">
              {{reason.label}}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <!-- Lý do khác -->
        <mat-form-field *ngIf="selectedReason === 'other'" appearance="outline" class="w-100">
          <mat-label>Nhập lý do khác</mat-label>
          <textarea matInput [(ngModel)]="otherReason" name="otherReason" required rows="3"></textarea>
        </mat-form-field>

        <!-- Upload file minh chứng (tối ưu giao diện) -->
        <div class="form-group">
          <label>Ảnh/Video minh chứng (tối đa 2MB cho ảnh, 5MB cho video):</label>
          <div class="custom-file-upload">
            <button type="button" class="upload-btn" (click)="fileInput.click()">
              <i class="fa fa-upload"></i> Chọn tệp minh chứng
            </button>
            <span class="file-name" *ngIf="selectedFileName">{{ selectedFileName }}</span>
            <input #fileInput type="file" (change)="onFileChange($event)" accept=".png,.jpg,.jpeg,.pdf,.webp,.mp4,.avi,.mov,.mkv,.flv" style="display: none;">
          </div>
          <div *ngIf="fileError" class="error">{{ fileError }}</div>
          <div *ngIf="imageData">
            <img *ngIf="isImageFile()" [src]="'data:image/*;base64,' + imageData" alt="Preview" style="max-width: 200px; max-height: 200px; margin-top: 10px;" />
            <video *ngIf="isVideoFile()" [src]="'data:video/*;base64,' + imageData" controls style="max-width: 200px; max-height: 200px; margin-top: 10px;"></video>
            <div *ngIf="isPdfFile()">
              <a [href]="'data:application/pdf;base64,' + imageData" target="_blank">Xem file PDF</a>
            </div>
          </div>
          <div class="note">Chỉ chấp nhận: png, jpg, jpeg, pdf, webp, mp4, avi, mov, mkv, flv</div>
        </div>

        <!-- Phương thức hoàn tiền -->
        <div class="payment-method">
          <label>Phương thức hoàn tiền:</label>
          <mat-radio-group [(ngModel)]="returnType" name="returnType" required>
            <mat-radio-button value="Bank">Chuyển khoản ngân hàng</mat-radio-button>
            <mat-radio-button value="Momo">Ví Momo</mat-radio-button>
            <mat-radio-button value="Other">Khác</mat-radio-button>
          </mat-radio-group>
        </div>

        <!-- Form ngân hàng -->
        <div *ngIf="returnType === 'Bank'" class="bank-info">
          <mat-form-field appearance="outline" class="w-100">
            <mat-label>Tên chủ tài khoản</mat-label>
            <input matInput [(ngModel)]="accountName" name="accountName" required>
          </mat-form-field>

          <mat-form-field appearance="outline" class="w-100">
            <mat-label>Số tài khoản</mat-label>
            <input matInput [(ngModel)]="accountNumber" name="accountNumber" required>
          </mat-form-field>

          <mat-form-field appearance="outline" class="w-100">
            <mat-label>Tên ngân hàng</mat-label>
            <input matInput [(ngModel)]="bankName" name="bankName" required>
          </mat-form-field>
        </div>

        <!-- Form Momo -->
        <div *ngIf="returnType === 'Momo'" class="momo-info">
          <mat-form-field appearance="outline" class="w-100">
            <mat-label>Số điện thoại Momo</mat-label>
            <input matInput [(ngModel)]="phoneNumber" name="phoneNumber" required>
          </mat-form-field>
        </div>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Hủy</button>
      <button mat-raised-button color="primary" (click)="onSubmit()" [disabled]="!isFormValid()">
        Gửi yêu cầu
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    :host {
      display: block;
      padding: 20px;
      max-width: 500px;
      padding-top: 40px;
    }
    .w-100 {
      width: 100%;
      margin-bottom: 16px;
    }
    .payment-method {
      margin: 20px 0;
    }
    mat-radio-group {
      display: flex;
      flex-direction: column;
      gap: 10px;
      margin-top: 10px;
    }
    .custom-file-upload {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-top: 8px;
    }
    .upload-btn {
      background: #007aff;
      color: #fff;
      border: none;
      padding: 8px 18px;
      border-radius: 6px;
      cursor: pointer;
      font-weight: 500;
      display: flex;
      align-items: center;
      gap: 6px;
      transition: background 0.2s;
    }
    .upload-btn:hover {
      background: #005ecb;
    }
    .file-name {
      font-size: 0.98rem;
      color: #333;
      max-width: 180px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
  `]
})
export class RefundModalComponent {
  reasons = [
    { value: 'Sản phẩm không đúng mô tả', label: 'Sản phẩm không đúng mô tả' },
    { value: 'Sản phẩm bị hỏng/lỗi', label: 'Sản phẩm bị hỏng/lỗi' },
    { value: 'Chất lượng không đạt yêu cầu', label: 'Chất lượng không đạt yêu cầu' },
    { value: 'other', label: 'Lý do khác' }
  ];

  selectedReason: string = '';
  otherReason: string = '';
  returnType: 'Bank' | 'Momo' | 'Other' = 'Bank';
  accountName: string = '';
  accountNumber: string = '';
  bankName: string = '';
  phoneNumber: string = '';
  imageData: string | null = null;
  fileError: string | null = null;
  lastFileExt: string | null = null;
  selectedFileName: string | null = null;
  permittedExtensions = [
    '.png', '.jpg', '.jpeg', '.pdf', '.webp',
    '.mp4', '.avi', '.mov', '.mkv', '.flv'
  ];

  constructor(
    public dialogRef: MatDialogRef<RefundModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: RefundModalData
  ) {}

  isFormValid(): boolean {
    if (!this.selectedReason) return false;
    if (this.selectedReason === 'other' && !this.otherReason) return false;

    switch (this.returnType) {
      case 'Bank':
        return !!(this.accountName && this.accountNumber && this.bankName);
      case 'Momo':
        return !!this.phoneNumber;
      case 'Other':
        return true;
      default:
        return false;
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onFileChange(event: Event) {
    this.fileError = null;
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      this.imageData = null;
      this.lastFileExt = null;
      this.selectedFileName = null;
      return;
    }
    const file = input.files[0];
    this.selectedFileName = file.name;
    const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    this.lastFileExt = ext;
    if (!this.permittedExtensions.includes(ext)) {
      this.fileError = 'Định dạng file không hợp lệ!';
      this.imageData = null;
      return;
    }
    if ((['.png', '.jpg', '.jpeg', '.pdf', '.webp'].includes(ext) && file.size > 2 * 1024 * 1024) ||
        (['.mp4', '.avi', '.mov', '.mkv', '.flv'].includes(ext) && file.size > 5 * 1024 * 1024)) {
      this.fileError = 'Dung lượng file vượt quá giới hạn!';
      this.imageData = null;
      return;
    }
    const reader = new FileReader();
    reader.onload = () => {
      const base64 = reader.result as string;
      this.imageData = base64.split(',')[1];
    };
    reader.readAsDataURL(file);
  }

  isImageFile(): boolean {
    return this.lastFileExt ? ['.png', '.jpg', '.jpeg', '.webp'].includes(this.lastFileExt) : false;
  }
  isVideoFile(): boolean {
    return this.lastFileExt ? ['.mp4', '.avi', '.mov', '.mkv', '.flv'].includes(this.lastFileExt) : false;
  }
  isPdfFile(): boolean {
    return this.lastFileExt === '.pdf';
  }

  onSubmit(): void {
    if (!this.isFormValid()) return;

    const request = {
      orderItemId: this.data.orderItemId,
      quantity: this.data.quantity,
      reason: this.selectedReason === 'other' ? this.otherReason : this.selectedReason,
      returnType: this.returnType,
      accountName: this.returnType === 'Bank' ? this.accountName : undefined,
      accountNumber: this.returnType === 'Bank' ? this.accountNumber : undefined,
      bankName: this.returnType === 'Bank' ? this.bankName : undefined,
      phoneNumber: this.returnType === 'Momo' ? this.phoneNumber : undefined,
      imageData: this.imageData || undefined
    };

    this.dialogRef.close(request);
  }
} 