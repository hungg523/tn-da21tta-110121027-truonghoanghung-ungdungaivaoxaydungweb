import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CouponService, Coupon, CouponResponse } from '../../services/coupon.service';
import { VndCurrencyPipe } from '../../pipes/vnd-currency.pipe';
import { PageBannerComponent } from '../../components/page-banner/page-banner.component';

interface ExtendedCoupon extends Coupon {
  showTerm?: boolean;
  isSaved: boolean;
}

@Component({
  selector: 'app-coupon-list',
  standalone: true,
  imports: [CommonModule, VndCurrencyPipe, PageBannerComponent],
  templateUrl: './coupon-list.component.html',
  styleUrls: ['./coupon-list.component.css']
})
export class CouponListComponent implements OnInit {
  coupons: {
    order: ExtendedCoupon[];
    ship: ExtendedCoupon[];
  } = {
    order: [],
    ship: []
  };
  selectedCoupon: ExtendedCoupon | null = null;
  isLoading = false;
  error: string | null = null;

  constructor(private couponService: CouponService) {}

  ngOnInit(): void {
    this.loadUserCoupons();
  }

  loadUserCoupons(): void {
    this.isLoading = true;
    this.error = null;

    this.couponService.getUserCoupons().subscribe({
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
        this.isLoading = false;
      },
      error: (err: any) => {
        this.error = 'Không thể tải danh sách voucher. Vui lòng thử lại sau.';
        this.isLoading = false;
        console.error('Error loading user coupons:', err);
      }
    });
  }

  openTermModal(coupon: ExtendedCoupon): void {
    this.selectedCoupon = coupon;
  }

  closeTermModal(): void {
    this.selectedCoupon = null;
  }
} 