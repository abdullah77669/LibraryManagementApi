import { Component, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { NgIf } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService, getErrorMessage } from '../../services/auth.service';
import { finalize } from 'rxjs';

@Component({
  imports: [FormsModule, RouterLink, NgIf],
  selector: 'app-register',
  styleUrl: './register.css',
  templateUrl: './register.html',
})
export class Register {
  form = { userID: '', fullName: '', email: '', phoneNumber: '', address: '', password: '' };
  error = signal('');
  success = signal('');
  showError = signal(false);
  loading = signal(false);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  submit(form: NgForm): void {
    this.error.set('');
    this.success.set('');
    this.showError.set(false);
    if (form.invalid) {
      form.control.markAllAsTouched();
      this.error.set(form.controls['email']?.invalid
        ? 'Please enter a valid email address, such as name@example.com.'
        : 'Please complete all required fields and correct the highlighted errors.');
      this.showError.set(true);
      return;
    }

    this.loading.set(true);
    this.auth.register(this.form).pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: () => {
        this.success.set('Registration successful. You can now log in.');
        setTimeout(() => this.router.navigate(['/login']), 800);
      },
      error: error => {
        this.loading.set(false);
        this.error.set(getErrorMessage(error, 'Registration failed.'));
        this.showError.set(true);
      },
    });
  }

  closeError(): void {
    this.showError.set(false);
    this.error.set('');
  }
}
