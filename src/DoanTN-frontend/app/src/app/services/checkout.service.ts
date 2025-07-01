import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { catchError } from 'rxjs/operators';

export interface OrderItem {
  variantId: number;
  quantity: number;
  name?: string;
  unitPrice: number;
  discountPrice: number;
  image?: string;
}

export interface OrderHistory {
  oiId: number;
  variantId: number;
  status: string;
  name: string;
  image: string;
  productAttribute: string;
  quantity: number;
  originalPrice: number;
  finalPrice: number;
  totalPrice: number;
  paymentUrl?: string;
  isReview?: boolean;
}

export interface OrderHistoryRespnse {
  isSuccess: boolean;
  statusCode: number;
  data: {
    items: OrderHistory[];
    totalItems: number;
  };
}

export interface CreateOrderRequest {
  userAddressId: number;
  couponId: number | null;
  shipCouponId: number | null;
  code: string;
  payment: string;
  orderItems: OrderItem[];
}

export interface OrderResponse {
  isSuccess: boolean;
  statusCode: number;
  data: {
    paymentUrl?: string;
  };
}

export interface OrderHistoryResponse {
  isSuccess: boolean;
  statusCode: number;
  data: OrderHistory[];
}

export interface PaymentConfirmResponse {
  isSuccess: boolean;
  statusCode: number;
  message?: string;
}

export interface OrderStatusRequest {
  itemStatus: number;
}

export interface OrderStatusResponse {
  isSuccess: boolean;
  statusCode: number;
}

export interface OrderDetail {
  oiId: number;
  variantId: number;
  status: string;
  name: string;
  image: string;
  productAttribute: string;
  quantity: number;
  originalPrice: number;
  finalPrice: number;
  totalPrice: number;
  userAddresses: {
    addressId: number;
    fullName: string;
    phoneNumber: string;
    address: string;
  };
  createdAt: string;
  updatedAt: string;
}

export interface OrderDetailResponse {
  isSuccess: boolean;
  statusCode: number;
  data: OrderDetail;
}

@Injectable({
  providedIn: 'root'
})
export class CheckoutService {
  private apiUrlV1 = `${environment.apiUrlV1}/order`;

  constructor(private http: HttpClient) {}

  createOrder(request: CreateOrderRequest): Observable<OrderResponse> {
    return this.http.post<OrderResponse>(`${this.apiUrlV1}/create`, request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'Đã có lỗi xảy ra. Vui lòng thử lại sau.';
        
        if (error.status === 400) {
          if (error.error?.details?.includes('CouponId must be greater than 0')) {
            errorMessage = 'CouponId phải lớn hơn 0';
          } else if (error.error?.details?.includes('ShipCouponId must be greater than 0')) {
            errorMessage = 'ShipCouponId phải lớn hơn 0';
          } else if (error.error?.details?.includes('ProductId must be greater than 0')) {
            errorMessage = 'ProductId phải lớn hơn 0';
          } else if (error.error?.details?.includes('Quantity must be greater than 0')) {
            errorMessage = 'Số lượng sản phẩm phải lớn hơn 0';
          }
        } else if (error.status === 404) {
          errorMessage = 'Voucher không hợp lệ hoặc chưa có sẵn';
        } else if (error.status === 409) {
          errorMessage = 'Giá sản phẩm không hợp lệ hoặc không đủ số lượng';
        }

        return throwError(() => new Error(errorMessage));
      })
    );
  }

  confirmPayOSPayment(orderCode: string): Observable<PaymentConfirmResponse> {
    return this.http.get<PaymentConfirmResponse>(`${this.apiUrlV1}/payos/confirm/${orderCode}`).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'Đã có lỗi xảy ra khi xác nhận thanh toán';
        if (error.status === 404) {
          errorMessage = 'Không tìm thấy đơn hàng';
        }
        return throwError(() => new Error(errorMessage));
      })
    );
  }

  getOrderHistory(options?: { status?: number, skip?: number, take?: number }): Observable<OrderHistoryRespnse> {
    let url = `${this.apiUrlV1}/view-history`;
    let params = [];
    if (options) {
      if (options.status !== undefined && options.status !== null) {
        params.push(`status=${options.status}`);
      }
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
    return this.http.get<OrderHistoryRespnse>(url);
  }

  requestCancelOrder(oiId: number): Observable<OrderStatusResponse> {
    const request: OrderStatusRequest = {
      itemStatus: 4 // 4 is Cancelled status
    };
    return this.http.put<OrderStatusResponse>(`${this.apiUrlV1}/change-status/${oiId}`, request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          let errorMessage = 'Đã có lỗi xảy ra khi yêu cầu hủy đơn hàng';
          if (error.status === 404) {
            errorMessage = 'Không tìm thấy đơn hàng';
          }
          return throwError(() => new Error(errorMessage));
        })
      );
  }

  requestSuccessOrder(oiId: number): Observable<OrderStatusResponse> {
    const request: OrderStatusRequest = {
      itemStatus: 3 // 3 is Success status
    };
    return this.http.put<OrderStatusResponse>(`${this.apiUrlV1}/change-status/${oiId}`, request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          let errorMessage = 'Đã có lỗi xảy ra khi yêu cầu thành công đơn hàng';
          if (error.status === 404) {
            errorMessage = 'Không tìm thấy đơn hàng';
          }
          return throwError(() => new Error(errorMessage));
        })
      );
  }

  getOrderDetail(oiId: number): Observable<OrderDetailResponse> {
    return this.http.get<OrderDetailResponse>(`${this.apiUrlV1}/get-detail/item/${oiId}`);
  }
}
