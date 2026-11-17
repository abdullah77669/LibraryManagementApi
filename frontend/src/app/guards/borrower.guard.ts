import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const borrowerGuard: CanActivateFn = () => {
  const role = inject(AuthService).currentUser()?.role;
  return role?.toLowerCase() === 'borrower' ? true : inject(Router).createUrlTree(['/library']);
};
