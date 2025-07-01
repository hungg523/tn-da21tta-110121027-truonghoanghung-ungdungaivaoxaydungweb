import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export enum ReturnStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Completed = 'Completed'
}

export enum ReturnType {
  Bank = 'Bank',
  Momo = 'Momo',
  Other = 'Other'
}

export interface UserAddress {
  addressId: number
  fullName: string
  phoneNumber: string
  address: string
}

export interface Return {
  returnId: number
  orderCode: string
  email: string
  reason: string
  status: ReturnStatus
  variantId: number
  name: string
  imageUrl: string
  quantity: number
  refundAmount: number
  userAddresses: UserAddress
  createdAt: string
  processedAt: string | null
  accountName: string | null
  accountNumber: string | null
  bankName: string | null
  phoneNumber: string | null
  returnType: ReturnType
}

export interface ReturnResponse {
  isSuccess: boolean
  statusCode: number
  data?: {
    items: Return[]
    totalItems: number
  }
}

interface ChangeStatusRequest {
  status: number
}

interface BaseResponse {
  isSuccess: boolean
  statusCode: number
}

class ReturnService {
  private apiUrlV1 = `${environment.apiUrlV1}`

  async getAllReturns(skip = 0, take = 25): Promise<{ items: Return[], totalItems: number }> {
    const params = new URLSearchParams();
    params.append('skip', skip.toString());
    params.append('take', take.toString());
    const url = `${this.apiUrlV1}/return/get-all?${params.toString()}`;
    const request = new Request(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })
    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể tải danh sách đơn trả hàng')
    }
    const data: ReturnResponse = await response.json()
    if (!data.data) {
      throw new Error('Không thể tải danh sách đơn trả hàng')
    }
    return {
      items: data.data.items,
      totalItems: data.data.totalItems
    }
  }

  async changeStatus(returnId: number, status: number): Promise<BaseResponse> {
    const request = new Request(`${this.apiUrlV1}/return/change-status/${returnId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify({ status } as ChangeStatusRequest)
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể cập nhật trạng thái')
    }

    return response.json()
  }
}

export const returnService = new ReturnService()

