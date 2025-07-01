import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface WishlistResponse {
  isSuccess: boolean;
  statusCode: number;
}

export interface WishlistItem {
  id: number;
  variantId: number;
  name: string;
  description: string;
  image: string;
  unitPrice: number;
  discountPrice: number;
  productAttributes: string[];
  originalPrice?: number;
}

export interface WishlistDetailResponse {
  isSuccess: boolean;
  statusCode: number;
  data: WishlistItem[];
}

export interface WishlistStatusResponse {
  isSuccess: boolean;
  statusCode: number;
  data: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private apiUrl = `${environment.apiUrlV1}/wish-list`;

  constructor(private http: HttpClient) { }

  addToWishlist(variantId: number): Observable<WishlistResponse> {
    return this.http.post<WishlistResponse>(`${this.apiUrl}/create`, { variantId });
  }

  changeWishlistStatus(variantId: number, isActived: boolean): Observable<WishlistResponse> {
    return this.http.put<WishlistResponse>(`${this.apiUrl}/change-status/${variantId}`, { isActived });
  }

  getWishlistItems(): Observable<WishlistDetailResponse> {
    return this.http.get<WishlistDetailResponse>(`${this.apiUrl}/get-detail?isActived=true`);
  }
} 