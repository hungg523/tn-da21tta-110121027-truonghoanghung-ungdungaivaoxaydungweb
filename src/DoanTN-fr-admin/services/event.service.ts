import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';

export interface Banner {
  id: number;
  title: string;
  description: string;
  url: string;
  link: string;
  status: number;
  position: number;
  createdAt: string;
  updatedAt: string;
}

export interface BannerResponse {
  isSuccess: boolean;
  statusCode: number;
  data?: Banner[];
}

export interface CreateBannerRequest {
  title: string;
  description: string;
  imageData: string | null;
  link: string;
  status: number;
  position: number | null;
}

export interface UpdateBannerRequest {
  title: string | null;
  description: string | null;
  link: string | null;
  status: number;
  position: number | null;
  imageData: string | null;
}

const permittedExtensions = [
  '.png', '.jpg', '.jpeg', '.pdf', '.webp',
  '.mp4', '.avi', '.mov', '.mkv', '.flv'
];

function getBase64Data(base64: string): string {
  const idx = base64.indexOf(',');
  return idx !== -1 ? base64.substring(idx + 1) : base64;
}

function checkFileValid(file: File) {
  const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
  if (!permittedExtensions.includes(ext)) {
    throw new Error('Định dạng file không được phép');
  }
  const isImage = ['.png', '.jpg', '.jpeg', '.webp'].includes(ext);
  const isVideo = ['.mp4', '.avi', '.mov', '.mkv', '.flv'].includes(ext);
  if (isImage && file.size > 2 * 1024 * 1024) {
    throw new Error('Ảnh vượt quá 2MB');
  }
  if (isVideo && file.size > 5 * 1024 * 1024) {
    throw new Error('Video vượt quá 5MB');
  }
}

class EventService {
    private apiUrlV1 = `${environment.apiUrlV1}`;

    async getAllBanner(status?: number, position?: number): Promise<Banner[]> {
        let url = `${this.apiUrlV1}/banner/get-all`;
        const params = [];
        if (typeof status !== 'undefined') params.push(`status=${status}`);
        if (typeof position !== 'undefined') params.push(`position=${position}`);
        if (params.length > 0) url += `?${params.join('&')}`;

        const request = new Request(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
        });

        const response = await authInterceptor(request);
        if (!response.ok) {
            throw new Error('Không thể lấy danh sách banner');
        }

        const data: BannerResponse = await response.json();
        return data.data || [];
    }

    async createBanner(data: CreateBannerRequest & { file?: File }): Promise<BannerResponse> {
        let imageData = data.imageData;
        if (data.file) {
            checkFileValid(data.file);
            imageData = getBase64Data(data.imageData ?? '');
        }
        const request = new Request(`${this.apiUrlV1}/banner/create`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
            body: JSON.stringify({ ...data, imageData: imageData }),
        });
        const response = await authInterceptor(request);
        if (!response.ok) {
            throw new Error('Không thể tạo banner');
        }
        return response.json();
    }

    async updateBanner(id: number, data: UpdateBannerRequest & { file?: File }): Promise<BannerResponse> {
        let imageData = data.imageData;
        if (data.file) {
            checkFileValid(data.file);
            imageData = getBase64Data(data.imageData ?? '');
        }
        const request = new Request(`${this.apiUrlV1}/banner/update/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
            body: JSON.stringify({ ...data, imageData: imageData }),
        });
        const response = await authInterceptor(request);
        if (!response.ok) {
            throw new Error('Không thể cập nhật banner');
        }
        return response.json();
    }

    async deleteBanner(id: number): Promise<BannerResponse> {
        const request = new Request(`${this.apiUrlV1}/banner/delete/${id}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
        });
        const response = await authInterceptor(request);
        if (!response.ok) {
            throw new Error('Không thể xóa banner');
        }
        return response.json();
    }
}

export const eventService = new EventService();