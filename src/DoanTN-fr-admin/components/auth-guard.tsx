'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { authService } from '@/services/auth.service';

export default function AuthGuard({ children }: { children: React.ReactNode }) {
  const router = useRouter();

  useEffect(() => {
    const checkAuth = () => {
      const token = authService.getAccessToken();
      if (!token) {
        router.push('/login');
      }
    };

    checkAuth();
  }, [router]);

  return <>{children}</>;
} 