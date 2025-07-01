import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { CartService } from './cart.service';

@Injectable({
  providedIn: 'root'
})
export class CartStateService {
  private cartItemCount = new BehaviorSubject<number>(0);

  constructor(private cartService: CartService) {
    // Subscribe vào cartItemCount$ của CartService để cập nhật real-time
    this.cartService.cartItemCount$.subscribe(count => {
      this.cartItemCount.next(count);
    });
  }

  getCartItemCount() {
    return this.cartItemCount.asObservable();
  }

  updateCartCount(count: number) {
    this.cartItemCount.next(count);
  }
} 