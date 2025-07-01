import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { catchError } from 'rxjs/operators';

export interface Banner {
  id: number;
  title: string;
  description: string;
  url: string;
  imageUrl: string;
  link: string;
  buttonText: string;
  startDate: string;
  endDate: string;
  status: number;
  position: number;
}

export interface BannerResponse {
  isSuccess: boolean;
  statusCode: number;
  data: Banner[];
}

@Injectable({
  providedIn: 'root'
})
export class BannerService {

  private apiUrlV1 = `${environment.apiUrlV1}/banner`;

  constructor(private http: HttpClient) {}

  getAllBanners(status: number, position: number): Observable<BannerResponse> {
    return this.http.get<BannerResponse>(`${this.apiUrlV1}/get-all?status=${status}&position=${position}`).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'Đã có lỗi xảy ra. Vui lòng thử lại sau.';
        if (error.status === 404) {
          errorMessage = 'Không tìm thấy banner';
        }
        return throwError(() => new Error(errorMessage));
      })
    );
  }
}
