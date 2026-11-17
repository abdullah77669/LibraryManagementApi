import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).token;
  const isPublicCatalogRequest = req.method === 'GET' && new URL(req.url).pathname.endsWith('/book-details');

  if (!token || isPublicCatalogRequest || req.url.endsWith('/Users/login') || req.url.endsWith('/Users/register')) {
    return next(req);
  }

  return next(req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  }));
};
