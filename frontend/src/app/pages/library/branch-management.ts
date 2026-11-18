import { NgFor, NgIf } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { finalize } from 'rxjs';
import { environment } from '../../../environments/environment';

interface BranchRecord { branchID: number; name: string; address: string | null; contactNumber: string | null; }

@Component({
  imports: [FormsModule, NgFor, NgIf],
  selector: 'app-branch-management',
  styleUrl: './branch-management.css',
  templateUrl: './branch-management.html',
})
export class BranchManagement {
  private readonly http = inject(HttpClient);
  readonly branches = signal<BranchRecord[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly editorOpen = signal(false);
  readonly editingID = signal<number | null>(null);
  readonly error = signal('');
  readonly message = signal('');
  readonly form = { name: '', address: '', contactNumber: '' };

  constructor() { this.loadBranches(); }

  loadBranches(): void {
    this.loading.set(true);
    this.error.set('');
    this.http.get<BranchRecord[]>(`${environment.apiUrl}/branches`).pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: branches => this.branches.set(branches),
      error: () => this.error.set('Branches could not be loaded.'),
    });
  }

  openCreate(): void {
    this.editingID.set(null);
    Object.assign(this.form, { name: '', address: '', contactNumber: '' });
    this.error.set('');
    this.message.set('');
    this.editorOpen.set(true);
  }

  openEdit(branch: BranchRecord): void {
    this.editingID.set(branch.branchID);
    Object.assign(this.form, { name: branch.name, address: branch.address ?? '', contactNumber: branch.contactNumber ?? '' });
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
      ? this.http.post(`${environment.apiUrl}/branches`, this.form)
      : this.http.put(`${environment.apiUrl}/branches/${id}`, { branchID: id, ...this.form });
    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.editorOpen.set(false);
        this.message.set(id === null ? 'Branch created.' : 'Branch updated.');
        this.loadBranches();
      },
      error: error => this.error.set(this.errorMessage(error)),
    });
  }

  delete(branch: BranchRecord): void {
    if (!globalThis.confirm(`Delete branch ${branch.name}?`)) return;
    this.error.set('');
    this.http.delete(`${environment.apiUrl}/branches/${branch.branchID}`).subscribe({
      next: () => {
        this.message.set('Branch deleted.');
        this.loadBranches();
      },
      error: error => this.error.set(this.errorMessage(error)),
    });
  }

  closeEditor(): void { this.editorOpen.set(false); }

  private errorMessage(error: { error?: unknown }): string {
    if (typeof error.error === 'string') return error.error;
    return 'The branch request could not be completed.';
  }
}
