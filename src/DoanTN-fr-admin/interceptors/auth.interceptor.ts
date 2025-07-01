import { authService } from '@/services/auth.service';
  
  let isRefreshing = false;
let refreshPromise: Promise<string | null> | null = null;
  
export async function authInterceptor(request: Request): Promise<Response> {
    const accessToken = authService.getAccessToken();
    
    if (accessToken) {
    request.headers.set('Authorization', `Bearer ${accessToken}`);
    }
  
  try {
    const options: RequestInit = {
      method: request.method,
      headers: request.headers,
      body: request.body,
      credentials: 'include',
      cache: request.cache,
      mode: request.mode,
      redirect: request.redirect,
      referrer: request.referrer,
      referrerPolicy: request.referrerPolicy,
      signal: request.signal,
    };

    // Thêm duplex nếu có body
    if (request.body) {
      (options as any).duplex = 'half';
    }

    const response = await fetch(request.url, options);
    
    if (response.status === 401) {
      return handle401Error(request);
        }
    
    return response;
  } catch (error) {
    throw error;
  }
}

async function handle401Error(request: Request): Promise<Response> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshPromise = refreshToken();
  }

  try {
    const newToken = await refreshPromise;
    
    if (newToken) {
      request.headers.set('Authorization', `Bearer ${newToken}`);
      
      const options: RequestInit = {
        method: request.method,
        headers: request.headers,
        body: request.body,
        credentials: 'include',
        cache: request.cache,
        mode: request.mode,
        redirect: request.redirect,
        referrer: request.referrer,
        referrerPolicy: request.referrerPolicy,
        signal: request.signal,
      };

      // Thêm duplex nếu có body
      if (request.body) {
        (options as any).duplex = 'half';
      }

      return fetch(request.url, options);
    }
    
    // Nếu refresh token thất bại, đăng xuất
    await authService.logout();
    throw new Error('Phiên đăng nhập đã hết hạn');
  } finally {
          isRefreshing = false;
    refreshPromise = null;
  }
}

async function refreshToken(): Promise<string | null> {
  try {
    const response = await authService.refreshToken();
    if (response.accessToken) {
          authService.setAccessToken(response.accessToken);
      return response.accessToken;
    }
    return null;
  } catch (error) {
    return null;
  }
  } 