import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CategoryService, Category, CategoryDetail } from '../../services/category.service';
import { ProductService, ProductVariant, ProductResponse, AttributeValue } from '../../services/product.service';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { WishlistService } from '../../services/wishlist.service';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { PageBannerComponent } from '../../components/page-banner/page-banner.component';
import { CheckoutService } from '../../services/checkout.service';

@Component({
  selector: 'app-shop',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageBannerComponent],
  templateUrl: './shop.component.html',
  styleUrls: ['./shop.component.css'],
  animations: [
    trigger('slideInOut', [
      state('in', style({
        height: '*',
        opacity: 1,
        visibility: 'visible'
      })),
      state('out', style({
        height: '0',
        opacity: 0,
        visibility: 'hidden'
      })),
      transition('in => out', animate('200ms ease-in-out')),
      transition('out => in', animate('200ms ease-in-out'))
    ])
  ]
})
export class ShopComponent implements OnInit {
  products: ProductVariant[] = [];
  displayedProducts: ProductVariant[] = [];
  categories: Category[] = [];
  selectedCategory: CategoryDetail | null = null;
  selectedCategoryId: number | null = null;
  sortBy: string = 'default';
  viewMode: 'grid' | 'list' = 'grid';
  isLoading: boolean = false;
  error: string | null = null;
  showCategoryMenu: boolean = false;
  attributeValues: AttributeValue[] = [];
  selectedColors: string[] = [];
  selectedStorages: string[] = [];
  minPrice: number | null = null;
  maxPrice: number | null = null;
  isDescending: boolean = false;
  isAscending: boolean = false;
  productCountByCategory: { [key: number]: number } = {};
  
