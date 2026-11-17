import { NgFor, NgIf } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService, getErrorMessage } from '../../services/auth.service';
import { canAccessManagementArea } from '../../models/staff-permissions';

interface BookDetails {
  bookID: number;
  title: string;
  imageUrl: string | null;
  authorName: string;
  categoryName: string;
  language: string;
  availableCopies: number;
}

interface BookCopy {
  bookCopyID: number;
  status: string | null;
}

@Component({
  imports: [FormsModule, NgFor, NgIf, RouterLink],
  selector: 'app-borrower-home',
  styleUrl: './borrower-home.css',
  templateUrl: './borrower-home.html',
})
export class BorrowerHome {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly books = signal<BookDetails[]>([]);
  readonly failedImages = signal(new Set<number>());
  readonly loading = signal(true);
  readonly error = signal('');
  readonly search = signal('');
  readonly selectedCategory = signal('All books');
  readonly userID = computed(() => this.auth.currentUser()?.userID ?? null);
  readonly isStaff = computed(() => canAccessManagementArea(this.auth.currentUser()?.role, 'dashboard'));
  readonly borrowPromptBook = signal<BookDetails | null>(null);
  readonly borrowingBookID = signal<number | null>(null);
  readonly borrowMessage = signal('');
  readonly borrowError = signal(false);
  readonly signupOpen = signal(false);
  readonly signupError = signal('');
  readonly signupSuccess = signal('');
  readonly signupLoading = signal(false);
  readonly signupForm = { userID: '', fullName: '', email: '', phoneNumber: '', address: '', password: '' };
  readonly categories = computed(() => [
    'All books',
    ...new Set(this.books().map(book => book.categoryName).filter(Boolean)),
  ]);
  readonly filteredBooks = computed(() => {
    const query = this.search().trim().toLowerCase();
    const category = this.selectedCategory();

    return this.books().filter(book => {
      const matchesCategory = category === 'All books' || book.categoryName === category;
      const matchesSearch = !query || [book.title, book.authorName, book.categoryName]
        .some(value => value?.toLowerCase().includes(query));
      return matchesCategory && matchesSearch;
    });
  });

  constructor() {
    this.loadBooks();
  }

  loadBooks(): void {
    this.loading.set(true);
    this.error.set('');
    this.http.get<BookDetails[]>(`${environment.apiUrl}/book-details`).pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: books => this.books.set(books),
      error: () => this.error.set('The catalog could not be loaded. Please try again.'),
    });
  }

  setSearch(value: string): void {
    this.search.set(value);
  }

  chooseCategory(category: string): void {
    this.selectedCategory.set(category);
  }

  shouldShowCoverImage(book: BookDetails): boolean {
    return !!book.imageUrl && !this.failedImages().has(book.bookID);
  }

  onCoverImageError(bookID: number): void {
    this.failedImages.update(failed => new Set(failed).add(bookID));
  }

  borrowBook(book: BookDetails): void {
    const userID = this.userID();
    if (!userID) {
      this.borrowPromptBook.set(book);
      return;
    }

    this.borrowMessage.set('');
    this.borrowError.set(false);
    this.borrowingBookID.set(book.bookID);
    this.http.get<BookCopy[]>(`${environment.apiUrl}/bookcopies`, {
      params: { bookId: book.bookID },
    }).subscribe({
      next: copies => {
        const availableCopy = copies.find(copy => !copy.status || copy.status.toLowerCase() === 'available');
        if (!availableCopy) {
          this.borrowError.set(true);
          this.borrowMessage.set('This book is no longer available. Please refresh the catalog and try again.');
          this.borrowingBookID.set(null);
          return;
        }

        this.http.post(`${environment.apiUrl}/loans/checkout`, {
          borrowerID: userID,
          bookCopyID: availableCopy.bookCopyID,
        }).pipe(
          finalize(() => this.borrowingBookID.set(null)),
        ).subscribe({
          next: () => {
            this.borrowMessage.set(`You borrowed “${book.title}”.`);
            this.loadBooks();
          },
          error: () => {
            this.borrowError.set(true);
            this.borrowMessage.set('The book could not be borrowed. It may no longer be available.');
          },
        });
      },
      error: () => {
        this.borrowError.set(true);
        this.borrowMessage.set('Could not check book availability. Please try again.');
        this.borrowingBookID.set(null);
      },
    });
  }

  closeBorrowPrompt(): void {
    this.borrowPromptBook.set(null);
  }

  goToSignIn(): void {
    this.borrowPromptBook.set(null);
    this.router.navigate(['/login']);
  }

  openMyBooksSignup(event: Event): void {
    event.preventDefault();
    this.signupError.set('');
    this.signupSuccess.set('');
    this.signupOpen.set(true);
  }

  openMyBooks(event: Event): void {
    event.preventDefault();
    if (this.userID()) {
      this.router.navigate(['/my-books']);
      return;
    }

    this.openMyBooksSignup(event);
  }

  closeSignup(): void {
    this.signupOpen.set(false);
    this.signupError.set('');
    this.signupSuccess.set('');
  }

  submitSignup(form: NgForm): void {
    this.signupError.set('');
    this.signupSuccess.set('');
    if (form.invalid) {
      form.control.markAllAsTouched();
      return;
    }

    this.signupLoading.set(true);
    this.auth.register(this.signupForm).pipe(
      finalize(() => this.signupLoading.set(false)),
    ).subscribe({
      next: () => this.signupSuccess.set('Account created. You can now sign in.'),
      error: error => this.signupError.set(getErrorMessage(error, 'Could not create your account. Please try again.')),
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
