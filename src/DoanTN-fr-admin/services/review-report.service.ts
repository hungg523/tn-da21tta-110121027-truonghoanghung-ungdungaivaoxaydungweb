import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export enum ReportStatus {
  Pending = 'Pending',
  Resolved = 'Resolved',
  Rejected = 'Rejected'
}

export interface User {
  id: number
  username: string
  avatar: string
}

export interface ReviewImage {
  imageUrl: string
}

export interface ReviewReply {
  id: number
  content: string
  createdAt: string
  user: User
}

export interface Review {
  reviewId: number
  user: User
  rating: number
  comment: string
  createdAt: string
  images: ReviewImage[]
  reply: ReviewReply | null
}

export interface ReportUser {
  user: User
  reason: string
  createdAt: string
}

export interface ReviewReport {
  id: number
  review: Review
  status: ReportStatus
  totalReports: number
  reportUsers: ReportUser[]
}

interface HandleReportRequest {
  status: number
}

interface BaseResponse {
  isSuccess: boolean
  statusCode: number
}

class ReviewReportService {
  private apiUrlV1 = `${environment.apiUrlV1}`

  async getAllReports(): Promise<ReviewReport[]> {
    const request = new Request(`${this.apiUrlV1}/review/report/get-all`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể tải danh sách báo cáo')
    }

    const data = await response.json()
    return data.data
  }

  async handleReport(reportId: number, status: number): Promise<BaseResponse> {
    const request = new Request(`${this.apiUrlV1}/review/report/handle/${reportId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify({ status } as HandleReportRequest)
    })

    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể xử lý báo cáo')
    }

    return response.json()
  }
}

export const reviewReportService = new ReviewReportService()
