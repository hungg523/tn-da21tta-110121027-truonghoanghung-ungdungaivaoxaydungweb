import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export interface Product {
  id: number;
  name: string;
  description: string;
  createdDate: string;
  categoryId: number;
  isActived: number;
}

export interface ProductDetail {
  productId: number;
  productName: string;
  detail: {
    key: string;
    value: string;
  }[];
}

export interface CreateProductRequest {
  name: string;
  description: string;
  categoryId: number;
  isActived: number;
}

export interface CreateProductDetailRequest {
  productId: number;
  detailKey: string;
  detailValue: string;
}

export interface UpdateProductRequest {
  name: string;
  description: string;
  categoryId: number;
  isActived: number;
}

export interface UpdateProductDetailRequest {
  productId: number;
  detailKey: string;
  detailValue: string;
}

export interface ApiResponse {
  isSuccess: boolean;
  statusCode: number;
}

export interface ProductResponse {
  isSuccess: boolean;
  statusCode: number;
  data?: Product[];
}

export interface ProductDetailResponse {
  isSuccess: boolean;
  statusCode: number;
  data?: ProductDetail[];
}

class ProductService {
  private apiUrlV1 = `${environment.apiUrlV1}/product`;
  private apiUrlV1Detail = `${environment.apiUrlV1}/product-detail`;

  async getAllProducts(): Promise<Product[]> {
    const request = new Request(`${this.apiUrlV1}/get-all`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể lấy danh sách sản phẩm');
    }

    const data: ProductResponse = await response.json();
    return data.data || [];
  }

  async getAllProductDetails(): Promise<ProductDetail[]> {
    const request = new Request(`${this.apiUrlV1Detail}/get-all`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể lấy danh sách chi tiết sản phẩm');
    }

    const data: ProductDetailResponse = await response.json();
    return data.data || [];
  }

  async createProduct(data: CreateProductRequest): Promise<ApiResponse> {
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
      throw new Error('Không thể tạo sản phẩm');
    }

    return response.json();
  }

  async createProductDetail(data: CreateProductDetailRequest): Promise<ApiResponse> {
    const request = new Request(`${this.apiUrlV1Detail}/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể tạo chi tiết sản phẩm');
    }

    return response.json();
  }

  async updateProduct(id: number, data: UpdateProductRequest): Promise<ApiResponse> {
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
      throw new Error('Không thể cập nhật sản phẩm');
    }

    const result: ApiResponse = await response.json();
    if (!result.isSuccess) {
      throw new Error('Không thể cập nhật sản phẩm');
    }

    return result;
  }

  async updateProductDetail(id: number, data: UpdateProductDetailRequest): Promise<ApiResponse> {
    const request = new Request(`${this.apiUrlV1Detail}/update/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể cập nhật chi tiết sản phẩm');
    }

    return response.json();
  }

  async deleteProductDetail(id: number): Promise<ApiResponse> {
    const request = new Request(`${this.apiUrlV1Detail}/delete/${id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể xóa chi tiết sản phẩm');
    }

    return response.json();
  }
}

export const productService = new ProductService();