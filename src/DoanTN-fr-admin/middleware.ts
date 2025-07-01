import { NextResponse } from 'next/server';
import { NextRequest } from 'next/server';

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Kiểm tra nếu đang truy cập vào route dashboard
  if (pathname.startsWith('/dashboard') || pathname.startsWith('/')) {
    // Không kiểm tra token trong middleware vì localStorage không có sẵn ở server-side
    // Thay vào đó, chúng ta sẽ kiểm tra trong client-side
    return NextResponse.next();
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    '/dashboard/:path*',
    '/'
  ],
}; 