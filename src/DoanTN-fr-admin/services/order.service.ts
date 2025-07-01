import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export enum OrderStatus {
  Pending = 'Pending',
  Confirmed = 'Confirmed',
  Packed = 'Packed',
  Successed = 'Successed',
  Shipping = 'Shipping',
  Mixed = 'Mixed',
  Cancelled = 'Cancelled',
  Returned = 'Returned',
  Paid = 'Paid',
  Failed = 'Failed',
}

export enum ItemStatus {
  Pending = 'Pending',
  Packed = 'Packed',
  Shipping = 'Shipping',
  Delivered = 'Delivered',
  Cancelled = 'Cancelled',
  PendingReturn = 'PendingReturn',
  ApprovedReturn = 'ApprovedReturn',
  RejectedReturn = 'RejectedReturn',
}

export interface UserAddress {
  addressId: number
  fullName: string
  phoneNumber: string
  address: string
}

export interface OrderItem {
  oiId: number
  variantId: number
  status: ItemStatus
  name: string
  image: string
  productAttribute: string
  quantity: number
  originalPrice: number
  finalPrice: number
  totalPrice: number
  userAddresses: UserAddress
}

export interface Order {
  orderId: number
  code: string
  email: string
  status: OrderStatus
  totalQuantity: number
  totalAmount: number
}

export interface OrderResponse {
  isSuccess: boolean
  statusCode: number
  data?: {
    totalItems: number
    items: Order[]
  } | null
}

export interface OrderDetail extends Order {
  items: OrderItem[]
}

interface ChangeItemStatusRequest {
  itemStatus: number
}

interface BaseResponse {
  isSuccess: boolean
  statusCode: number
}

class OrderService {
  private apiUrlV1 = `${environment.apiUrlV1}`

  async getAllOrders(status?: string, skip = 0, take = 25): Promise<{ items: Order[], totalItems: number }> {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    params.append('skip', skip.toString());
    params.append('take', take.toString());
    const url = `${this.apiUrlV1}/order/get-all?${params.toString()}`;
    const request = new Request(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })
    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể tải danh sách đơn hàng')
    }
    const data: OrderResponse = await response.json()
    if (!data.data) {
      throw new Error('Không thể tải danh sách đơn hàng')
    }
    return {
        items: data.data?.items || [],
        totalItems: data.data?.totalItems || 0
    };
  }

  async getOrderDetail(orderId: number): Promise<OrderDetail> {
    const request = new Request(`${this.apiUrlV1}/order/get-detail/${orderId}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })
    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể tải chi tiết đơn hàng')
    }
    const data = await response.json()
    return data.data
  }
  async changeItemStatus(oiId: number, itemStatus: number): Promise<BaseResponse> {
    const request = new Request(`${this.apiUrlV1}/order/change-status/${oiId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify({ itemStatus } as ChangeItemStatusRequest)
    })
    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể cập nhật trạng thái sản phẩm trong đơn hàng')
    }
    return response.json()
  }
}

export const orderService = new OrderService()
