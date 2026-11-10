import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { Library } from './pages/library/library';
import { BorrowerHome } from './pages/borrower-home/borrower-home';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'library' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'borrower', component: BorrowerHome, canActivate: [authGuard] },
  { path: 'library', component: Library, canActivate: [authGuard] },
  { path: '**', redirectTo: 'library' },
];
