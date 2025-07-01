import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  ProductAttributes,
  ProductService,
  ProductVariant,
} from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import {
  ReviewService,
  Review
} from '../../services/review.service';
import { ReportReviewRequest } from '../../services/review.service';
import { NotificationService } from '../../services/notification.service';
import { WishlistService } from '../../services/wishlist.service';
import { AuthService } from '../../services/auth.service';
import { PageBannerComponent } from '../../components/page-banner/page-banner.component';
@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageBannerComponent],
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css'],
})
export class ProductDetailComponent implements OnInit {
  product: ProductVariant | null = null;
  similarProducts: ProductVariant[] = [];
  reviews: Review[] = [];
  selectedStar: number | null = null;
  isFilterByImage: boolean | null = null;
  loading = false;
  error: string | null = null;
  currentImageIndex = 0;
  defaultImage = 'assets/images/no-image.png';
  reviewSummary: any = null;
  quantity: number = 1;
  isInWishlist: boolean = false;
  showNotification = false;
  notificationMessage = '';
  notificationType = '';
  isAddedToCart: boolean = false;
  selectedAttributes: { [key: string]: string } = {};
  colorOptions: ProductAttributes[] = [];
  pageSize: number = 5;
  currentPage: number = 1;
  totalPages: number = 0;
  showReportModal = false;
  selectedReportReason = '';
  customReportReason = '';
  reportReasons = [
    { value: 'OFFENSIVE', label: 'Mang tính xúc phạm' },
    { value: 'GAMBLING', label: 'Cờ bạc' },
    { value: 'SEXUAL', label: 'Hành vi tình dục' },
    { value: 'SPAM', label: 'Spam' },
    { value: 'FAKE', label: 'Thông tin giả mạo' },
    { value: 'OTHER', label: 'Khác' }
  ];
  currentReviewId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private reviewService: ReviewService,
    private cartService: CartService,
    private notificationService: NotificationService,
    private wishlistService: WishlistService,
    private authService: AuthService
  ) {
    // Lắng nghe sự kiện NavigationEnd để cuộn lên đầu trang
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        window.scrollTo(0, 0);
      }
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const id = +params['id'];
      if (id) {
        this.loadProductDetail(id);
      }
    });
    this.loadReviewSummary();
    this.setupTabHandlers();
  }

  loadProductDetail(id: number): void {
    this.loading = true;
    this.error = null;
    this.productService.getProductDetail(id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.product = response.data;
          this.productService.getColorOptions(id).subscribe({
            next: (colors) => {
              this.colorOptions = colors.data;
            },
          });
          this.loadReviews(id);
          this.loadSimilarProducts(id);
          // Nếu sản phẩm không có ảnh, tạo một mảng ảnh mặc định
          if (!this.product.images || this.product.images.length === 0) {
            this.product.images = [
              {
                id: 0,
                title: 'No Image',
                url: this.defaultImage,
                position: 0,
              },
            ];
          }
          // Kiểm tra trạng thái wishlist
          this.checkWishlistStatus();
        } else {
          this.error = response.message || 'Không thể tải thông tin sản phẩm';
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Đã xảy ra lỗi khi tải thông tin sản phẩm';
        this.loading = false;
      },
    });
  }

  loadSimilarProducts(id: number): void {
    this.productService.getSimilarProducts(id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.similarProducts = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading similar products:', error);
      }
    });
  }

  loadReviews(variantId: number, page: number = 1): void {
    this.loading = true;
    this.reviewService.getReviews(
      variantId,
      page,
      this.pageSize,
      this.selectedStar,
      this.isFilterByImage
    ).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          // Cập nhật reviews từ item đầu tiên nếu có
          if (response.data.items && response.data.items.length > 0) {
            this.reviews = response.data.items[0].reviews || [];
          } else {
            this.reviews = [];
          }
          
          // Cập nhật thông tin phân trang
          this.currentPage = response.data.pageNumber;
          this.totalPages = response.data.totalPages;
        } else {
          this.reviews = [];
          this.currentPage = 1;
          this.totalPages = 0;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Lỗi khi tải đánh giá:', error);
        this.reviews = [];
        this.currentPage = 1;
        this.totalPages = 0;
        this.loading = false;
      }
    });
  }

  changeReviewPage(page: number): void {
    if (this.product) {
      this.loadReviews(this.product.variantId, page);
    }
  }

  filterByStar(star: number): void {
    this.selectedStar = this.selectedStar === star ? null : star;
    if (this.product) {
      this.loadReviews(this.product.variantId, 1);
    }
  }

  toggleImageFilter(): void {
    this.isFilterByImage = !this.isFilterByImage;
    if (this.product) {
      this.loadReviews(this.product.variantId, 1);
    }
  }

  calculateDiscountPercentage(price: number, discountPrice: number): number {
    if (!price || !discountPrice || price <= discountPrice) return 0;
    const discount = ((price - discountPrice) / price) * 100;
    return Math.round(discount);
  }

  changeImage(index: number): void {
    this.currentImageIndex = index;
  }

  isVideo(url: string): boolean {
    return url?.toLowerCase().endsWith('.mp4') || false;
  }

  getImageUrl(url: string | null | undefined): string {
    return url || this.defaultImage;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date
      .toLocaleDateString('vi-VN', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
      })
      .replace(',', '');
  }

  formatDescription(description: string): string {
    if (!description) return '';

    // Xử lý các đoạn văn bản
    let formattedText = description
      // Chuyển đổi xuống dòng thành thẻ p
      .split('\n\n')
      .map((paragraph) => `<p>${paragraph}</p>`)
      .join('');

    // Xử lý các đề mục
    formattedText = formattedText.replace(/#{4} (.*?)(?:\n|$)/g, '<h4>$1</h4>');
    formattedText = formattedText.replace(/#{3} (.*?)(?:\n|$)/g, '<h3>$1</h3>');

    // Xử lý danh sách
    formattedText = formattedText.replace(/^- (.*?)$/gm, '<li>$1</li>');
    formattedText = formattedText.replace(/(<li>.*?<\/li>)\s+(?=<li>)/gs, '$1');
    formattedText = formattedText.replace(/(<li>.*?<\/li>)+/g, '<ul>$&</ul>');

    // Xử lý text in đậm và in nghiêng
    formattedText = formattedText.replace(
      /\*\*(.*?)\*\*/g,
      '<strong>$1</strong>'
    );
    formattedText = formattedText.replace(/\*(.*?)\*/g, '<em>$1</em>');

    // Xử lý blockquote
    formattedText = formattedText.replace(
      /^> (.*?)$/gm,
      '<blockquote><p>$1</p></blockquote>'
    );

    // Xử lý các liên kết
    formattedText = formattedText.replace(
      /\[([^\]]+)\]\(([^)]+)\)/g,
      '<a href="$2" target="_blank">$1</a>'
    );

    return formattedText;
  }

  loadReviewSummary() {
    if (this.product?.variantId) {
      this.reviewService.getReviewSummary(this.product.variantId).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.reviewSummary = response.data;
          }
        },
        error: (error) => {
          console.error('Error loading review summary:', error);
        },
      });
    }
  }

  getPercentage(count: number): number {
    if (!this.reviewSummary?.totalReviews) return 0;
    return (count / this.reviewSummary.totalReviews) * 100;
  }

  setupTabHandlers() {
    // Đợi DOM load xong
    setTimeout(() => {
      const reviewTab = document.querySelector('a[href="#review"]');
      if (reviewTab) {
        reviewTab.addEventListener('click', () => {
          // Load review summary khi click vào tab đánh giá
          if (this.product?.variantId && !this.reviewSummary) {
            this.loadReviewSummary();
          }

          // Hiển thị tab đánh giá
          const reviewPane = document.querySelector('#review');
          if (reviewPane) {
            // Ẩn tất cả các tab pane khác
            document.querySelectorAll('.tab-pane').forEach((pane) => {
              pane.classList.remove('active', 'show');
            });
            // Hiển thị tab đánh giá
            reviewPane.classList.add('active', 'show');
          }
        });
      }
    }, 0);
  }

  decreaseQuantity(): void {
    if (this.quantity > 1) {
      this.quantity--;
    }
  }

  increaseQuantity(): void {
    if (this.product?.stock && this.quantity < this.product.stock) {
      this.quantity++;
    }
  }

  private showNotificationMessage(
    message: string,
    type: 'success' | 'error' = 'success'
  ) {
    this.showNotification = true;
    this.notificationMessage = message;
    this.notificationType = type;
    setTimeout(() => {
      this.showNotification = false;
    }, 1500);
  }

  addToCart(): void {
    if (!this.product) return;

    this.loading = true;
    this.cartService
      .addToCart({
        variantId: this.product.variantId,
        quantity: this.quantity,
      })
      .subscribe({
        next: (response) => {
          if (response.isSuccess === true) {
            this.loading = false;
            this.notificationService.show('Thêm vào giỏ hàng thành công!');
            //this.isAddedToCart = true;
          } else {
            this.loading = false;
            this.notificationService.show(
              'Có lỗi xảy ra khi thêm vào giỏ hàng',
              'error'
            );
          }
        },
        error: (error) => {
          console.error('Error adding to cart:', error);
          this.loading = false;
          this.notificationService.show(
            'Có lỗi xảy ra khi thêm vào giỏ hàng',
            'error'
          );
        },
      });
  }

  buyNow() {
    if (!this.product || !this.product.stock) return;
    
    // Chuyển hướng đến trang thanh toán với thông tin sản phẩm
    this.router.navigate(['/checkout'], {
      queryParams: {
        variantId: this.product.variantId,
        quantity: this.quantity,
        name: this.product.name,
        unitPrice: this.product.price,
        discountPrice: this.product.discountPrice || this.product.price,
        image: this.product.images[0]
      }
    });
  }

  selectVariant(variantId: number, attributeName: string, value: string): void {
    // Cập nhật selectedAttributes
    this.selectedAttributes[attributeName] = value;

    // Nếu chọn là "Dung lượng" → load danh sách màu tương ứng
    if (attributeName === 'Dung lượng') {
      this.productService.getColorOptions(variantId).subscribe({
        next: (colors) => {
          this.colorOptions = colors.data;
          this.loading = false;
        },
        error: (err) => {
          console.error('Lỗi khi load màu sắc:', err);
          this.loading = false;
        },
      });
    }

    this.router.navigate(['/product-detail', variantId], {
      replaceUrl: true
    });
    
    // Gọi API để lấy thông tin variant mới
    this.loading = true;
    this.productService.getProductDetail(variantId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.product = response.data;
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Error loading variant:', error);
        this.loading = false;
        this.error = 'Có lỗi xảy ra khi tải thông tin sản phẩm';
      },
    });
  }

  getAttributeValues(
    attributeName: string,
    attributes: any[] | undefined
  ): any[] {
    const filtered =
      attributes?.filter((attr) => attr.attributeName === attributeName) || [];
    return filtered;
  }

  isAttributeSelected(attributeName: string, value: string): boolean {
    return this.selectedAttributes[attributeName] === value;
  }

  reportReview(reviewId: number): void {
    this.currentReviewId = reviewId;
    this.showReportModal = true;
    this.selectedReportReason = '';
    this.customReportReason = '';
  }

  closeReportModal(): void {
    this.showReportModal = false;
    this.selectedReportReason = '';
    this.customReportReason = '';
    this.currentReviewId = null;
  }

  onReasonChange(reason: string): void {
    if (reason !== 'OTHER') {
      this.customReportReason = '';
    }
  }

  isValidReport(): boolean {
    if (!this.selectedReportReason) return false;
    if (this.selectedReportReason === 'OTHER' && !this.customReportReason.trim()) return false;
    return true;
  }

  submitReport(): void {
    if (!this.currentReviewId || !this.isValidReport()) return;

    const reason = this.selectedReportReason === 'OTHER' 
      ? this.customReportReason.trim()
      : this.reportReasons.find(r => r.value === this.selectedReportReason)?.label || '';

    const reportRequest: ReportReviewRequest = {
      reviewId: this.currentReviewId,
      reason: reason
    };

    this.reviewService.reportReview(reportRequest).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.notificationService.show('Đã gửi báo cáo bình luận thành công', 'success');
          this.closeReportModal();
        } else {
          this.notificationService.show(
            response.error?.details?.[0] || 'Có lỗi xảy ra khi gửi báo cáo', 
            'error'
          );
        }
      },
      error: (error) => {
        let errorMessage = 'Có lỗi xảy ra khi gửi báo cáo';
        
        if (error instanceof Error) {
          errorMessage = error.message;
        }
        
        console.error('Lỗi khi báo cáo bình luận:', error);
        this.notificationService.show(errorMessage, 'error');
        this.closeReportModal();
      }
    });
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(this.totalPages, startPage + maxVisiblePages - 1);

    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadReviews(this.product?.variantId || 0, page);
    }
  }

  checkWishlistStatus(): void {
    if (!this.product || !this.authService.isLoggedIn) {
      this.isInWishlist = false;
      return;
    }

    this.wishlistService.getWishlistItems().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          // Kiểm tra xem sản phẩm hiện tại có trong wishlist không
          this.isInWishlist = response.data.some(item => item.variantId === this.product?.variantId);
        }
      },
      error: () => {
        this.isInWishlist = false;
      }
    });
  }

  navigateToDetail(variantId: number) {
    this.router.navigate(['/product-detail', variantId]);
  }

  addToWishlist(event: Event, product: ProductVariant) {
    event.stopPropagation();
    
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    const isInWishlist = product.isInWishlist;

    if (isInWishlist) {
      this.wishlistService.changeWishlistStatus(product.variantId, false).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            product.isInWishlist = false;
            this.notificationService.show('Đã xóa khỏi danh sách yêu thích!');
          }
        },
        error: () => {
          this.notificationService.show('Có lỗi xảy ra khi xóa khỏi danh sách yêu thích', 'error');
        }
      });
    } else {
      this.wishlistService.addToWishlist(product.variantId).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            product.isInWishlist = true;
            this.notificationService.show('Thêm vào danh sách yêu thích thành công!');
          }
        },
        error: () => {
          this.notificationService.show('Có lỗi xảy ra khi thêm vào danh sách yêu thích', 'error');
        }
      });
    }
  }

  handleImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'assets/images/no-image.png';
  }
}
