// check-out.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CheckoutService, CreateOrderRequest, OrderItem } from '../../services/checkout.service';
import { UserAddress, UserAddressService, UserAddressResponse } from '../../services/address.service';
import { Coupon, CouponService } from '../../services/coupon.service';
import { NotificationService } from '../../services/notification.service';
import { PageBannerComponent } from '../../components/page-banner/page-banner.component';
import { AddressFormComponent } from '../../components/address-form/address-form.component';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageBannerComponent, AddressFormComponent],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.css']
})
export class CheckoutComponent implements OnInit {
  userAddressId: number = 0;
  couponId: number | null = null;
  shipCouponId: number | null = null;
  orderCouponCode: string = '';
  shipCouponCode: string = '';
  paymentMethod: string = 'COD'; // Mặc định là "COD" (Cash on Delivery)
  orderItems: OrderItem[] = [];
  loading: boolean = false;
  error: string | null = null;
  
  // Thêm các thuộc tính mới
  addresses: UserAddress[] = []; // Cần thay thế any bằng interface Address phù hợp
  coupons: {
    order: Coupon[];
    ship: Coupon[];
  } = {
    order: [],
    ship: []
  };
  subtotal: number = 0;
  shippingFee: number = 30000;
  discount: number = 0;
  shipDiscount: number = 0;
  total: number = 0;

  // Modal state
  showCouponModal: boolean = false;
  modalType: 'order' | 'ship' = 'order';
  selectedOrderCoupon: Coupon | null = null;
  selectedShipCoupon: Coupon | null = null;

  enteredCouponCode: string = ''; // Đổi tên biến code thành enteredCouponCode

  showAddressModal: boolean = false;
  selectedAddress: UserAddress | null = null;

  showAddressForm: boolean = false;
  editingAddress: UserAddress | null = null;

  constructor(
    private checkoutService: CheckoutService,
    private route: ActivatedRoute,
    private addressService: UserAddressService,
    private couponService: CouponService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    // Lấy thông tin sản phẩm từ query params (trường hợp mua ngay)
    this.route.queryParams.subscribe(params => {
      if (params['variantId'] && params['quantity']) {
        const orderItem: OrderItem = {
          variantId: +params['variantId'],
          quantity: +params['quantity'],
          name: params['name'],
          unitPrice: +params['unitPrice'],
          discountPrice: +params['discountPrice'],
          image: params['image']
        };
        this.orderItems = [orderItem];
        this.calculateTotals();
      } else {
        // Lấy thông tin sản phẩm từ localStorage (trường hợp thanh toán từ giỏ hàng)
        const checkoutItems = localStorage.getItem('checkoutItems');
        if (checkoutItems) {
          this.orderItems = JSON.parse(checkoutItems);
          this.calculateTotals();
        }
      }
    });

    // Load danh sách địa chỉ
    this.loadUserAddresses();
    // Load danh sách mã giảm giá
    this.loadUserCoupons();
  }

  loadUserAddresses(): void {
    this.addressService.getAllAddresses().subscribe({
      next: (response: UserAddressResponse) => {
        if (response.isSuccess) {
          this.addresses = response.data;
          // Chọn địa chỉ mặc định nếu có
          if (this.addresses.length > 0) {
            this.userAddressId = this.addresses[0].addressId;
          }
        }
      },
      error: (error) => {
        this.error = 'Không thể tải danh sách địa chỉ';
      }
    });
  }

