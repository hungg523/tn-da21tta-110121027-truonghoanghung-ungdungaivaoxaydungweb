import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface SpinCheckResponse {
  isSuccess: boolean;
  statusCode: number;
  data: {
    canSpin: boolean;
    spinCount?: number;
  };
}

export interface MiniGameResponse {
  isSuccess: boolean;
  statusCode: number;
  data: Coupon[];
}

export interface Coupon {
  id: number | null;
  code: string;
  description: string;
  discountPercentage: number;
  discountAmount: number;
  minOrderValue: number;
  maxDiscountAmount: number;
  timesUsed: number;
  maxUsage: number;
  maxUsagePerUser: number | null;
  userSpecific: boolean | null;
  isVip: boolean | null;
  couponType: string;
  availableDate: string | null;
  startDate: string;
  endDate: string;
  createdAt: string | null;
  discountDisplay: string | null;
  term: string | null;
  isAvtived: boolean | null;
}

@Injectable({
  providedIn: 'root'
})
export class MinigameService {
  private apiUrl = environment.apiUrlV1;

  constructor(private http: HttpClient) { }

  getRandomCoupons(): Observable<MiniGameResponse> {
    return this.http.get<MiniGameResponse>(`${this.apiUrl}/coupon/get-random`);
  }

  checkSpinToday(): Observable<SpinCheckResponse> {
    return this.http.get<SpinCheckResponse>(`${this.apiUrl}/spin/check-today`);
  }

  saveSpinHistory(couponId: number | null): Observable<MiniGameResponse> {
    return this.http.post<MiniGameResponse>(`${this.apiUrl}/spin/save-history`, { couponId });
  }
}
