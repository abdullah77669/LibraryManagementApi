import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { JsonPipe, NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../services/auth.service';

@Component({
  imports: [JsonPipe, NgIf],
  selector: 'app-library',
  styleUrl: './library.css',
  templateUrl: './library.html',
})
export class Library {
  books: unknown = null;
  error = '';
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  constructor() {
    this.http.get(`${environment.apiUrl}/Book`).subscribe({
      next: books => this.books = books,
      error: error => this.error = `${error.status}: Unable to load books.`,
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
