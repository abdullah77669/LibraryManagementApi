import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { borrowerGuard } from './guards/borrower.guard';
import { staffGuard } from './guards/staff.guard';
import { Library } from './pages/library/library';
import { BorrowerHome } from './pages/borrower-home/borrower-home';
import { MyBooks } from './pages/my-books/my-books';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'borrower' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'borrower', component: BorrowerHome },
  { path: 'my-books', component: MyBooks, canActivate: [authGuard, borrowerGuard] },
  { path: 'library', component: Library, canActivate: [staffGuard] },
  { path: '**', redirectTo: 'borrower' },
];
