import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export interface Prompt {
  id: number;
  name: string | null;
  content: string;
  createdAt: string;
}

export interface CreatePromptRequest {
  name: string | null;
  content: string;
}

export interface UpdatePromptRequest {
  name: string | null;
  content: string;
}

export interface GenerateDescriptionRequest {
  prompt: string;
}

export interface GenerateDescriptionResponse {
  isSuccess: boolean;
  statusCode: number;
  data: {
    description: string;
  };
}

export interface PromptResponse {
  isSuccess: boolean;
  statusCode: number;
  data?: Prompt[];
}

export interface UserActivityMetrics {
  total_users: number;
  active_users: number;
  inactive_users: number;
  avg_interactions_per_user: number;
  max_interactions_per_user: number;
  min_interactions_per_user: number;
}

export interface ProductActivityMetrics {
  total_products: number;
  popular_products: number;
  unpopular_products: number;
  avg_interactions_per_product: number;
  max_interactions_per_product: number;
  min_interactions_per_product: number;
}

export interface ModelPerformanceMetrics {
  "precision@k": number;
  "recall@k": number;
  "map@k": number;
}

export interface ScheduleResponse {
  next_run_time: string | null;
  cron: string | null;
}

export interface MessageResponse {
  message: string;
}

// Interface cho file model
export interface ModelFile {
  filename: string;
  created: number;
  size: number;
}

class AIService {
  private apiUrlV1 = `${environment.apiUrlV1}/prompt`;
  private apiAI = `${environment.apiAI}`;

  async getAllPrompts(): Promise<Prompt[]> {
    const request = new Request(`${this.apiUrlV1}/get-all`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể lấy danh sách prompt');
    }

    const data: PromptResponse = await response.json();
    return data.data || [];
  }

  async createPrompt(data: CreatePromptRequest): Promise<PromptResponse> {
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
      throw new Error('Không thể tạo prompt');
    }

    return response.json();
  }

  async updatePrompt(id: number, data: UpdatePromptRequest): Promise<PromptResponse> {
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
      throw new Error('Không thể cập nhật prompt');
    }

    return response.json();
  }

  async deletePrompt(id: number): Promise<PromptResponse> {
    const request = new Request(`${this.apiUrlV1}/delete/${id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể xóa prompt');
    }

    return response.json();
  }

  async generateDescription(data: GenerateDescriptionRequest): Promise<string> {
    const request = new Request(`${this.apiUrlV1}/generate-description`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể tạo mô tả');
    }

    const responseData: GenerateDescriptionResponse = await response.json();
    if (!responseData.isSuccess) {
      throw new Error('Không thể tạo mô tả');
    }

    // Format lại mô tả
    let description = responseData.data.description;
    
    // Xóa các phần dư thừa
    description = description
      .replace(/## Mô tả sản phẩm.*?\n\n/, '') // Xóa tiêu đề
      .replace(/\*\*Từ khóa:.*?\*\*\n\n/, '') // Xóa phần từ khóa
      .replace(/\*\*Khuyến khích hành động:.*?\n\n/, '') // Xóa phần khuyến khích hành động
      .replace(/\*\*Lưu ý:.*$/, '') // Xóa phần lưu ý
      .trim();

    return description;
  }

  async getUserActivityMetrics(): Promise<UserActivityMetrics> {
    const response = await fetch(`${this.apiAI}/metrics/user-activity`);
    if (!response.ok) throw new Error("Không thể lấy thống kê hoạt động user");
    return await response.json();
  }

  async getProductActivityMetrics(): Promise<ProductActivityMetrics> {
    const response = await fetch(`${this.apiAI}/metrics/product-activity`);
    if (!response.ok) throw new Error("Không thể lấy thống kê hoạt động sản phẩm");
    return await response.json();
  }

  async getModelPerformanceMetrics(): Promise<ModelPerformanceMetrics> {
    const response = await fetch(`${this.apiAI}/metrics/model-performance`);
    if (!response.ok) throw new Error("Không thể lấy thống kê hiệu suất model");
    return await response.json();
  }

  async scheduleTrain(cronExpression: string): Promise<MessageResponse> {
    const response = await fetch(`${this.apiAI}/admin/schedule-train?cron=${encodeURIComponent(cronExpression)}`, {
      method: 'POST',
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.detail || "Không thể đặt lịch train");
    }
    return await response.json();
  }

  async getSchedule(): Promise<ScheduleResponse> {
    const response = await fetch(`${this.apiAI}/admin/schedule-train`);
    if (!response.ok) {
       const error = await response.json();
       throw new Error(error.detail || "Không thể lấy lịch train");
    }
    return await response.json();
  }

  async deleteSchedule(): Promise<MessageResponse> {
    const response = await fetch(`${this.apiAI}/admin/schedule-train`, {
      method: 'DELETE',
    });
     if (!response.ok) {
        const error = await response.json();
        throw new Error(error.detail || "Không thể xóa lịch train");
     }
    return await response.json();
  }

  // Although the primary use is SSE, keep a simple POST method
  async triggerTrain(): Promise<MessageResponse> {
    const response = await fetch(`${this.apiAI}/admin/train-model`, {
      method: 'POST',
    });
     if (!response.ok) {
        const error = await response.json();
        throw new Error(error.detail || "Không thể trigger train model");
     }
    return await response.json();
  }

  // Lấy danh sách các file model đã train
  async getAllModelFiles(): Promise<ModelFile[]> {
    const response = await fetch(`${this.apiAI}/admin/models`);
    if (!response.ok) throw new Error("Không thể lấy danh sách file model");
    return await response.json();
  }

  // Chọn file model để sử dụng
  async useModelFile(filename: string): Promise<MessageResponse> {
    const response = await fetch(`${this.apiAI}/admin/models/use?filename=${encodeURIComponent(filename)}`, {
      method: 'POST',
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.detail || "Không thể chọn file model");
    }
    return await response.json();
  }

  // Xóa file model
  async deleteModelFile(filename: string): Promise<MessageResponse> {
    const response = await fetch(`${this.apiAI}/admin/models/${encodeURIComponent(filename)}`, {
      method: 'DELETE',
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.detail || "Không thể xóa file model");
    }
    return await response.json();
  }
}

export const aiService = new AIService(); 