import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CouponService, Coupon } from '../../services/coupon.service';
import { VndCurrencyPipe } from '../../pipes/vnd-currency.pipe';
import { NotificationService } from '../../services/notification.service';
import { PageBannerComponent } from '../../components/page-banner/page-banner.component';
interface ExtendedCoupon extends Coupon {
  showTerm?: boolean;
  isSaved: boolean;
}

@Component({
  selector: 'app-coupon',
  standalone: true,
  imports: [CommonModule, VndCurrencyPipe, PageBannerComponent],
  templateUrl: './coupon.component.html',
  styleUrls: ['./coupon.component.css']
})
export class CouponComponent implements OnInit {
  coupons: {
    order: ExtendedCoupon[];
    ship: ExtendedCoupon[];
  } = {
    order: [],
    ship: []
  };
  loading = false;
  error: string | null = null;
  selectedCoupon: ExtendedCoupon | null = null;
  showNotification = false;
  notificationMessage = '';
  notificationType = '';

  constructor(
    private couponService: CouponService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadCoupons();
  }

  loadCoupons(): void {
    this.loading = true;
    this.couponService.getAllCoupons().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.coupons = {
            order: response.data.couponOrder.map(coupon => ({
              ...coupon,
              showTerm: false,
              isSaved: false
            })),
            ship: response.data.couponShip.map(coupon => ({
              ...coupon,
              showTerm: false,
              isSaved: false
            }))
          };
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Lỗi khi tải mã giảm giá:', error);
        this.error = 'Có lỗi xảy ra khi tải mã giảm giá';
        this.loading = false;
      }
    });
  }

  openTermModal(coupon: ExtendedCoupon): void {
    this.selectedCoupon = coupon;
  }

  closeTermModal(): void {
    this.selectedCoupon = null;
  }

  saveCoupon(coupon: ExtendedCoupon): void {
    this.couponService.saveCoupon(coupon.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          coupon.isSaved = true;
          this.notificationService.show('Đã lưu mã giảm giá thành công', 'success');
        } else {
          this.notificationService.show(response.error?.details?.[0] || 'Có lỗi xảy ra khi lưu mã giảm giá', 'info');
        }
      },
      error: (error) => {
        if (error.status === 409) {
          coupon.isSaved = true;
          this.notificationService.show('Bạn đã lưu mã giảm giá này trước đó', 'info');
        } else {
          this.notificationService.show('Có lỗi xảy ra khi lưu mã giảm giá', 'error');
        }
      }
    });
  }
} 