import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export interface User {
  id: number
  username: string
  avatar: string
}

export interface ReviewImage {
  imageUrl: string
}

export interface ReviewReply {
  user: User
  replyMessage: string
  createdAt: string
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

export interface VariantReviews {
  variantId: number
  reviews: Review[]
}

interface ReplyRequest {
  reviewId: number
  replyText: string
}

interface BaseResponse {
  isSuccess: boolean
  statusCode: number
}

class ReviewService {
  private apiUrlV1 = `${environment.apiUrlV1}`

  async getAllReviews(variantId: number, star?: number): Promise<VariantReviews> {
    let url = `${this.apiUrlV1}/review/get-all-by-admin?variantId=${variantId}`
    if (star) url += `&star=${star}`
    const request = new Request(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })
    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể tải danh sách bình luận')
    }
    const data = await response.json()
    return data.data[0]
  }

  async replyReview(reviewId: number, replyText: string): Promise<BaseResponse> {
    const request = new Request(`${this.apiUrlV1}/review/reply`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify({ reviewId, replyText } as ReplyRequest)
    })
    const response = await authInterceptor(request)
    if (!response.ok) {
      throw new Error('Không thể gửi phản hồi')
    }
    return response.json()
  }
}

export const reviewService = new ReviewService()


