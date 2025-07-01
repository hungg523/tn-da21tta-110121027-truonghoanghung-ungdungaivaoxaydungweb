// cart.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService, CartItem, CartDetail } from '../../services/cart.service';
import { CartStateService } from '../../services/cart-state.service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { NotificationService } from '../../services/notification.service';
import { PageBannerComponent } from '../../components/page-banner/page-banner.component';
@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageBannerComponent],
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent implements OnInit, OnDestroy {
  cartItems: CartItem[] = [];
  totalPrice: number = 0;
  loading: boolean = true;
  error: string | null = null;
  showNotification = false;
  notificationMessage = '';
  notificationType = '';
  selectedItems: { [key: number]: boolean } = {};
  selectAll: boolean = false;

  private cartSubscription: Subscription;

  constructor(
    private cartService: CartService,
    private cartStateService: CartStateService,
    private router: Router,
    private notificationService: NotificationService
  ) {
    this.cartSubscription = this.cartService.cartDetail$.subscribe(cartDetail => {
      if (cartDetail) {
        this.cartItems = cartDetail.cartItems;
        this.totalPrice = cartDetail.totalPrice;
        this.cartStateService.updateCartCount(cartDetail.cartItems.length);
        this.initializeSelectedItems();
      } else {
        this.cartItems = [];
        this.totalPrice = 0;
        this.cartStateService.updateCartCount(0);
      }
      this.loading = false;
    });
  }

  ngOnInit(): void {
    this.loadCart();
  }

  ngOnDestroy(): void {
    if (this.cartSubscription) {
      this.cartSubscription.unsubscribe();
    }
  }

  loadCart(): void {
    this.loading = true;
    this.error = null;
  
    this.cartService.getCartDetail().subscribe({
      next: (response) => {
        if (!response.isSuccess) {
          this.error = 'Không thể tải thông tin giỏ hàng';
        }
      },
      error: (err) => {
        if (err.status === 404) {
          // Giỏ hàng không tồn tại => xử lý như giỏ rỗng
          this.cartService.clearCart().subscribe();
        } else {
          this.error = 'Đã xảy ra lỗi khi tải thông tin giỏ hàng';
        }
        this.loading = false;
      }
    });
  }  

  private initializeSelectedItems() {
    this.selectedItems = {};
    this.cartItems.forEach(item => {
      this.selectedItems[item.variantId] = false;
    });
    this.selectAll = false;
  }

  toggleSelectAll() {
    this.selectAll = !this.selectAll;
    this.cartItems.forEach(item => {
      this.selectedItems[item.variantId] = this.selectAll;
    });
  }

  toggleSelectItem(itemId: number) {
    this.selectedItems[itemId] = !this.selectedItems[itemId];
    this.updateSelectAllState();
  }

  private updateSelectAllState() {
    this.selectAll = this.cartItems.every(item => this.selectedItems[item.variantId]);
  }

  getSelectedItemsCount(): number {
    return Object.values(this.selectedItems).filter(selected => selected).length;
  }

  getSelectedItemsTotal(): number {
    return this.cartItems
      .filter(item => this.selectedItems[item.variantId])
      .reduce((total, item) => total + (item.discountPrice * item.quantity), 0);
  }

  proceedToCheckout() {
    const selectedItems = this.cartItems.filter(item => this.selectedItems[item.variantId]);

    if (selectedItems.length === 0) {
      this.notificationService.show('Vui lòng chọn ít nhất một sản phẩm để thanh toán', 'error');
      return;
    }

    // Chuyển đến trang thanh toán với thông tin sản phẩm đã chọn
    const orderItems = selectedItems.map(item => ({
      variantId: item.variantId,
      quantity: item.quantity,
      name: item.name,
      unitPrice: item.unitPrice,
      discountPrice: item.discountPrice,
      image: item.image
    }));

    // Lưu thông tin đơn hàng vào localStorage để sử dụng ở trang checkout
    localStorage.setItem('checkoutItems', JSON.stringify(orderItems));
    
    this.router.navigate(['/checkout']);
  }

  private showNotificationMessage(message: string, type: 'success' | 'error' = 'success') {
    this.showNotification = true;
    this.notificationMessage = message;
    this.notificationType = type;
    setTimeout(() => {
      this.showNotification = false;
    }, 3000);
  }

  updateQuantity(item: CartItem, newQuantity: number): void {
    if (newQuantity < 1) return;
    
    this.loading = true;
    this.cartService.updateCartQuantity({
      variantId: item.variantId,
      quantity: newQuantity
    }).subscribe({
      next: () => {
        this.loading = false;
        this.notificationService.show('Cập nhật số lượng thành công');
      },
      error: (error) => {
        console.error('Lỗi khi cập nhật số lượng:', error);
        this.loading = false;
        this.notificationService.show('Có lỗi xảy ra khi cập nhật số lượng', 'error');
        // Khôi phục lại số lượng cũ nếu có lỗi
        this.loadCart();
      }
    });
  }

  handleQuantityChange(item: CartItem, event: Event): void {
    const input = event.target as HTMLInputElement;
    const newQuantity = parseInt(input.value);
    if (!isNaN(newQuantity)) {
      this.updateQuantity(item, newQuantity);
    }
  }

  removeItem(variantId: number): void {
    this.loading = true;
    this.cartService.removeItem(variantId).subscribe({
      next: () => {
        this.loading = false;
        this.notificationService.show('Xóa sản phẩm thành công');
      },
      error: (error) => {
        console.error('Lỗi khi xóa sản phẩm:', error);
        this.loading = false;
        this.notificationService.show('Có lỗi xảy ra khi xóa sản phẩm', 'error');
      }
    });
  }

  clearCart(): void {
    this.loading = true;
    this.cartService.clearCart().subscribe({
      next: () => {
        this.loading = false;
        this.notificationService.show('Xóa giỏ hàng thành công');
      },
      error: (error) => {
        console.error('Lỗi khi xóa giỏ hàng:', error);
        this.loading = false;
        this.notificationService.show('Có lỗi xảy ra khi xóa giỏ hàng', 'error');
      }
    });
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  }
}
