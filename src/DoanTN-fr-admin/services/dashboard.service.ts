import { format } from "date-fns"
import { environment } from "../environments/environment"
import { authInterceptor } from "@/interceptors/auth.interceptor"

export interface NewUserStatistic {
  date: string
  newUserCount: number
}

export interface TopSpendingUser {
  userId: number
  userName: string
  email: string
  avatar: string | null
  totalSpending: number
  totalOrders: number
  averageOrderValue: number
}

export interface TopProduct {
  variantId: number
  name: string
  image: string
  price: number
  discountPrice: number
  totalQuantity: number
  averageRating: number
  totalReviews: number
  productAttributes: string[]
}

export interface NewUserStatisticResponse {
  isSuccess: boolean
  statusCode: number
  data?: NewUserStatistic[]
}

export interface TopSpendingUserResponse {
  isSuccess: boolean
  statusCode: number
  data?: TopSpendingUser[]
}

export interface TopProductResponse {
  isSuccess: boolean
  statusCode: number
  data?: TopProduct[]
}

export type ProductStatisticType = 'best-selling' | 'most-viewed' | 'most-carted' | 'most-wished' | 'highest-rated'

export type TimeUnit = 'day' | 'month' | 'year'

export interface OrderStatistic {
  date: string
  totalRevenue: number
  totalOrders: number
  successfulOrders: number
  failedOrders: number
  successRate: number
}

export interface OrderStatisticResponse {
  isSuccess: boolean
  statusCode: number
  data?: OrderStatistic[]
}

export interface ReturnReason {
  reason: string
  count: number
  percentage: number
}

export interface ReturnStatistic {
  date: string
  totalReturns: number
  totalRefundAmount: number
  topReasons: ReturnReason[]
}

export interface ReturnStatisticResponse {
  isSuccess: boolean
  statusCode: number
  data?: ReturnStatistic[]
}

export interface SummaryStatistic {
  date: string;
  totalViews: number;
  totalCartItems: number;
  totalOrders: number;
  viewToCartRate: number;
  cartToOrderRate: number;
  viewToOrderRate: number;
  totalCustomers: number;
  returningCustomers: number;
  returningCustomerRate: number;
}

export interface SummaryStatisticResponse {
  isSuccess: boolean;
  statusCode: number;
  data?: SummaryStatistic[];
}

class DashboardService {
  private apiUrlV1 = `${environment.apiUrlV1}/statistical`

  async getNewUsers(startDate?: Date, endDate?: Date): Promise<NewUserStatistic[]> {
    const params = new URLSearchParams()
    if (startDate) {
      params.append('startDate', format(startDate, "yyyy-MM-dd'T'HH:mm:ss"))
    }
    if (endDate) {
      params.append('endDate', format(endDate, "yyyy-MM-dd'T'HH:mm:ss"))
    }

    const request = new Request(`${this.apiUrlV1}/users/new?${params.toString()}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể lấy thống kê người dùng mới')
    }

    const data: NewUserStatisticResponse = await response.json()
    if (!data.isSuccess) {
      throw new Error(data.statusCode.toString())
    }

    return data.data || []
  }

  async getTopSpendingUsers(topCount: number = 5, startDate?: Date, endDate?: Date): Promise<TopSpendingUser[]> {
    const params = new URLSearchParams()
    params.append('topCount', topCount.toString())
    if (startDate) {
      params.append('startDate', format(startDate, "yyyy-MM-dd'T'HH:mm:ss"))
    }
    if (endDate) {
      params.append('endDate', format(endDate, "yyyy-MM-dd'T'HH:mm:ss"))
    }

    const request = new Request(`${this.apiUrlV1}/users/top-spending?${params.toString()}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể lấy top người dùng chi tiêu nhiều nhất')
    }

    const data: TopSpendingUserResponse = await response.json()
    if (!data.isSuccess) {
      throw new Error(data.statusCode.toString())
    }

    return data.data || []
  }

  async getTopProducts(
    type: ProductStatisticType = 'best-selling',
    startDate?: Date,
    endDate?: Date
  ): Promise<TopProduct[]> {
    const params = new URLSearchParams()
    params.append('type', type)
    if (startDate) {
      params.append('startDate', format(startDate, "yyyy-MM-dd'T'HH:mm:ss"))
    }
    if (endDate) {
      params.append('endDate', format(endDate, "yyyy-MM-dd'T'HH:mm:ss"))
    }

    const request = new Request(`${this.apiUrlV1}/products/top?${params.toString()}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể lấy thống kê sản phẩm')
    }

    const data: TopProductResponse = await response.json()
    if (!data.isSuccess) {
      throw new Error(data.statusCode.toString())
    }

    return data.data || []
  }

  async getOrderStatistics(
    timeUnit: TimeUnit = 'day',
    startDate?: Date,
    endDate?: Date
  ): Promise<OrderStatistic[]> {
    const params = new URLSearchParams()
    params.append('timeUnit', timeUnit)
    if (startDate) params.append('startDate', format(startDate, "yyyy-MM-dd'T'HH:mm:ss"))
    if (endDate) params.append('endDate', format(endDate, "yyyy-MM-dd'T'HH:mm:ss"))

    const request = new Request(`${this.apiUrlV1}/orders?${params.toString()}`, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
    })

    const response = await authInterceptor(request)
    if (!response.ok) throw new Error('Không thể lấy thống kê đơn hàng')
    const data: OrderStatisticResponse = await response.json()
    if (!data.isSuccess) throw new Error(data.statusCode.toString())
    return data.data || []
  }

  async getReturnStatistics(
    timeUnit: TimeUnit = 'day',
    startDate?: Date,
    endDate?: Date
  ): Promise<ReturnStatistic[]> {
    const params = new URLSearchParams()
    params.append('timeUnit', timeUnit)
    if (startDate) params.append('startDate', format(startDate, "yyyy-MM-dd'T'HH:mm:ss"))
    if (endDate) params.append('endDate', format(endDate, "yyyy-MM-dd'T'HH:mm:ss"))

    const request = new Request(`${this.apiUrlV1}/returns?${params.toString()}`, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
    })

    const response = await authInterceptor(request)
    if (!response.ok) throw new Error('Không thể lấy thống kê trả hàng')
    const data: ReturnStatisticResponse = await response.json()
    if (!data.isSuccess) throw new Error(data.statusCode.toString())
    return data.data || []
  }

  async getSummaryStatistics(
    timeUnit: TimeUnit = 'day',
    startDate?: Date,
    endDate?: Date
  ): Promise<SummaryStatistic[]> {
    const params = new URLSearchParams();
    params.append('timeUnit', timeUnit);
    if (startDate) params.append('startDate', format(startDate, "yyyy-MM-dd'T'HH:mm:ss"));
    if (endDate) params.append('endDate', format(endDate, "yyyy-MM-dd'T'HH:mm:ss"));

    const request = new Request(`${this.apiUrlV1}/summary?${params.toString()}`, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) throw new Error('Không thể lấy thống kê tổng quan');
    const data: SummaryStatisticResponse = await response.json();
    if (!data.isSuccess) throw new Error(data.statusCode.toString());
    return data.data || [];
  }
}

export const dashboardService = new DashboardService()
