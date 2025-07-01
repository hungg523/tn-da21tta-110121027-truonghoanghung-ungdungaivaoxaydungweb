import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export interface Promotion {
  promotionId: number;
  name: string;
  description: string;
  discountAmount: number | null;
  discountPercentage: number;
  startDate: string;
  endDate: string;
  isActived: number;
  isFlashSale: boolean;
  promotions: {
    pmId: number;
    variantId: number;
    variantName: string;
  }[];
}

export interface ProductPromotion {
  pmId: number;
  productId: number | null;
  variantId: number;
  promotionId: number;
  productName?: string;
  variantName?: string;
}

export interface GetPromotionsParams {
  isActived?: number;
}

export interface CreatePromotionRequest {
  name: string;
  description: string;
  discountPercentage: number;
  discountAmount: number | null;
  startDate: string;
  endDate: string;
  isActived: number;
  isFlashSale: boolean;
}

export interface UpdatePromotionRequest {
  name: string;
  description: string;
  discountPercentage: number;
  discountAmount: number | null;
  startDate: string;
  endDate: string;
  isActived: number;
  isFlashSale: boolean;
}

export interface CreateProductPromotionRequest {
  variantIds: number[];
  promotionId: number;
}

export interface UpdateProductPromotionRequest {
  variantId: number;
  promotionId: number;
}

export interface PromotionResponse {
  isSuccess: boolean;
  statusCode: number;
  data?: Promotion[];
}

export interface ProductPromotionResponse {
  isSuccess: boolean;
  statusCode: number;
  data?: ProductPromotion[];
}

class PromotionService {
  private apiUrlV1 = `${environment.apiUrlV1}`;

  async getAllPromotions(isActived?: number): Promise<Promotion[]> {
    const queryParams = new URLSearchParams();
    if (isActived !== undefined) {
      queryParams.append('isActived', isActived.toString());
    }

    const request = new Request(`${this.apiUrlV1}/promotion/get-all?${queryParams.toString()}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể lấy danh sách khuyến mãi');
}

    const data: PromotionResponse = await response.json();
    return data.data || [];
  }

  async getAllProductPromotions(): Promise<ProductPromotion[]> {
    const request = new Request(`${this.apiUrlV1}/product-promotion/get-all`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể lấy danh sách sản phẩm khuyến mãi');
    }

    const data = await response.json();
    return data.data || [];
  }

  async createPromotion(data: CreatePromotionRequest): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/promotion/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể tạo khuyến mãi');
    }
  }

  async createProductPromotion(data: CreateProductPromotionRequest): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/product-promotion/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể thêm sản phẩm vào khuyến mãi');
    }
  }

  async updatePromotion(id: number, data: UpdatePromotionRequest): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/promotion/update/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể cập nhật khuyến mãi');
    }
  }

  async updateProductPromotion(id: number, data: UpdateProductPromotionRequest): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/product-promotion/update/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể cập nhật sản phẩm khuyến mãi');
    }
  }

  async deletePromotion(id: number): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/promotion/delete/${id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể xóa khuyến mãi');
    }
  }

  async deleteProductPromotion(id: number): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/product-promotion/delete/${id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể xóa sản phẩm khuyến mãi');
    }
  }
}

export const promotionService = new PromotionService();
