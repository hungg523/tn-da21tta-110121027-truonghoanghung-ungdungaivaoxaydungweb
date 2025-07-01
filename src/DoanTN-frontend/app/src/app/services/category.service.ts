import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface Category {
  id: number;
  name: string;
  description?: string;
  categories?: Category[];
  isOpen?: boolean;
}

export interface CategoryResponse {
  isSuccess: boolean;
  statusCode: number;
  data: Category[];
}

export interface CategoryDetail extends Category {
  categories: Category[];
}

interface ApiResponse<T> {
  isSuccess: boolean;
  statusCode: number;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private apiUrlV1 = `${environment.apiUrlV1}/category`;
  private httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    }),
    withCredentials: true
  };

  constructor(private http: HttpClient) { }

  getCategories(): Observable<Category[]> {
    return this.http.get<CategoryResponse>(`${this.apiUrlV1}/get-all?isActived=1`).pipe(
      map(response => {
        // Khởi tạo isOpen = false cho tất cả các danh mục
        const initializeIsOpen = (categories: Category[]): Category[] => {
          return categories.map(category => ({
            ...category,
            isOpen: false,
            categories: category.categories ? initializeIsOpen(category.categories) : []
          }));
        };
        return initializeIsOpen(response.data);
      })
    );
  }

  // Lấy thẳng CategoryDetail luôn
  getCategoryDetail(id: number): Observable<CategoryDetail> {
    return this.http.get<ApiResponse<CategoryDetail>>(`${this.apiUrlV1}/get-detail/${id}`, this.httpOptions)
      .pipe(map(res => res.data));
  }
}