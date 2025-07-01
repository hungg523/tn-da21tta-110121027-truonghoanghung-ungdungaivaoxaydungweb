import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { WishlistService, WishlistItem } from '../../services/wishlist.service';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { PageBannerComponent } from '../../components/page-banner/page-banner.component';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    RouterModule,
    PageBannerComponent
  ],
  templateUrl: './wishlist.component.html',
  styleUrls: ['./wishlist.component.css']
})
export class WishlistComponent implements OnInit {
  searchQuery: string = '';
  email: string = '';
  cartItems: any[] = [];
  wishlistItems: WishlistItem[] = [];
  cartTotal: number = 0;
  isLoading = false;

  constructor(
    private wishlistService: WishlistService,
    private authService: AuthService,
    private notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.loadWishlistItems();
    }
  }

  loadWishlistItems(): void {
    this.isLoading = true;
    this.wishlistService.getWishlistItems().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.wishlistItems = response.data;
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.notificationService.show('Không thể tải danh sách yêu thích', 'error');
      }
    });
  }

  onSubscribe(): void {
    // Xử lý đăng ký newsletter
    console.log('Subscribing with email:', this.email);
  }

  scrollToTop(): void {
    // Xử lý scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  addToCart(item: any): void {
    // Xử lý thêm vào giỏ hàng
    this.cartItems.push({...item, quantity: 1});
    this.calculateCartTotal();
  }

  removeFromCart(item: any): void {
    // Xử lý xóa khỏi giỏ hàng
    const index = this.cartItems.findIndex(i => i.id === item.id);
    if (index > -1) {
      this.cartItems.splice(index, 1);
      this.calculateCartTotal();
    }
  }

  removeFromWishlist(variantId: number): void {
    this.wishlistService.changeWishlistStatus(variantId, false).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.wishlistItems = this.wishlistItems.filter(item => item.variantId !== variantId);
          this.notificationService.show('Đã xóa khỏi danh sách yêu thích');
        }
      },
      error: () => {
        this.notificationService.show('Có lỗi xảy ra khi xóa khỏi danh sách yêu thích', 'error');
      }
    });
  }

  calculateCartTotal(): void {
    // Tính tổng giá trị giỏ hàng
    this.cartTotal = this.cartItems.reduce((total, item) => total + (item.price * item.quantity), 0);
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  }
}
