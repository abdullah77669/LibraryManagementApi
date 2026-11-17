import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { canAccessManagementArea } from '../models/staff-permissions';

export const staffGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }
  return canAccessManagementArea(auth.currentUser()?.role, 'dashboard')
    ? true
    : router.createUrlTree(['/borrower']);
};
