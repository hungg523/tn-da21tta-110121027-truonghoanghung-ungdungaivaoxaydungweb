import { CommonModule } from "@angular/common";
import { Component, OnInit, HostListener, ElementRef } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule, Router } from "@angular/router";
import { AuthService } from "../../services/auth.service";
import { CategoryService, Category, CategoryDetail } from '../../services/category.service';
import { CartStateService } from '../../services/cart-state.service';
import { WishlistService } from '../../services/wishlist.service';
import { UserService, UserProfile } from '../../services/user.service';
import { ProductService } from '../../services/product.service';
import { ProductResponse, ProductVariant } from '../../services/product.service';
import { Subject, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { ModalService } from '../../services/modal.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule
  ],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {
  isLoggedIn: boolean = false;
  showUserMenu: boolean = false;
  showMobileMenu: boolean = false;
  showSearch: boolean = false;
  showCategoryMenu: boolean = false;
  cartItemCount: number = 0;
  wishlistItemCount: number = 0;
  searchQuery: string = '';
  searchResults: ProductVariant[] = [];
  searchLoading: boolean = false;
  private searchSubject = new Subject<string>();
  categories: Category[] = [];
  selectedCategoryId: number | null = null;
  userName: string = '';
  userAvatar: string = '';
  isMobile: boolean = false;
  isMenuOpen: boolean = false;
  userProfile: UserProfile | null = null;
  hoveredCategoryIndex: number | null = null;

  constructor(
    private authService: AuthService,
    private categoryService: CategoryService,
    private cartStateService: CartStateService,
    private elementRef: ElementRef,
    private wishlistService: WishlistService,
    private userService: UserService,
    private productService: ProductService,
    private router: Router,
    private modalService: ModalService
  ) {
    this.checkScreenSize();
    this.hoveredCategoryIndex = null;
  }

  ngOnInit() {
    // Đăng ký lắng nghe thay đổi trạng thái đăng nhập
    this.authService.isLoggedIn$.subscribe(isLoggedIn => {
      this.isLoggedIn = isLoggedIn;
      if (isLoggedIn) {
        const user = this.authService.getCurrentUser();
        if (user) {
          this.userName = user.username;
          this.userAvatar = user.avatar || 'assets/images/default-avatar.png';
        }
      }
    });

    // Lấy số lượng sản phẩm trong giỏ hàng
    this.cartStateService.getCartItemCount().subscribe(count => {
      this.cartItemCount = count;
    });
    
    // Load categories
    this.loadCategories();

    if (this.authService.isLoggedIn()) {
      this.loadWishlistCount();
    }

    this.loadUserProfile();

    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap((query) => {
        if (!query.trim()) {
          this.searchResults = [];
          return of({ isSuccess: true, data: [] });
        }
        this.searchLoading = true;
        return this.productService.searchProductByName(query);
      })
    ).subscribe((res: any) => {
      this.searchLoading = false;
      if (res && res.isSuccess && res.data) {
        this.searchResults = res.data;
      } else {
        this.searchResults = [];
      }
    }, () => {
      this.searchLoading = false;
      this.searchResults = [];
    });
  }

  @HostListener('window:resize')
  checkScreenSize() {
    this.isMobile = window.innerWidth <= 768;
    if (!this.isMobile) {
      this.showSearch = false;
      this.showMobileMenu = false;
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    // Đóng user menu khi click outside
    if (!this.elementRef.nativeElement.querySelector('.user-menu')?.contains(event.target as Node)) {
      this.showUserMenu = false;
    }

    // Đóng mobile menu khi click outside
    const mobileMenuBtn = this.elementRef.nativeElement.querySelector('.mobile-menu-btn');
    if (mobileMenuBtn && !mobileMenuBtn.contains(event.target as Node) &&
      !this.elementRef.nativeElement.querySelector('.nav-menu')?.contains(event.target as Node)) {
      this.showMobileMenu = false;
    }

    // Đóng search khi click outside
    const searchContainer = this.elementRef.nativeElement.querySelector('.search-container');
    if (searchContainer && !searchContainer.contains(event.target as Node)) {
      this.showSearch = false;
      this.searchResults = [];
    }

    // Đóng category menu khi click outside
    const categoryLink = this.elementRef.nativeElement.querySelector('.nav-item.dropdown .nav-link');
    const categoryMenu = this.elementRef.nativeElement.querySelector('.dropdown-menu');
    if (!categoryLink?.contains(event.target as Node) && !categoryMenu?.contains(event.target as Node)) {
      this.showCategoryMenu = false;
    }
  }

  toggleMobileMenu() {
    this.showMobileMenu = !this.showMobileMenu;
    if (this.showMobileMenu) {
      this.showSearch = false;
      this.showUserMenu = false;
      this.showCategoryMenu = false;
    }
  }

  toggleSearch() {
    this.showSearch = !this.showSearch;
    if (this.showSearch) {
      this.showMobileMenu = false;
      this.showUserMenu = false;
      this.showCategoryMenu = false;
    }
  }

  toggleUserMenu() {
    this.showUserMenu = !this.showUserMenu;
    if (this.showUserMenu) {
      this.showMobileMenu = false;
      this.showSearch = false;
      this.showCategoryMenu = false;
    }
  }

  toggleCategoryMenu(event: Event) {
    event.stopPropagation(); // Ngăn chặn sự kiện click lan ra ngoài
    this.showCategoryMenu = !this.showCategoryMenu;
    if (this.showCategoryMenu) {
      this.showMobileMenu = false;
      this.showSearch = false;
      this.showUserMenu = false;
    }
  }

  logout() {
    this.authService.logout();
    this.showUserMenu = false;
    this.userName = '';
    this.userAvatar = '';
  }

  onSearchInputChange() {
    if (this.searchQuery.trim()) {
      this.showSearch = true;
      this.searchSubject.next(this.searchQuery);
    } else {
      this.searchResults = [];
      this.showSearch = false;
    }
  }

  search() {
    if (this.searchQuery.trim()) {
      this.showSearch = false;
      this.searchResults = [];
      this.router.navigate(['/search'], { queryParams: { query: this.searchQuery.trim() } });
    }
  }

  private loadCategories() {
    this.categoryService.getCategories().subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      },
    });
  }

  loadCategoryDetail(categoryId: number) {
    if (this.selectedCategoryId !== categoryId) {
      this.selectedCategoryId = categoryId;
      this.categoryService.getCategoryDetail(categoryId).subscribe({
        next: (categoryDetail: CategoryDetail) => {
          const categoryIndex = this.categories.findIndex(c => c.id === categoryId);
          if (categoryIndex !== -1) {
            this.categories[categoryIndex] = { ...this.categories[categoryIndex], ...categoryDetail };
          }
        },
        error: (error) => {
          console.error('Error loading category detail:', error);
        }
      });
    }
  }

  loadWishlistCount(): void {
    this.wishlistService.getWishlistItems().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.wishlistItemCount = response.data.length;
        }
      }
    });
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
  }

  loadUserProfile(): void {
    this.userService.getProfile().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.userProfile = response.data;
        }
      }
    });
  }

  get validSearchResults() {
    return this.searchResults.filter(p => p && p.name);
  }

  openLoginModal() {
    this.modalService.openLoginModal();
  }

  openRegisterModal() {
    this.modalService.openRegisterModal();
  }
}
