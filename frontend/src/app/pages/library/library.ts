import { Component, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgFor, NgIf } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { finalize } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../services/auth.service';
import { CatalogManagement } from './catalog-management';

interface BookSummary { availableCopies: number; }
interface LoanSummary { status: string; isOverdue: boolean; }
interface BorrowerSummary { userID: string; }
interface BranchSummary { branchID: number; }

@Component({
  imports: [CatalogManagement, NgFor, NgIf, RouterLink],
  selector: 'app-library',
  styleUrl: './library.css',
  templateUrl: './library.html',
})
export class Library {
  readonly loading = signal(true);
  readonly error = signal('');
  readonly selectedSection = signal<'dashboard' | 'catalog' | 'loans' | 'borrowers' | 'branches'>('dashboard');
  readonly titleCount = signal(0);
  readonly availableCopies = signal(0);
  readonly activeLoans = signal(0);
  readonly overdueLoans = signal(0);
  readonly borrowerCount = signal(0);
  readonly branchCount = signal(0);
  readonly isAdmin = computed(() => this.auth.currentUser()?.role?.toLowerCase() === 'admin');
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  constructor() {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading.set(true);
    this.error.set('');
    forkJoin({
      books: this.http.get<BookSummary[]>(`${environment.apiUrl}/book-details`),
      loans: this.http.get<LoanSummary[]>(`${environment.apiUrl}/loans`),
      borrowers: this.isAdmin() ? this.http.get<BorrowerSummary[]>(`${environment.apiUrl}/borrowers`) : of([]),
      branches: this.isAdmin() ? this.http.get<BranchSummary[]>(`${environment.apiUrl}/branches`) : of([]),
    }).pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: data => {
        this.titleCount.set(data.books.length);
        this.availableCopies.set(data.books.reduce((total, book) => total + book.availableCopies, 0));
        this.activeLoans.set(data.loans.filter(loan => loan.status.toLowerCase() === 'active').length);
        this.overdueLoans.set(data.loans.filter(loan => loan.isOverdue).length);
        this.borrowerCount.set(data.borrowers.length);
        this.branchCount.set(data.branches.length);
      },
      error: () => this.error.set('Dashboard information could not be loaded. Check your connection and try again.'),
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
