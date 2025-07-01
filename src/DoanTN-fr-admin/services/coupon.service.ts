import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export enum CouponTypeEnum {
  ORDER = 0,
  SHIP = 1
}

export interface CouponType {
  typeId: number
  typeName: string
  description: string
}

export interface Coupon {
  id: number
  code: string
  description: string
  discountPercentage: number
  discountAmount: number
  minOrderValue: number
  maxDiscountAmount: number
  timesUsed: number
  maxUsage: number
  maxUsagePerUser: number
  userSpecific: boolean
  isVip: boolean
  couponType: string
  availableDate: string
  startDate: string
  endDate: string
  createdAt: string
  discountDisplay: string
  term: string
  isAvtived: boolean
}

export interface CouponResponse {
  couponOrder: Coupon[]
  couponShip: Coupon[]
}

export interface CreateCouponRequest {
  code: string
  description: string
  discountPercentage: number
  discountAmount: number
  minOrderValue: number
  maxDiscountAmount: number
  maxUsage: number
  maxUsagePerUser: number
  isVip: boolean
  userSpecific: boolean
  terms: string
  ctId: number
  isActived: boolean
  startDate: string
  endDate: string
}

export interface UpdateCouponRequest {
  code: string
  description: string
  discountPercentage: number
  discountAmount: number
  minOrderValue: number
  maxDiscountAmount: number
  maxUsage: number
  maxUsagePerUser: number
  isVip: boolean
  userSpecific: boolean
  terms: string
  ctId: number
  isActived: boolean
  startDate: string
  endDate: string
}

export interface CreateCouponTypeRequest {
  name: CouponTypeEnum
  description: string
}

export interface UpdateCouponTypeRequest {
  name: CouponTypeEnum
  description: string
}

class CouponService {
  private apiUrlV1 = `${environment.apiUrlV1}`

  async getAllCoupons(isActived?: boolean): Promise<CouponResponse> {
    const url = new URL(`${this.apiUrlV1}/coupon/get-all`)
    if (isActived !== undefined) {
      url.searchParams.append('isActived', isActived.toString())
    }

    const request = new Request(url.toString(), {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể tải danh sách voucher')
    }

    const data = await response.json()
    return data.data
  }

  async getAllCouponTypes(): Promise<CouponType[]> {
    const request = new Request(`${this.apiUrlV1}/coupon-type/get-all`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể tải danh sách loại voucher')
    }

    const data = await response.json()
    return data.data
  }

  async createCoupon(data: CreateCouponRequest): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/coupon/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể tạo voucher')
    }
  }

  async updateCoupon(id: number, data: UpdateCouponRequest): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/coupon/update/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể cập nhật voucher')
    }
  }

  async deleteCoupon(id: number): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/coupon/delete/${id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể xóa voucher')
    }
  }

  async createCouponType(data: CreateCouponTypeRequest): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/coupon-type/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể tạo loại voucher')
    }
  }

  async updateCouponType(id: number, data: UpdateCouponTypeRequest): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/coupon-type/update/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể cập nhật loại voucher')
    }
  }

  async deleteCouponType(id: number): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/coupon-type/delete/${id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể xóa loại voucher')
    }
  }
}

export const couponService = new CouponService()
