import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export interface Attribute {
  id: number;
  name: string;
}

export interface AttributeValue {
  id: number;
  attributeId: number;
  value: string;
}

export interface CreateAttributeRequest {
  name: string;
}

export interface CreateAttributeValueRequest {
  attributeId: number;
  value: string;
}

export interface UpdateAttributeRequest {
  name: string;
}

export interface UpdateAttributeValueRequest {
  attributeId: number;
  value: string;
}

export interface ApiResponse {
  isSuccess: boolean;
  statusCode: number;
}

export interface AttributeResponse {
  isSuccess: boolean;
  statusCode: number;
  data?: Attribute[];
}

export interface AttributeValueResponse {
  isSuccess: boolean;
  statusCode: number;
  data?: AttributeValue[];
}

class AttributeService {
  private apiUrlV1 = `${environment.apiUrlV1}/attribute`;

  async getAllAttributes(): Promise<Attribute[]> {
    const request = new Request(`${this.apiUrlV1}/get-all`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể lấy danh sách thuộc tính');
    }

    const data = await response.json();
    return data.data || [];
  }

  async getAllAttributeValues(): Promise<AttributeValue[]> {
    const request = new Request(`${this.apiUrlV1}-value/get-all`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể lấy danh sách giá trị thuộc tính');
    }

    const data = await response.json();
    return data.data || [];
  }

  async createAttribute(data: CreateAttributeRequest): Promise<ApiResponse> {
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
      throw new Error('Không thể tạo thuộc tính');
    }

    return response.json();
  }

  async createAttributeValue(data: CreateAttributeValueRequest): Promise<ApiResponse> {
    const request = new Request(`${this.apiUrlV1}-value/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể tạo giá trị thuộc tính');
    }

    return response.json();
  }

  async updateAttribute(id: number, data: UpdateAttributeRequest): Promise<ApiResponse> {
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
      throw new Error('Không thể cập nhật thuộc tính');
    }

    return response.json();
  }

  async updateAttributeValue(id: number, data: UpdateAttributeValueRequest): Promise<ApiResponse> {
    const request = new Request(`${this.apiUrlV1}-value/update/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể cập nhật giá trị thuộc tính');
    }

    return response.json();
  }

  async deleteAttribute(id: number): Promise<ApiResponse> {
    const request = new Request(`${this.apiUrlV1}/delete/${id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể xóa thuộc tính');
    }

    return response.json();
  }

  async deleteAttributeValue(id: number): Promise<ApiResponse> {
    const request = new Request(`${this.apiUrlV1}-value/delete/${id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể xóa giá trị thuộc tính');
    }

    return response.json();
  }
}

export const attributeService = new AttributeService();
