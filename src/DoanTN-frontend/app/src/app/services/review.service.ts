import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ReviewImage {
  imageUrl: string;
}

export interface ReviewUser {
  id: number;
  username: string;
  avatar: string;
}

export interface Reply {
  user: ReviewUser;
  replyMessage: string;
  createdAt: string;
}

export interface Review {
  reviewId: number;
  user: ReviewUser;
  rating: number;
  comment: string;
  createdAt: string;
  images: ReviewImage[];
  reply: Reply | null;
}

export interface CreateReviewRequest {
  variantId: number;
  oiId: number;
  rating: number;
  comment: string;
  imageDatas: string[];
}

export interface CreateReviewResponse {
  isSuccess: boolean;
  statusCode: number;
}

export interface ReportReviewRequest {
  reviewId: number;
  reason: string;
}

export interface ReviewGroup {
  variantId: number;
  reviews: Review[];
}

export interface ReviewResponse {
  isSuccess: boolean;
  statusCode: number;
  data: {
    items: ReviewGroup[];
    totalItems: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  };
}

export interface ReviewSummaryResponse {
  isSuccess: boolean;
  statusCode: number;
  data: {
    variantId: number;
    totalReviews: number;
    averageRating: number;
    ratingsBreakdown: {
      oneStar: number;
      twoStar: number;
      threeStar: number;
      fourStar: number;
      fiveStar: number;
    };
  };
}

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private apiUrlV1 = `${environment.apiUrlV1}/review`;
  private allowedFileTypes = {
    image: ['.png', '.jpg', '.jpeg', '.webp'],
    video: ['.mp4', '.avi', '.mov', '.mkv', '.flv'],
    document: ['.pdf']
  };
  private maxFileSize = {
    image: 2 * 1024 * 1024, // 2MB
    video: 5 * 1024 * 1024  // 5MB
  };

  constructor(private http: HttpClient) { }

  createReview(request: CreateReviewRequest): Observable<CreateReviewResponse> {
    return this.http.post<CreateReviewResponse>(`${this.apiUrlV1}/create`, request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'Đã có lỗi xảy ra khi tạo đánh giá';
        
        if (error.status === 400) {
          if (error.error?.message?.includes('Rating must be between 1 and 5')) {
            errorMessage = 'Đánh giá phải từ 1 đến 5 sao';
          } else if (error.error?.message?.includes('Comment length exceeds 500 characters')) {
            errorMessage = 'Nội dung đánh giá không được vượt quá 500 ký tự';
          }
        }
        
        return throwError(() => new Error(errorMessage));
      })
    );
  }

  reportReview(request: ReportReviewRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrlV1}/report`, request).pipe(
      catchError((error) => this.handleError(error))
    );
  }

  getReviews(
    variantId: number,
    pageNumber: number = 1,
    pageSize: number = 5,
    star?: number | null,
    isFilterByImage: boolean | null = null
  ): Observable<ReviewResponse> {
    let params = new HttpParams()
      .set('variantId', variantId.toString())
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString())
      .set('isDeleted', 'false');

    if (star !== null && star !== undefined) {
      params = params.set('star', star.toString());
    }

    if (isFilterByImage !== null) {
      params = params.set('isFilterByImage', isFilterByImage.toString());
    }

    return this.http.get<ReviewResponse>(`${this.apiUrlV1}/get-all`, { params });
  }

  getReviewSummary(variantId: number): Observable<ReviewSummaryResponse> {
    return this.http.get<ReviewSummaryResponse>(`${this.apiUrlV1}/summary/${variantId}`);
  }

  validateFile(file: File): { isValid: boolean; error?: string } {
    // Kiểm tra định dạng file
    const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase();
    
    // Xác định loại file
    let fileType: 'image' | 'video' | 'document' | null = null;
    if (this.allowedFileTypes.image.includes(fileExtension)) {
      fileType = 'image';
    } else if (this.allowedFileTypes.video.includes(fileExtension)) {
      fileType = 'video';
    } else if (this.allowedFileTypes.document.includes(fileExtension)) {
      fileType = 'document';
    }

    if (!fileType) {
      return {
        isValid: false,
        error: 'Định dạng file không được hỗ trợ'
      };
    }

    // Kiểm tra kích thước file
    if (fileType === 'image' && file.size > this.maxFileSize.image) {
      return {
        isValid: false,
        error: 'Kích thước ảnh không được vượt quá 2MB'
      };
    }

    if (fileType === 'video' && file.size > this.maxFileSize.video) {
      return {
        isValid: false,
        error: 'Kích thước video không được vượt quá 5MB'
      };
    }

    return { isValid: true };
  }

  convertFileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => {
        const base64String = reader.result as string;
        // Lấy phần sau dấu ","
        const base64Data = base64String.split(',')[1];
        resolve(base64Data);
      };
      reader.onerror = error => reject(error);
    });
  }

  private handleError(error: any): Observable<never> {
    let errorMessage = 'Đã có lỗi xảy ra. Vui lòng thử lại sau.';
    if (error.status === 400) {
      if (error.error?.details?.includes('Rating cannot be null')) {
        errorMessage = 'Đánh giá không thể để trống';
      } else if (error.error?.details?.includes('Rating range from 1 to 5')) {
        errorMessage = 'Đánh giá phải từ 1 đến 5 sao';
      } else if (error.error?.details?.includes('Comment cannot exceed 500 characters')) {
        errorMessage = 'Lý do đánh giá không được vượt quá 500 ký tự';
      }
    } else if (error.status === 409) {
      errorMessage = 'Bạn đã báo cáo đánh giá này, vui lòng chờ phản hồi từ quản trị viên';
    }
    return throwError(() => new Error(errorMessage));
  }
}
