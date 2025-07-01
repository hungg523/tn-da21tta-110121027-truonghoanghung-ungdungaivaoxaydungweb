import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export interface Category {
  id: number;
  catPid: number;
  name: string;
  description: string;
  isActived: number;
  createdAt: string;
  updatedAt: string;
  categories?: Category[];
}

export interface CreateCategoryRequest {
  catPid: number;
  name: string;
  description: string;
  isActived: number;
}

export interface UpdateCategoryRequest {
  catPid: number;
  name: string;
  description: string;
  isActived: number;
}

export interface CategoryResponse {
  isSuccess: boolean;
  statusCode: number;
  data?: Category[];
}

class CategoryService {
  private apiUrlV1 = `${environment.apiUrlV1}/category`;

  async getAll(): Promise<Category[]> {
    const request = new Request(`${this.apiUrlV1}/get-all`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể lấy danh sách danh mục');
    }

    const data: CategoryResponse = await response.json();
    return data.data || [];
  }

  async create(data: CreateCategoryRequest): Promise<CategoryResponse> {
    const request = new Request(`${this.apiUrlV1}/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể tạo danh mục');
    }

    return response.json();
  }

  async update(id: number, data: UpdateCategoryRequest): Promise<CategoryResponse> {
    const request = new Request(`${this.apiUrlV1}/update/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể cập nhật danh mục');
    }

    return response.json();
  }
}

export const categoryService = new CategoryService();
