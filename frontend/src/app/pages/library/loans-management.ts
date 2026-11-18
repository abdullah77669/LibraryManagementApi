import { DatePipe, NgFor, NgIf } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

interface LoanRecord {
  loanID: number;
  borrowerID: string;
  borrowerName: string;
  bookCopyID: number;
  bookTitle: string;
  branchName: string;
  loanDate: string;
  dueDate: string;
  returnDate: string | null;
  status: string;
  isOverdue: boolean;
}
interface BookRecord { bookID: number; title: string; }
interface BranchRecord { branchID: number; name: string; }
interface CopyRecord { bookCopyID: number; bookID: number; branchID: number; copyNumber: number; status: string | null; }
interface CopyOption { id: number; label: string; }
type LoanFilter = 'All' | 'Active' | 'Returned';

@Component({
  imports: [DatePipe, FormsModule, NgFor, NgIf],
  selector: 'app-loans-management',
  styleUrl: './loans-management.css',
  templateUrl: './loans-management.html',
})
export class LoansManagement {
  private readonly http = inject(HttpClient);
  readonly loans = signal<LoanRecord[]>([]);
  readonly availableCopies = signal<CopyOption[]>([]);
  readonly loading = signal(true);
  readonly checkoutOpen = signal(false);
  readonly checkoutLoading = signal(false);
  readonly returningLoanID = signal<number | null>(null);
  readonly error = signal('');
  readonly message = signal('');
  readonly filter = signal<LoanFilter>('All');
  readonly checkoutForm = { borrowerID: '', bookCopyID: null as number | null, dueDate: '' };
  readonly filteredLoans = computed(() => this.filter() === 'All'
    ? this.loans()
    : this.loans().filter(loan => loan.status.toLowerCase() === this.filter().toLowerCase()));

  constructor() {
    this.loadLoans();
    this.loadAvailableCopies();
  }

  loadLoans(): void {
    this.loading.set(true);
    this.error.set('');
    this.http.get<LoanRecord[]>(`${environment.apiUrl}/loans`).pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: loans => this.loans.set(loans),
      error: () => this.error.set('Loans could not be loaded.'),
    });
  }

  loadAvailableCopies(): void {
    forkJoin({
      copies: this.http.get<CopyRecord[]>(`${environment.apiUrl}/bookcopies`),
      books: this.http.get<BookRecord[]>(`${environment.apiUrl}/Book`),
      branches: this.http.get<BranchRecord[]>(`${environment.apiUrl}/branches`),
    }).pipe(catchError(() => of({ copies: [], books: [], branches: [] }))).subscribe(data => {
      const activeCopyIDs = new Set(this.loans()
        .filter(loan => loan.status.toLowerCase() === 'active')
        .map(loan => loan.bookCopyID));
      this.availableCopies.set(data.copies
        .filter(copy => (!copy.status || copy.status.toLowerCase() === 'available') && !activeCopyIDs.has(copy.bookCopyID))
        .map(copy => ({
          id: copy.bookCopyID,
          label: `${data.books.find(book => book.bookID === copy.bookID)?.title ?? `Book #${copy.bookID}`} · ${data.branches.find(branch => branch.branchID === copy.branchID)?.name ?? `Branch #${copy.branchID}`} · Copy #${copy.copyNumber}`,
        })));
    });
  }

  setFilter(value: LoanFilter): void {
    this.filter.set(value);
  }

  openCheckout(): void {
    this.error.set('');
    this.message.set('');
    Object.assign(this.checkoutForm, { borrowerID: '', bookCopyID: null, dueDate: '' });
    this.checkoutOpen.set(true);
  }

  checkout(form: NgForm): void {
    if (form.invalid || this.checkoutForm.bookCopyID === null) {
      form.control.markAllAsTouched();
      return;
    }

    this.checkoutLoading.set(true);
    this.error.set('');
    this.http.post(`${environment.apiUrl}/loans/checkout`, {
      borrowerID: this.checkoutForm.borrowerID,
      bookCopyID: this.checkoutForm.bookCopyID,
      dueDate: this.checkoutForm.dueDate || null,
    }).pipe(finalize(() => this.checkoutLoading.set(false))).subscribe({
      next: () => {
        this.checkoutOpen.set(false);
        this.message.set('Loan created.');
        this.loadLoans();
        this.loadAvailableCopies();
      },
      error: error => this.error.set(this.errorMessage(error)),
    });
  }

  returnLoan(loan: LoanRecord): void {
    this.returningLoanID.set(loan.loanID);
    this.error.set('');
    this.http.post(`${environment.apiUrl}/loans/${loan.loanID}/return`, {}).pipe(
      finalize(() => this.returningLoanID.set(null)),
    ).subscribe({
      next: () => {
        this.message.set(`Returned “${loan.bookTitle}”.`);
        this.loadLoans();
        this.loadAvailableCopies();
      },
      error: error => this.error.set(this.errorMessage(error)),
    });
  }

  closeCheckout(): void {
    this.checkoutOpen.set(false);
  }

  private errorMessage(error: { error?: unknown }): string {
    if (typeof error.error === 'string') return error.error;
    return 'The loan request could not be completed. Verify the borrower ID and available copy.';
  }
}
