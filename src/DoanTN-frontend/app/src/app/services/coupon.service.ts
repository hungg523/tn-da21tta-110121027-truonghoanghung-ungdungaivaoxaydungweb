import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Coupon {
  id: number;
  code: string;
  description: string;
  discountPercentage: number;
  discountAmount: number;
  minOrderValue: number;
  maxDiscountAmount: number;
  timesUsed: number;
  maxUsage: number;
  maxUsagePerUser: number;
  couponType: 'ORDER' | 'SHIP';
  availableDate: string;
  discountDisplay: string;
  term: string;
  isVip: boolean;
}

export interface CouponResponse {
  isSuccess: boolean;
  statusCode: number;
  data: {
    couponOrder: Coupon[];
    couponShip: Coupon[];
  };
}

@Injectable({
  providedIn: 'root'
})
export class CouponService {
  private apiUrl = environment.apiUrlV1;

  constructor(private http: HttpClient) { }

  getAllCoupons(): Observable<CouponResponse> {
    return this.http.get<CouponResponse>(`${this.apiUrl}/coupon/get-all?isActived=true`);
  }

  saveCoupon(couponId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/user-coupon/create`, { couponId });
  }

  getUserCoupons(): Observable<CouponResponse> {
    return this.http.get<CouponResponse>(`${this.apiUrl}/user-coupon/get-by-user-id`);
  }

  getCouponDetail(code: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/coupon/get-detail/${code}`);
  }
} 