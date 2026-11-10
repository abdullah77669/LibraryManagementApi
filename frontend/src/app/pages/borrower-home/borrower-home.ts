import { NgFor, NgIf } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../services/auth.service';

interface BookDetails {
  bookID: number;
  title: string;
  authorName: string;
  categoryName: string;
  language: string;
  availableCopies: number;
}

@Component({
  imports: [NgFor, NgIf, RouterLink],
  selector: 'app-borrower-home',
  styleUrl: './borrower-home.css',
  templateUrl: './borrower-home.html',
})
export class BorrowerHome {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly books = signal<BookDetails[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly search = signal('');
  readonly selectedCategory = signal('All books');
  readonly userID = this.auth.currentUser()?.userID ?? 'Reader';
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

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