  loadUserCoupons(): void {
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
      },
      error: (error) => {
        this.error = 'Không thể tải danh sách mã giảm giá';
      }
    });
  }

  openCouponModal(type: 'order' | 'ship'): void {
    this.modalType = type;
    this.showCouponModal = true;
  }

  closeCouponModal(): void {
    this.showCouponModal = false;
  }

  selectCoupon(coupon: Coupon): void {
    if (this.modalType === 'order') {
      this.selectedOrderCoupon = coupon;
      this.orderCouponCode = ''; // Reset mã nhập tay khi chọn coupon
    } else {
      this.selectedShipCoupon = coupon;
    }
    this.closeCouponModal();
    this.calculateTotals();
  }

  removeOrderCoupon(): void {
    this.selectedOrderCoupon = null;
    this.calculateTotals();
  }

  removeShipCoupon(): void {
    this.selectedShipCoupon = null;
    this.calculateTotals();
  }

  async applyCoupon() {
    if (!this.enteredCouponCode) return;

    try {
      this.loading = true;
      this.error = null;
      
      const response = await this.couponService.getCouponDetail(this.enteredCouponCode).toPromise();
      
      if (response?.isSuccess && response.data) {
        const coupon = response.data;
        
        // Kiểm tra điều kiện áp dụng
        if (this.subtotal < coupon.minOrderValue) {
          this.error = `Đơn hàng tối thiểu ${coupon.minOrderValue.toLocaleString('vi-VN')} VNĐ để áp dụng mã giảm giá này`;
          return;
        }

        // Kiểm tra số lần sử dụng
        if (coupon.timesUsed >= coupon.maxUsage) {
          this.error = 'Mã giảm giá đã hết lượt sử dụng';
          return;
        }

        // Nếu là mã giảm giá đơn hàng
        if (coupon.couponType === 'ORDER') {
          this.selectedOrderCoupon = {
            ...coupon,
            code: this.enteredCouponCode,
            discountDisplay: coupon.discountDisplay
          };
          this.enteredCouponCode = ''; // Reset mã sau khi áp dụng thành công
          this.calculateTotals();
        } else {
          this.error = 'Mã giảm giá không hợp lệ cho đơn hàng';
        }
      } else {
        this.error = 'Mã giảm giá không tồn tại hoặc đã hết hạn';
      }
    } catch (error) {
      this.error = 'Có lỗi xảy ra khi áp dụng mã giảm giá';
    } finally {
      this.loading = false;
    }
  }

  calculateTotals(): void {
    // Tính tổng tiền hàng
    this.subtotal = this.orderItems.reduce((total, item) => {
      return total + (item.discountPrice * item.quantity);
    }, 0);

    // Miễn phí vận chuyển nếu đơn hàng >= 2 triệu
    if (this.subtotal >= 2000000) {
      this.shippingFee = 0;
    } else {
      this.shippingFee = 30000;
    }
    
    // Tính giảm giá đơn hàng
    let orderDiscount = 0;
    if (this.selectedOrderCoupon) {
      if (this.selectedOrderCoupon.discountPercentage > 0) {
        orderDiscount = (this.subtotal * this.selectedOrderCoupon.discountPercentage) / 100;
        if (this.selectedOrderCoupon.maxDiscountAmount > 0) {
          orderDiscount = Math.min(orderDiscount, this.selectedOrderCoupon.maxDiscountAmount);
        }
      } else if (this.selectedOrderCoupon.discountAmount > 0) {
        orderDiscount = this.selectedOrderCoupon.discountAmount;
      }
    }

    // Tính giảm giá vận chuyển
    let shippingDiscount = 0;
    if (this.selectedShipCoupon) {
      if (this.selectedShipCoupon.discountPercentage > 0) {
        shippingDiscount = (this.shippingFee * this.selectedShipCoupon.discountPercentage) / 100;
        if (this.selectedShipCoupon.maxDiscountAmount > 0) {
          shippingDiscount = Math.min(shippingDiscount, this.selectedShipCoupon.maxDiscountAmount);
        }
      } else if (this.selectedShipCoupon.discountAmount > 0) {
        shippingDiscount = this.selectedShipCoupon.discountAmount;
      }
    }

    // Cập nhật tổng giảm giá và tổng tiền
    this.discount = orderDiscount + shippingDiscount;
    this.total = this.subtotal + this.shippingFee - this.discount;
  }

  calculateDiscountAmount(coupon: Coupon): number {
    if (coupon.discountAmount > 0) {
      return coupon.discountAmount;
    }
    
    if (coupon.discountPercentage > 0) {
      const baseAmount = coupon.couponType === 'ORDER' ? this.subtotal : this.shippingFee;
      let discount = (baseAmount * coupon.discountPercentage) / 100;
      
      if (coupon.maxDiscountAmount > 0) {
        discount = Math.min(discount, coupon.maxDiscountAmount);
      }
      
      return discount;
    }
    
    return 0;
  }

  submitOrder(): void {
    if (!this.userAddressId) {
      this.notificationService.show('Vui lòng chọn địa chỉ giao hàng', 'error');
      return;
    }

    this.loading = true;
    this.error = null;

    const orderRequest: CreateOrderRequest = {
      userAddressId: this.userAddressId,
      couponId: this.couponId,
      shipCouponId: this.shipCouponId,
      code: this.selectedOrderCoupon?.code || this.orderCouponCode,
      payment: this.paymentMethod,
      orderItems: this.orderItems
    };

    this.checkoutService.createOrder(orderRequest).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          if (this.paymentMethod === 'PAYOS' && response.data?.paymentUrl) {
            // Nếu thanh toán qua PayOS và có payment URL, mở URL trong tab mới
            window.location.href = response.data.paymentUrl;
            localStorage.removeItem('checkoutItems');
          } else {
            // Nếu thanh toán COD, hiển thị thông báo thành công
            this.notificationService.show('Đơn hàng của bạn đã được tạo thành công!');
            this.router.navigate(['/thank-you']);
            localStorage.removeItem('checkoutItems');
          }
        }
        this.loading = false;
      },
      error: (error) => {
        this.notificationService.show(error.message || 'Đã có lỗi xảy ra khi tạo đơn hàng', 'error');
        this.loading = false;
      }
    });
  }

  openAddressModal(): void {
    this.showAddressModal = true;
  }

  closeAddressModal(): void {
    this.showAddressModal = false;
  }

  selectAddress(address: UserAddress): void {
    this.selectedAddress = address;
    this.userAddressId = address.addressId;
    this.closeAddressModal();
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  }

  openAddressForm(address?: UserAddress): void {
    this.editingAddress = address || null;
    this.showAddressForm = true;
  }

  closeAddressForm(): void {
    this.showAddressForm = false;
    this.editingAddress = null;
  }

  onAddressAdded(): void {
    this.loadUserAddresses();
    this.closeAddressForm();
  }

  onAddressUpdated(): void {
    this.loadUserAddresses();
    this.closeAddressForm();
  }

  deleteAddress(addressId: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa địa chỉ này?')) {
      this.addressService.deleteAddress(addressId).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.notificationService.show('Xóa địa chỉ thành công');
            this.loadUserAddresses();
            if (this.selectedAddress?.addressId === addressId) {
              this.selectedAddress = null;
              this.userAddressId = 0;
            }
          }
        },
        error: (error) => {
          this.notificationService.show('Có lỗi xảy ra khi xóa địa chỉ', 'error');
        }
      });
    }
  }
}
