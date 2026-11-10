import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, timeout } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginRequest, LoginResponse, RegisterRequest } from '../models/auth.models';

export function getErrorMessage(error: unknown, fallback: string): string {
  const responseError = (error as { error?: unknown } | null)?.error ?? error;

  if (typeof responseError === 'string' && responseError.trim()) {
    return responseError;
  }

  if (responseError && typeof responseError === 'object') {
    const details = responseError as Record<string, unknown>;
    for (const key of ['message', 'error', 'title']) {
      if (typeof details[key] === 'string' && details[key]) {
        return details[key] as string;
      }
    }

    if (details['errors'] && typeof details['errors'] === 'object') {
      const messages = Object.values(details['errors'] as Record<string, unknown>)
        .flatMap(value => Array.isArray(value) ? value : [value])
        .filter((value): value is string => typeof value === 'string');
      if (messages.length) {
        return messages.join(' ');
      }
    }
  }

  return fallback;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'library_jwt';
  private readonly session = signal<LoginResponse | null>(this.readSession());
  readonly currentUser = computed(() => this.session());
  readonly isAuthenticated = computed(() => !!this.session()?.token);

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/Users/login`, request).pipe(
      timeout(3000),
      tap(response => {
        localStorage.setItem(this.tokenKey, JSON.stringify(response));
        this.session.set(response);
      }),
    );
  }

  register(request: RegisterRequest): Observable<unknown> {
    return this.http.post(`${environment.apiUrl}/Users/register`, request).pipe(
      timeout(3000),
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.session.set(null);
  }

  get token(): string | null {
    return this.session()?.token ?? null;
  }

  private readSession(): LoginResponse | null {
    const stored = localStorage.getItem(this.tokenKey);
    if (!stored) {
      return null;
    }

    try {
      const session = JSON.parse(stored) as LoginResponse;
      return session.token ? session : null;
    } catch {
      localStorage.removeItem(this.tokenKey);
      return null;
    }
  }
}