  // Thêm các thuộc tính mới cho phân trang
  pageSize: number = 20;
  currentPage: number = 1;
  hasMoreProducts: boolean = true;
  totalProducts: number = 0;
  skip: number = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private categoryService: CategoryService,
    private productService: ProductService,
    private wishlistService: WishlistService,
    private authService: AuthService,
    private notificationService: NotificationService,
    private checkoutService: CheckoutService
  ) { }

  calculateDiscountPercentage(originalPrice: number, discountPrice: number): number {
    if (!originalPrice || !discountPrice || originalPrice <= discountPrice) return 0;
    const discount = ((originalPrice - discountPrice) / originalPrice) * 100;
    return Math.round(discount);
  }

  ngOnInit(): void {
    // Kiểm tra callback từ PayOS
    this.route.queryParams.subscribe(params => {
      const status = params['status'];
      const orderCode = params['orderCode'];
      const categoryId = params['categoryId'];
      
      if (status && orderCode) {
        if (status === 'PAID' || status === 'CANCELLED') {
          this.confirmPayOSPayment(orderCode);
        } else {
          this.notificationService.show('Thanh toán không thành công', 'error');
        }
      }

      // Xử lý categoryId từ URL
      if (categoryId) {
        this.filterByCategory(Number(categoryId));
      } else {
        this.loadProducts();
      }
    });

    this.loadCategories();
    this.loadAttributeValues();
  }

  loadCategories() {
    this.isLoading = true;
    this.error = null;

    this.categoryService.getCategories().subscribe({
      next: (data) => {
        this.categories = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching categories:', error);
        this.error = 'Không thể tải danh sách danh mục. Vui lòng thử lại sau.';
        this.isLoading = false;
      }
    });
  }

  loadProducts(categoryId?: number | null, append: boolean = false) {
    this.isLoading = true;
    this.error = null;

    const filters: any = {
      skip: this.skip,
      take: this.pageSize
    };
    if (categoryId !== null && categoryId !== undefined) {
      filters.categoryId = categoryId;
    }
    this.productService.getProducts(filters).subscribe({
      next: (response: ProductResponse) => {
        if (response.isSuccess) {
          if (append) {
            this.products = [...this.products, ...response.data.productVariants];
          } else {
            this.products = response.data.productVariants;
          }
          
          this.totalProducts = response.data.totalItems;
          this.hasMoreProducts = this.products.length < this.totalProducts;
          this.updateDisplayedProducts();
          if (this.authService.isLoggedIn()) {
            this.loadWishlistStatus();
          } else {
            this.products.forEach(product => {
              product.isInWishlist = false;
            });
          }
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching products:', error);
        this.error = 'Không thể tải sản phẩm. Vui lòng thử lại sau.';
        this.isLoading = false;
      }
    });
  }

  loadWishlistStatus(): void {
    this.wishlistService.getWishlistItems().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          const wishlistMap = new Map<number, boolean>();
          response.data.forEach(item => {
            wishlistMap.set(item.variantId, true);
          });

          this.products.forEach(product => {
            product.isInWishlist = wishlistMap.has(product.variantId);
          });
        }
      },
      error: () => {
        this.products.forEach(product => {
          product.isInWishlist = false;
        });
      }
    });
  }

  filterByCategory(categoryId: number | null) {
    this.selectedCategoryId = categoryId;
    
    if (categoryId === null) {
      this.loadProducts();
      this.selectedCategory = null;
    } else {
      this.loadProducts(categoryId);
      this.categoryService.getCategoryDetail(categoryId).subscribe({
        next: (data) => {
          this.selectedCategory = data;
        },
        error: (error) => {
          console.error('Error fetching category details:', error);
          this.error = 'Không thể tải chi tiết danh mục. Vui lòng thử lại sau.';
        }
      });
    }
  }

  closeCategoryMenu() {
    this.showCategoryMenu = false;
  }

  openCategoryMenu() {
    this.showCategoryMenu = true;
  }

  sortProducts(sortType: string) {
    this.sortBy = sortType;
    switch(sortType) {
      case 'price-low':
        this.products.sort((a, b) => a.price - b.price);
        break;
      case 'price-high':
        this.products.sort((a, b) => b.price - a.price);
        break;
      case 'name-asc':
        this.products.sort((a, b) => a.name.localeCompare(b.name));
        break;
      case 'name-desc':
        this.products.sort((a, b) => b.name.localeCompare(a.name));
        break;
    }
    this.updateDisplayedProducts();
  }

  toggleView(view: string) {
    this.viewMode = view as 'grid' | 'list';
  }

  addToCart(productId: number) {
    console.log('Add to cart:', productId);
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

  quickView(productId: number) {
    console.log('Quick view:', productId);
  }

  toggleCategory(category: Category) {
    this.categoryService.getCategoryDetail(category.id).subscribe({
      next: (categoryDetail) => {
        category.categories = categoryDetail.categories;
        category.isOpen = !category.isOpen;
      },
      error: (error) => {
        console.error('Error loading category details:', error);
        this.error = 'Không thể tải thông tin danh mục con';
      }
    });
  }

  getProductImage(product: ProductVariant): string {
    const mainImage = product.images?.find((img: any) => img.position === 0);
    return mainImage?.url || 'assets/images/no-image.png';
  }

  handleImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'assets/images/no-image.png';
  }

  navigateToDetail(variantId: number) {
    this.router.navigate(['/product-detail', variantId]);
  }

  private confirmPayOSPayment(orderCode: string): void {
    
    this.checkoutService.confirmPayOSPayment(orderCode).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.notificationService.show('Đã xử lý đơn hàng thành công!', 'success');
          this.router.navigate(['/shop']);
        } else {
          this.notificationService.show('Có lỗi xảy ra khi xác nhận thanh toán', 'error');
        }
      },
      error: (error) => {
        this.notificationService.show(error.message || 'Có lỗi xảy ra khi xác nhận thanh toán', 'error');
      }
    });
  }

  loadAttributeValues() {
    this.productService.getAllAttributeValues().subscribe({
      next: (res) => {
        this.attributeValues = res.data;
      },
      error: (err) => {
        this.attributeValues = [];
      }
    });
  }

  onFilterChange() {
    this.skip = 0;
    const filters: any = {
      categoryId: this.selectedCategoryId,
      minPrice: this.minPrice,
      maxPrice: this.maxPrice,
      isDescending: this.isDescending,
      isAscending: this.isAscending,
      skip: this.skip,
      take: this.pageSize
    };
    if (this.selectedColors.length > 0) {
      filters.color = this.selectedColors.join(',');
    }
    if (this.selectedStorages.length > 0) {
      filters.storage = this.selectedStorages.join(',');
    }
    this.isLoading = true;
    this.error = null;
    this.productService.getProducts(filters).subscribe({
      next: (response: ProductResponse) => {
        if (response.isSuccess) {
          this.products = response.data.productVariants;
          this.totalProducts = response.data.totalItems;
          this.hasMoreProducts = this.products.length < this.totalProducts;
          this.updateDisplayedProducts();
          this.countProductsByCategory();
          if (this.authService.isLoggedIn()) {
            this.loadWishlistStatus();
          } else {
            this.products.forEach(product => {
              product.isInWishlist = false;
            });
          }
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching products:', error);
        this.error = 'Không thể tải sản phẩm. Vui lòng thử lại sau.';
        this.isLoading = false;
      }
    });
  }

  countProductsByCategory() {
    this.productCountByCategory = {};
    this.products.forEach(p => {
      if (p.Productid) {
        this.productCountByCategory[p.Productid] = (this.productCountByCategory[p.Productid] || 0) + 1;
      }
    });
  }

  onColorChange(event: Event, color: string) {
    const checkbox = event.target as HTMLInputElement;
    if (checkbox.checked) {
      this.selectedColors.push(color);
    } else {
      this.selectedColors = this.selectedColors.filter(c => c !== color);
    }
    this.onFilterChange();
  }

  onStorageChange(event: Event, storage: string) {
    const checkbox = event.target as HTMLInputElement;
    if (checkbox.checked) {
      this.selectedStorages.push(storage);
    } else {
      this.selectedStorages = this.selectedStorages.filter(s => s !== storage);
    }
    this.onFilterChange();
  }

  get storageAttributes(): AttributeValue[] {
    return this.attributeValues.filter(a => a.attributeId === 2);
  }
  get colorAttributes(): AttributeValue[] {
    return this.attributeValues.filter(a => a.attributeId === 1);
  }

  onSortChange() {
    switch(this.sortBy) {
      case 'price-asc':
        this.isAscending = true;
        this.isDescending = false;
        break;
      case 'price-desc':
        this.isAscending = false;
        this.isDescending = true;
        break;
      default:
        this.isAscending = false;
        this.isDescending = false;
    }
    this.onFilterChange();
  }

  getColorCode(colorName: string): string {
    const colorMap: { [key: string]: string } = {
      'Đen': '#000000',
      'Trắng': '#ffffff',
      'Đỏ': '#ff0000',
      'Vàng': '#ffd700',
      'Xanh dương': '#0074D9',
      'Xanh lá': '#2ECC40',
      'Xanh lá đậm': '#008000',
      'Xanh ngọc (Teal)': '#20B2AA',
      'Xanh Pacific': '#1CA9C9',
      'Xanh Sierra': '#5B7F95',
      'Xanh Ultramarine': '#3F00FF',
      'Xanh Alpine': '#7EC850',
      'Xám': '#808080',
      'Bạc': '#C0C0C0',
      'Hồng': '#ffc0cb',
      'Tím': '#800080',
      'Tím đậm (Deep Purple)': '#673ab7',
      'Titan đen': '#222',
      'Titan trắng': '#eee',
      'Titan xanh dương': '#1e90ff',
      'Titan xám đá phiến': '#6e7b8b',
      'Titan sa mạc (Desert Titanium)': '#e1c699',
      'Titan tự nhiên (Natural Titanium)': '#d6cfc7',
      'Rose Gold': '#b76e79',
      'Graphite': '#383838',
      'Midnight (đen)': '#191970',
      'Starlight (trắng ấm)': '#f8f4e3',
      'Jet Black': '#343434',
      'Space Black': '#161616',
      'Xám không gian (Space Gray)': '#4B4B4B',
      'Đen không gian (Space Black)': '#161616',
      'Cam': '#ffa500',
      'Xanh ngọc': '#20B2AA',
    };
    return colorMap[colorName.trim()] || '#ddd';
  }

  updateDisplayedProducts() {
    this.displayedProducts = this.products;
  }

  loadMoreProducts() {
    if (!this.isLoading && this.hasMoreProducts) {
      this.skip += this.pageSize;
      this.loadProducts(this.selectedCategoryId, true);
    }
  }

  getRemainingProductsCount(): number {
    return Math.max(0, this.totalProducts - this.products.length);
  }
}
