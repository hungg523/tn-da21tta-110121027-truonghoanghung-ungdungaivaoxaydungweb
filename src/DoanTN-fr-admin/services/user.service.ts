import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export interface User {
  id: number;
  avatar: string | null;
  userName: string;
  email: string;
  role: string;
  isActived: number;
  totalOrders: number;
  createdAt: string;
}

export interface UserResponse {
  isSuccess: boolean;
  statusCode: number;
  data?: {
    items: User[]
    totalItems: number
  }
}

export interface ChangeStatusRequest {
  isActived: number;
}

class UserService {
  private apiUrlV1 = `${environment.apiUrlV1}/user`;

  async getAll(role?: number, isActived?: number, skip = 0, take = 25): Promise<{ items: User[], totalItems: number }> {
    let url = `${this.apiUrlV1}/get-all`;
    const params = new URLSearchParams();
    
    if (role !== undefined) {
      params.append('role', role.toString());
    }
    if (isActived !== undefined) {
      params.append('isActived', isActived.toString());
    }
    if (skip !== undefined) { 
      params.append('skip', skip.toString());
    }
    if (take !== undefined) {
      params.append('take', take.toString());
    }

    if (params.toString()) {
      url += `?${params.toString()}`;
    }

    const request = new Request(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể lấy danh sách người dùng');
    }

    const data: UserResponse = await response.json();
    if (!data.data) {
      throw new Error('Không thể lấy danh sách người dùng')
    }
    return {
      items: data.data.items,
      totalItems: data.data.totalItems
    }
  }

  async search(email: string): Promise<User[]> {
    const request = new Request(`${this.apiUrlV1}/search?email=${encodeURIComponent(email)}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể tìm kiếm người dùng');
    }

    const data: UserResponse = await response.json();
    return data.data?.items || [];
  }

  async changeStatus(id: number, data: ChangeStatusRequest): Promise<UserResponse> {
    const request = new Request(`${this.apiUrlV1}/change-status/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể thay đổi trạng thái người dùng');
    }

    return response.json();
  }
}

export const userService = new UserService();