import { environment } from "../environments/environment";
import { authInterceptor } from "@/interceptors/auth.interceptor";

export interface ProductImage {
  id: number;
  title: string;
  url: string;
  position: number;
}

export interface ProductDetail {
  key: string;
  value: string;
}

export interface ProductVariant {
  variantId: number;
  productId: number | null;
  name: string;
  description: string;
  price: number;
  discountPrice: number;
  stock: number;
  reservedStock: number;
  actualStock: number;
  soldQuantity: number;
  totalReviews: number;
  averageRating: number;
  isActived: number;
  images: ProductImage[];
  productsAttributes: any[];
  details: ProductDetail[];
}

export interface GetVariantsParams {
  isActived?: number;
  categoryId?: number;
  skip?: number;
  take?: number;
}

export interface VariantResponse {
  isSuccess: boolean;
  statusCode: number;
  data?: {
    totalItems: number;
    productVariants: ProductVariant[];
  };
}

// Interface cho request tạo biến thể
export interface CreateVariantRequest {
  productId: number;
  price: number;
  stock: number;
  isActived: number;
  avIds: number[];
}

// Interface cho request cập nhật biến thể
export interface UpdateVariantRequest {
  productId: number | null;
  price: number;
  stock: number;
  isActived: number;
  avIds: number[] | null;
}

// Interface cho request tạo hình ảnh
export interface CreateImageRequest {
  title: string;
  imageData: string;
  position: number;
  variantId: number;
}

// Interface cho request cập nhật hình ảnh
export interface UpdateImageRequest {
  title: string;
  imageData?: string | null;
  position: number;
  variantId: number;
}

// Interface cho response API
export interface ApiResponse {
  isSuccess: boolean;
  statusCode: number;
}

export class VariantService {
  private apiUrlV1 = `${environment.apiUrlV1}`;

  async getAll(params?: GetVariantsParams): Promise<{ productVariants: ProductVariant[]; totalItems: number }> {
    const queryParams = new URLSearchParams();
    if (params?.isActived !== undefined) {
      queryParams.append('isActived', params.isActived.toString());
    }
    if (params?.categoryId !== undefined) {
      queryParams.append('categoryId', params.categoryId.toString());
    }
    if (params?.skip !== undefined) {
      queryParams.append('skip', params.skip.toString());
    }
    if (params?.take !== undefined) {
      queryParams.append('take', params.take.toString());
    }

    const request = new Request(`${this.apiUrlV1}/product-variant/get-all?${queryParams.toString()}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể lấy danh sách biến thể sản phẩm');
    }

    const data: VariantResponse = await response.json();
    return {
      productVariants: data.data?.productVariants || [],
      totalItems: data.data?.totalItems || 0
    };
  }

  // Tạo biến thể mới
  async createVariant(request: CreateVariantRequest): Promise<ApiResponse> {
    try {
        const apiRequest = new Request(`${this.apiUrlV1}/product-variant/create`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(request),
      });

      const response = await authInterceptor(apiRequest);
      if (!response.ok) {
        throw new Error('Không thể tạo biến thể');
      }

      return response.json();
    } catch (error) {
      console.error('Error creating variant:', error);
      throw error;
    }
  }

  // Tạo hình ảnh mới
  async createImage(request: CreateImageRequest): Promise<ApiResponse> {
    try {
      const apiRequest = new Request(`${this.apiUrlV1}/product-img/create`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(request),
      });

      const response = await authInterceptor(apiRequest);
      if (!response.ok) {
        throw new Error('Không thể tạo hình ảnh');
      }

      return response.json();
    } catch (error) {
      console.error('Error creating image:', error);
      throw error;
    }
  }

  // Cập nhật biến thể
  async updateVariant(id: number, request: UpdateVariantRequest): Promise<ApiResponse> {
    try {
      const apiRequest = new Request(`${this.apiUrlV1}/product-variant/update/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(request),
      });

      const response = await authInterceptor(apiRequest);
      if (!response.ok) {
        throw new Error('Không thể cập nhật biến thể');
      }

      return response.json();
    } catch (error) {
      console.error('Error updating variant:', error);
      throw error;
    }
  }

  // Cập nhật hình ảnh
  async updateImage(id: number, request: UpdateImageRequest): Promise<ApiResponse> {
    try {
      const apiRequest = new Request(`${this.apiUrlV1}/product-img/update/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(request),
      });

      const response = await authInterceptor(apiRequest);
      if (!response.ok) {
        throw new Error('Không thể cập nhật hình ảnh');
      }

      return response.json();
    } catch (error) {
      console.error('Error updating image:', error);
      throw error;
    }
  }

  // Xóa biến thể (tắt trạng thái hoạt động)
  async deleteVariant(id: number): Promise<ApiResponse> {
    try {
      const apiRequest = new Request(`${this.apiUrlV1}/product-variant/delete/${id}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });

      const response = await authInterceptor(apiRequest);
      if (!response.ok) {
        throw new Error('Không thể xóa biến thể');
      }

      return response.json();
    } catch (error) {
      console.error('Error deleting variant:', error);
      throw error;
    }
  }

  // Xóa hình ảnh
  async deleteImage(id: number): Promise<ApiResponse> {
    try {
      const apiRequest = new Request(`${this.apiUrlV1}/product-img/delete/${id}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });

      const response = await authInterceptor(apiRequest);
      if (!response.ok) {
        throw new Error('Không thể xóa hình ảnh');
      }

      return response.json();
    } catch (error) {
      console.error('Error deleting image:', error);
      throw error;
    }
  }

  async search(name: string, skip = 0, take = 25): Promise<{ productVariants: ProductVariant[]; totalItems: number }> {
    const queryParams = new URLSearchParams();
    queryParams.append('name', name);
    queryParams.append('skip', skip.toString());
    queryParams.append('take', take.toString());
    const request = new Request(`${this.apiUrlV1}/product-variant/search?${queryParams.toString()}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });
    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể tìm kiếm biến thể sản phẩm');
    }
    const data: VariantResponse = await response.json();
    return {
      productVariants: data.data?.productVariants || [],
      totalItems: data.data?.totalItems || 0
    };
  }
}

export const variantService = new VariantService();
