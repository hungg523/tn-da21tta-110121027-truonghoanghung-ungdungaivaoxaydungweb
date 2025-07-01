import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { ProductService, ProductVariant } from '../../services/product.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-search-result',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './search-result.component.html',
  styleUrls: ['./search-result.component.css']
})
export class SearchResultComponent implements OnInit {
  searchQuery: string = '';
  products: ProductVariant[] = [];
  displayedProducts: ProductVariant[] = [];
  isLoading: boolean = false;
  error: string | null = null;
  
  // Thêm các thuộc tính cho phân trang
  pageSize: number = 20;
  skip: number = 0;
  hasMoreProducts: boolean = true;
  totalProducts: number = 0;

  constructor(
    private route: ActivatedRoute, 
    private productService: ProductService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.searchQuery = params['query'] || '';
      if (this.searchQuery) {
        this.skip = 0;
        this.products = [];
        this.searchProducts();
      }
    });
  }

  searchProducts(append: boolean = false) {
    this.isLoading = true;
    this.error = null;
    this.productService.searchProducts({ name: this.searchQuery, skip: this.skip, take: this.pageSize }).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res && res.isSuccess && res.data) {
          if (append) {
            this.products = [...this.products, ...res.data.productVariants];
          } else {
            this.products = res.data.productVariants;
          }
          this.totalProducts = res.data.totalItems;
          this.hasMoreProducts = this.products.length < this.totalProducts;
          this.updateDisplayedProducts();
        } else {
          this.products = [];
          this.displayedProducts = [];
          this.hasMoreProducts = false;
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.error = 'Không thể tải kết quả tìm kiếm.';
        this.products = [];
        this.displayedProducts = [];
        this.hasMoreProducts = false;
      }
    });
  }

  updateDisplayedProducts() {
    this.displayedProducts = this.products;
  }

  loadMoreProducts() {
    if (!this.isLoading && this.hasMoreProducts) {
      this.skip += this.pageSize;
      this.searchProducts(true);
    }
  }

  getRemainingProductsCount(): number {
    return Math.max(0, this.totalProducts - this.products.length);
  }

  // Thêm các phương thức mới
  navigateToDetail(variantId: string) {
    this.router.navigate(['/product-detail', variantId]);
  }

  getProductImage(product: ProductVariant): string {
    if (!product.images || product.images.length === 0) {
      return 'assets/images/no-image.png';
    }
    const firstImage = product.images[0];
    return typeof firstImage === 'string' ? firstImage : firstImage.url;
  }

  handleImageError(event: any) {
    event.target.src = 'assets/images/no-image.png';
  }

  addToWishlist(event: Event, product: ProductVariant) {
    event.stopPropagation();
    // TODO: Implement wishlist functionality
    product.isInWishlist = !product.isInWishlist;
  }

  calculateDiscountPercentage(originalPrice: number, discountPrice: number): number {
    if (!originalPrice || !discountPrice) return 0;
    return Math.round(((originalPrice - discountPrice) / originalPrice) * 100);
  }
} 