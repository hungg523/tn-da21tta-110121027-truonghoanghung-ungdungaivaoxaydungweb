// cart.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface CartItem {
  variantId: number;
  quantity: number;
  name: string;
  description: string;
  image: string;
  unitPrice: number;
  discountPrice: number;
  totalPrice: number;
  productAttributes: string[];
}

export interface CartDetail {
  id: number;
  cartItems: CartItem[];
  totalPrice: number;
}

export interface CartResponse {
  isSuccess: boolean;
  statusCode: number;
  data: CartDetail;
}

export interface CartItemRequest {
  variantId: number;
  quantity: number;
}

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiUrlV1 = `${environment.apiUrlV1}/cart`;
  
  // BehaviorSubject để theo dõi thay đổi của giỏ hàng
  private cartDetailSubject = new BehaviorSubject<CartDetail | null>(null);
  public cartDetail$ = this.cartDetailSubject.asObservable();

  // BehaviorSubject để theo dõi số lượng sản phẩm trong giỏ hàng
  private cartItemCountSubject = new BehaviorSubject<number>(0);
  public cartItemCount$ = this.cartItemCountSubject.asObservable();

  constructor(private http: HttpClient) {
    // Khởi tạo giỏ hàng khi service được tạo
    this.getCartDetail().subscribe();
  }

  getCartDetail(): Observable<CartResponse> {
    return this.http.get<CartResponse>(`${this.apiUrlV1}/get-detail`, { withCredentials: true })
      .pipe(
        tap(response => {
          if (response.isSuccess && response.data) {
            this.cartDetailSubject.next(response.data);
            this.cartItemCountSubject.next(response.data.cartItems.length);
          }
        })
      );
  }

  addToCart(request: CartItemRequest): Observable<CartResponse> {
    return this.http.post<CartResponse>(`${this.apiUrlV1}/create`, request, { withCredentials: true })
      .pipe(
        tap(response => {
          if (response.isSuccess) {
            this.getCartDetail().subscribe();
          }
        })
      );
  }

  updateCartQuantity(request: CartItemRequest): Observable<CartResponse> {
    return this.http.put<CartResponse>(`${this.apiUrlV1}/update-quantity`, request, { withCredentials: true })
      .pipe(
        tap(response => {
          if (response.isSuccess) {
            this.getCartDetail().subscribe();
          }
        })
      );
  }

  removeItem(variantId: number): Observable<CartResponse> {
    return this.http.delete<CartResponse>(`${this.apiUrlV1}/delete-item/${variantId}`, { withCredentials: true })
      .pipe(
        tap(response => {
          if (response.isSuccess) {
            this.getCartDetail().subscribe();
          }
        })
      );
  }

  clearCart(): Observable<CartResponse> {
    return this.http.delete<CartResponse>(`${this.apiUrlV1}/clear`, { withCredentials: true })
      .pipe(
        tap(response => {
          if (response.isSuccess) {
            this.getCartDetail().subscribe();
          }
        })
      );
  }

  // Lấy giá trị hiện tại của giỏ hàng
  getCurrentCartDetail(): CartDetail | null {
    return this.cartDetailSubject.getValue();
  }

  // Lấy số lượng sản phẩm hiện tại
  getCurrentCartItemCount(): number {
    return this.cartItemCountSubject.getValue();
  }
}
