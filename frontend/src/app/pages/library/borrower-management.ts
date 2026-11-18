import { NgFor, NgIf } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { finalize } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RegisterRequest } from '../../models/auth.models';

interface BorrowerRecord {
  userID: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  address: string;
  status: string;
  role: string;
}

@Component({
  imports: [FormsModule, NgFor, NgIf],
  selector: 'app-borrower-management',
  styleUrl: './borrower-management.css',
  templateUrl: './borrower-management.html',
})
export class BorrowerManagement {
  private readonly http = inject(HttpClient);
  readonly borrowers = signal<BorrowerRecord[]>([]);
  readonly search = signal('');
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly editorOpen = signal(false);
  readonly editingID = signal<string | null>(null);
  readonly error = signal('');
  readonly message = signal('');
  readonly form = { userID: '', fullName: '', email: '', phoneNumber: '', address: '', password: '', status: 'Active' };

  constructor() {
    this.loadBorrowers();
  }

  loadBorrowers(): void {
    this.loading.set(true);
    this.error.set('');
    this.http.get<BorrowerRecord[]>(`${environment.apiUrl}/borrowers`, {
      params: this.search().trim() ? { search: this.search().trim() } : {},
    }).pipe(finalize(() => this.loading.set(false))).subscribe({
      next: borrowers => this.borrowers.set(borrowers),
      error: () => this.error.set('Borrowers could not be loaded.'),
    });
  }

  setSearch(value: string): void {
    this.search.set(value);
    this.loadBorrowers();
  }

  openCreate(): void {
    this.editingID.set(null);
    Object.assign(this.form, { userID: '', fullName: '', email: '', phoneNumber: '', address: '', password: '', status: 'Active' });
    this.error.set('');
    this.message.set('');
    this.editorOpen.set(true);
  }

  openEdit(borrower: BorrowerRecord): void {
    this.editingID.set(borrower.userID);
    Object.assign(this.form, {
      userID: borrower.userID,
      fullName: borrower.fullName,
      email: borrower.email,
      phoneNumber: borrower.phoneNumber,
      address: borrower.address,
      password: '',
      status: borrower.status,
    });
    this.error.set('');
    this.message.set('');
    this.editorOpen.set(true);
  }

  save(form: NgForm): void {
    if (form.invalid) {
      form.control.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set('');
    const id = this.editingID();
    const request = id === null
      ? this.http.post(`${environment.apiUrl}/Users/register`, this.form as RegisterRequest)
      : this.http.put(`${environment.apiUrl}/borrowers/${encodeURIComponent(id)}`, {
        userID: id,
        fullName: this.form.fullName,
        email: this.form.email,
        phoneNumber: this.form.phoneNumber,
        address: this.form.address,
        status: this.form.status,
        role: this.borrowers().find(borrower => borrower.userID === id)?.role ?? 'Borrower',
      });
    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.editorOpen.set(false);
        this.message.set(id === null ? 'Borrower account created.' : 'Borrower updated.');
        this.loadBorrowers();
      },
      error: error => this.error.set(this.errorMessage(error)),
    });
  }

  delete(borrower: BorrowerRecord): void {
    if (!globalThis.confirm(`Delete borrower ${borrower.fullName}?`)) return;
    this.error.set('');
    this.http.delete(`${environment.apiUrl}/borrowers/${encodeURIComponent(borrower.userID)}`).subscribe({
      next: () => {
        this.message.set('Borrower deleted.');
        this.loadBorrowers();
      },
      error: error => this.error.set(this.errorMessage(error)),
    });
  }

  closeEditor(): void {
    this.editorOpen.set(false);
  }

  private errorMessage(error: { error?: unknown }): string {
    if (typeof error.error === 'string') return error.error;
    return 'The request could not be completed. Check the fields and try again.';
  }
}
