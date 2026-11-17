import { DatePipe, NgFor, NgIf } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../services/auth.service';

interface LoanDetails {
  loanID: number;
  borrowerID: string;
  bookCopyID: number;
  bookID: number;
  bookTitle: string;
  branchName: string;
  loanDate: string;
  dueDate: string;
  returnDate: string | null;
  status: string;
  isOverdue: boolean;
}

@Component({
  imports: [DatePipe, NgFor, NgIf, RouterLink],
  selector: 'app-my-books',
  styleUrl: './my-books.css',
  templateUrl: './my-books.html',
})
export class MyBooks {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loans = signal<LoanDetails[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly userID = computed(() => this.auth.currentUser()?.userID ?? 'Reader');
  readonly currentLoans = computed(() => this.loans().filter(loan => loan.status.toLowerCase() === 'active'));
  readonly returnedLoans = computed(() => this.loans().filter(loan => loan.status.toLowerCase() === 'returned'));

  constructor() {
    this.loadLoans();
  }

  loadLoans(): void {
    this.loading.set(true);
    this.error.set('');
    this.http.get<LoanDetails[]>(`${environment.apiUrl}/loans`).pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: loans => this.loans.set(loans),
      error: () => this.error.set('Your borrowed books could not be loaded. Please try again.'),
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
