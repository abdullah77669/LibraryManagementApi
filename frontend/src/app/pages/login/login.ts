import { Component, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { NgIf } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService, getErrorMessage } from '../../services/auth.service';
import { finalize } from 'rxjs';

@Component({
  imports: [FormsModule, RouterLink, NgIf],
  selector: 'app-login',
  styleUrl: './login.css',
  templateUrl: './login.html',
})
export class Login {
  userID = '';
  password = '';
  error = signal('');
  showError = signal(false);
  loading = signal(false);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  submit(form: NgForm): void {
    this.error.set('');
    this.showError.set(false);
    if (form.invalid) {
      form.control.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.auth.login({ userID: this.userID, password: this.password }).pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: response => {
        const path = response.role?.toLowerCase() === 'borrower' ? '/borrower' : '/library';
        this.router.navigate([path]);
      },
      error: error => {
        this.loading.set(false);
        this.error.set(getErrorMessage(error, 'Incorrect User ID or password.'));
        this.showError.set(true);
      },
    });
  }

  closeError(): void {
    this.showError.set(false);
    this.error.set('');
  }
}
