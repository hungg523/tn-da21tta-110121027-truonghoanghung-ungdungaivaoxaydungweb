import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { catchError } from 'rxjs/operators';

export interface RefundRequest {
  orderItemId: number;
  quantity: number;
  reason: string;
  imageData?: string;
  accountName?: string;
  accountNumber?: string;
  bankName?: string;
  phoneNumber?: string;
  returnType: 'Bank' | 'Momo' | 'Other';
}

export interface RefundResponse {
  isSuccess: boolean;
  statusCode: number;
}

export interface ReturnHistory {
  returnId: number;
  variantId: number;
  status: string;
  name: string;
  productAttribute: string;
  imageUrl: string;
  quantity: number;
  refundAmount: number;
  returnUrl: string;
}

export interface ReturnHistoryResponse {
  isSuccess: boolean;
  statusCode: number;
  data: {
    totalItems: number;
    items: ReturnHistory[];
  };
}

export interface ReturnDetail {
  returnId: number;
  variantId: number;
  status: string;
  name: string;
  imageUrl: string;
  productAttribute: string;
  quantity: number;
  refundAmount: number;
  returnUrl: string;
  userAddresses: {
    addressId: number;
    fullName: string;
    phoneNumber: string;
    address: string;
  };
  createdAt: string;
  processedAt: string | null;
  accountName: string | null;
  accountNumber: string | null;
  bankName: string | null;
  phoneNumber: string | null;
  returnType: 'Bank' | 'Momo' | 'Other';
}

export interface ReturnDetailResponse {
  isSuccess: boolean;
  statusCode: number;
  data: ReturnDetail;
}

@Injectable({
  providedIn: 'root'
})
export class ReturnService {
  private apiUrlV1 = `${environment.apiUrlV1}/return`;

  constructor(private http: HttpClient) {}

  requestRefund(request: RefundRequest): Observable<RefundResponse> {
    return this.http.post<RefundResponse>(`${this.apiUrlV1}/refund`, request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'Đã có lỗi xảy ra khi yêu cầu hoàn tiền';
        if (error.status === 400 || error.status === 409) {
          if (error.error?.message?.includes('Returns are not allowed if the order is more than 7 days old or has not been confirmed as success')) {
            errorMessage = 'Không thể trả hàng nếu đơn hàng đã quá 7 ngày hoặc chưa được xác nhận thành công';
          } else {
            errorMessage = 'Không thể trả hàng nếu đơn hàng đã quá 7 ngày hoặc chưa được xác nhận thành công';
          }
        } else if (error.status === 404) {
          errorMessage = 'Không tìm thấy đơn hàng';
        }
        return throwError(() => new Error(errorMessage));
      })
    );
  }

  getReturnHistory(options?: { skip?: number, take?: number }): Observable<ReturnHistoryResponse> {
    let url = `${this.apiUrlV1}/view-history`;
    let params = [];
    if (options) {
      if (options.skip !== undefined) {
        params.push(`skip=${options.skip}`);
      }
      if (options.take !== undefined) {
        params.push(`take=${options.take}`);
      }
    }
    if (params.length > 0) {
      url += `?${params.join('&')}`;
    }
    return this.http.get<ReturnHistoryResponse>(url);
  }

  getReturnDetail(returnId: number): Observable<ReturnDetailResponse> {
    return this.http.get<ReturnDetailResponse>(`${this.apiUrlV1}/get-detail/item/${returnId}`);
  }
}
