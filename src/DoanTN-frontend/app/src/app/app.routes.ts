import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { ShopComponent } from './pages/shop/shop.component';
import { ProductDetailComponent } from './pages/product-detail/product-detail.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password.component';
import { CartComponent } from './pages/cart/cart.component';
import { ChangePasswordComponent } from './pages/change-password/change-password.component';
import { CheckoutComponent } from './pages/checkout/checkout.component';
import { OAuthCallbackComponent } from './pages/oauth-callback/oauth-callback.component';
import { CouponComponent } from './pages/coupon/coupon.component';
import { WishlistComponent } from './pages/wishlist/wishlist.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { CouponListComponent } from './pages/coupon-list/coupon-list.component';
import { OrderHistoryComponent } from './pages/order-history/order-history.component';
import { SearchResultComponent } from './pages/search-result/search-result.component';
import { ThankYouComponent } from './pages/thank-you/thank-you.component';
import { SorryComponent } from './pages/sorry/sorry.component';
import { AboutComponent } from './pages/about/about.component';
import { DeliveryComponent } from './pages/delivery/delivery.component';
import { TermsComponent } from './pages/terms/terms.component';
export const routes: Routes = [
  {
    path: 'about',
    component: AboutComponent
  },
  {
    path: 'delivery',
    component: DeliveryComponent
  },
  {
    path: 'terms',
    component: TermsComponent
  },
  {
    path: '',
    component: HomeComponent
  },
  {
    path: 'shop',
    component: ShopComponent
  },
  {
    path: 'product-detail/:id',
    component: ProductDetailComponent
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'register',
    component: RegisterComponent
  },
  {
    path: 'forgot-password',
    component: ForgotPasswordComponent
  },
  {
    path: 'change-password',
    component: ChangePasswordComponent
  },
  {
    path: 'cart',
    component: CartComponent
  },
  {
    path: 'wishlist',
    component: WishlistComponent
  },
  {
    path: 'checkout',
    component: CheckoutComponent
  },
  {
    path: 'auth/google',
    component: OAuthCallbackComponent,
    data: { provider: 'google' }
  },
  {
    path: 'auth/facebook',
    component: OAuthCallbackComponent,
    data: { provider: 'facebook' }
  },
  {
    path: 'voucher',
    component: CouponComponent
  },
  {
    path: 'profile',
    component: ProfileComponent
  },
  {
    path: 'user-voucher',
    component: CouponListComponent
  },
  {
    path: 'order-history',
    component: OrderHistoryComponent,
  },
  {
    path: 'search',
    component: SearchResultComponent
  },
  {
    path: 'thank-you',
    component: ThankYouComponent
  },
  {
    path: 'sorry',
    component: SorryComponent
  }
];