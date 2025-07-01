import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ReturnDetail } from '../../services/return.service';

@Component({
  selector: 'app-return-detail-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="return-detail-modal">
      <div class="modal-header">
        <h2>Chi tiết đơn trả hàng #{{data.returnId}}</h2>
        <button type="button" class="close-btn" (click)="close()">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <div class="modal-body">
        <div class="product-info">
          <div class="product-image">
            <img [src]="data.imageUrl" [alt]="data.name">
          </div>
          <div class="product-details">
            <h3>{{data.name}}</h3>
            <p class="variant">{{data.productAttribute}}</p>
            <p class="quantity">Số lượng trả: {{data.quantity}}</p>
            <p class="refund">Số tiền hoàn trả: {{formatPrice(data.refundAmount)}}</p>
          </div>
        </div>

        <!-- Minh chứng trả hàng -->
        <div class="evidence-info" *ngIf="data.returnUrl">
          <h3>
            <i class="fas fa-paperclip"></i>
            Minh chứng trả hàng
          </h3>
          <div class="evidence-preview">
            <img *ngIf="isImageFile(data.returnUrl)" [src]="data.returnUrl" alt="Minh chứng" style="max-width: 200px; max-height: 200px;" />
            <video *ngIf="isVideoFile(data.returnUrl)" [src]="data.returnUrl" controls style="max-width: 200px; max-height: 200px;"></video>
            <div *ngIf="isPdfFile(data.returnUrl)">
              <a [href]="data.returnUrl" target="_blank">Xem file PDF</a>
            </div>
          </div>
        </div>

        <div class="status-info">
          <div class="status-row">
            <span class="label">Trạng thái:</span>
            <span class="status" [ngClass]="getStatusClass(data.status)">
              {{getStatusText(data.status)}}
            </span>
          </div>
          <div class="status-row">
            <span class="label">Ngày yêu cầu:</span>
            <span>{{data.createdAt | date:'dd/MM/yyyy HH:mm'}}</span>
          </div>
          <div class="status-row" *ngIf="data.processedAt">
            <span class="label">Ngày xử lý:</span>
            <span>{{data.processedAt | date:'dd/MM/yyyy HH:mm'}}</span>
          </div>
        </div>

        <div class="address-info">
          <h3>
            <i class="fas fa-map-marker-alt"></i>
            Thông tin địa chỉ
          </h3>
          <div class="info-card">
            <p><strong>Người nhận:</strong> {{data.userAddresses.fullName}}</p>
            <p><strong>Số điện thoại:</strong> {{data.userAddresses.phoneNumber}}</p>
            <p><strong>Địa chỉ:</strong> {{data.userAddresses.address}}</p>
          </div>
        </div>

        <div class="refund-info" *ngIf="data.returnType !== 'Other'">
          <h3>
            <i class="fas fa-money-bill-wave"></i>
            Thông tin hoàn tiền
          </h3>
          <div class="info-card">
            <ng-container *ngIf="data.returnType === 'Bank'">
              <p><strong>Tên tài khoản:</strong> {{data.accountName}}</p>
              <p><strong>Số tài khoản:</strong> {{data.accountNumber}}</p>
              <p><strong>Ngân hàng:</strong> {{data.bankName}}</p>
            </ng-container>
            <ng-container *ngIf="data.returnType === 'Momo'">
              <p><strong>Số điện thoại Momo:</strong> {{data.phoneNumber}}</p>
            </ng-container>
          </div>
        </div>
      </div>

      <div class="modal-footer">
        <button type="button" class="close-btn" (click)="close()">
          <i class="fas fa-times"></i>
          Đóng
        </button>
      </div>
    </div>
  `,
  styles: [`
    .return-detail-modal {
      padding: 16px 12px;
      max-width: 700px;
      width: 100%;
      background: #ffffff;
      border-radius: 16px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
      max-height: 90vh;
      overflow-y: auto;
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
      padding-bottom: 16px;
      border-bottom: 1px solid #e2e8f0;
    }

    .modal-header h2 {
      margin: 0;
      font-size: 1.5rem;
      color: #1a1f36;
      font-weight: 600;
    }

    .close-btn {
      background: none;
      border: none;
      color: #64748b;
      cursor: pointer;
      font-size: 1.25rem;
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 16px;
      border-radius: 8px;
      transition: all 0.2s ease;
    }

    .close-btn:hover {
      background: #f1f5f9;
      color: #1e293b;
    }

    .product-info {
      display: flex;
      gap: 24px;
      margin-bottom: 24px;
      padding: 16px;
      background: #f8fafc;
      border-radius: 12px;
    }

    .product-image {
      width: 120px;
      height: 120px;
      border-radius: 12px;
      overflow: hidden;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
    }

    .product-image img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .product-details h3 {
      margin: 0 0 12px;
      font-size: 1.25rem;
      color: #1a1f36;
      font-weight: 600;
    }

    .variant {
      color: #64748b;
      margin: 0 0 8px;
      font-size: 0.95rem;
    }

    .quantity {
      margin: 8px 0;
      color: #1e293b;
      font-size: 0.95rem;
    }

    .refund {
      font-weight: 600;
      color: #e11d48;
      font-size: 1.1rem;
    }

    .status-info {
      margin-bottom: 24px;
      padding: 16px;
      background: #f8fafc;
      border-radius: 12px;
    }

    .status-row {
      display: flex;
      align-items: center;
      margin-bottom: 12px;
    }

    .status-row:last-child {
      margin-bottom: 0;
    }

    .label {
      width: 140px;
      color: #64748b;
      font-size: 0.95rem;
    }

    .status {
      padding: 6px 16px;
      border-radius: 20px;
      font-size: 0.9rem;
      font-weight: 500;
    }

    .status-pending {
      background: #fff7ed;
      color: #c2410c;
    }

    .status-delivered {
      background: #f0fdf4;
      color: #15803d;
    }

    .status-cancelled {
      background: #fef2f2;
      color: #b91c1c;
    }

    .address-info, .refund-info {
      margin-bottom: 24px;
    }

    .address-info h3, .refund-info h3 {
      font-size: 1.1rem;
      margin: 0 0 16px;
      color: #1e293b;
      font-weight: 600;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .info-card {
      background: #f8fafc;
      padding: 16px;
      border-radius: 12px;
    }

    .info-card p {
      margin: 8px 0;
      color: #1e293b;
      font-size: 0.95rem;
    }

    .info-card strong {
      color: #64748b;
      margin-right: 8px;
    }

    .modal-footer {
      margin-top: 24px;
      padding-top: 16px;
      border-top: 1px solid #e2e8f0;
      text-align: right;
    }

    @media (max-width: 640px) {
      .return-detail-modal {
        padding: 16px;
      }

      .product-info {
        flex-direction: column;
        align-items: center;
        text-align: center;
      }

      .product-image {
        width: 160px;
        height: 160px;
      }

      .status-row {
        flex-direction: column;
        align-items: flex-start;
        gap: 4px;
      }

      .label {
        width: 100%;
      }
    }
  `]
})
export class ReturnDetailModalComponent {
  constructor(
    private dialogRef: MatDialogRef<ReturnDetailModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ReturnDetail
  ) {}

  getStatusText(status: string): string {
    switch (status.toLowerCase()) {
      case 'pendingreturn':
        return 'Đang chờ xử lý trả hàng';
      case 'approvedreturn':
        return 'Đã chấp nhận trả hàng';
      case 'rejectedreturn':
        return 'Đã từ chối trả hàng';
      default:
        return status;
    }
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'pendingreturn':
        return 'status-pending';
      case 'approvedreturn':
        return 'status-delivered';
      case 'rejectedreturn':
        return 'status-cancelled';
      default:
        return '';
    }
  }

  close(): void {
    this.dialogRef.close();
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  }

  isImageFile(url: string): boolean {
    return /\.(png|jpg|jpeg|webp)$/i.test(url);
  }

  isVideoFile(url: string): boolean {
    return /\.(mp4|avi|mov|mkv|flv)$/i.test(url);
  }

  isPdfFile(url: string): boolean {
    return /\.pdf$/i.test(url);
  }
} 