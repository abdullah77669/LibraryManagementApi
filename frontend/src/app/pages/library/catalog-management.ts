import { NgFor, NgIf } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

 type CatalogKey = 'books' | 'authors' | 'categories' | 'publishers' | 'copies';
 type FieldType = 'text' | 'number' | 'date' | 'email' | 'url' | 'textarea' | 'select';
 type LookupKey = 'authors' | 'categories' | 'publishers' | 'books' | 'branches' | 'statuses';
 type FormValue = string | number | null;

interface CatalogRecord { [key: string]: unknown; }
interface SelectOption { value: number | string; label: string; }
interface EditorField { name: string; label: string; type: FieldType; required?: boolean; maxLength?: number; lookup?: LookupKey; }
interface ResourceConfig { label: string; singular: string; endpoint: string; idField: string; titleField: string; subtitleField?: string; fields: EditorField[]; }

const resourceConfigs: Record<CatalogKey, ResourceConfig> = {
  books: {
    label: 'Books', singular: 'Book', endpoint: '/Book', idField: 'bookID', titleField: 'title', subtitleField: 'isbn',
    fields: [
      { name: 'title', label: 'Title', type: 'text', required: true, maxLength: 255 },
      { name: 'isbn', label: 'ISBN', type: 'text', required: true, maxLength: 50 },
      { name: 'authorID', label: 'Author', type: 'select', required: true, lookup: 'authors' },
      { name: 'categoryID', label: 'Category', type: 'select', required: true, lookup: 'categories' },
      { name: 'publisherID', label: 'Publisher', type: 'select', required: true, lookup: 'publishers' },
      { name: 'language', label: 'Language', type: 'text', required: true, maxLength: 50 },
      { name: 'publishDate', label: 'Publish date', type: 'date', required: true },
      { name: 'imageUrl', label: 'Cover image URL', type: 'url', maxLength: 2048 },
      { name: 'excerpts', label: 'Description / excerpts', type: 'textarea', required: true, maxLength: 2000 },
    ],
  },
  authors: {
    label: 'Authors', singular: 'Author', endpoint: '/authors', idField: 'authorID', titleField: 'name', subtitleField: 'dateOfBirth',
    fields: [
      { name: 'name', label: 'Name', type: 'text', required: true, maxLength: 255 },
      { name: 'dateOfBirth', label: 'Date of birth', type: 'date', required: true },
      { name: 'biography', label: 'Biography', type: 'textarea' },
    ],
  },
  categories: {
    label: 'Categories', singular: 'Category', endpoint: '/categories', idField: 'bookCategoryID', titleField: 'categoryName',
    fields: [{ name: 'categoryName', label: 'Category name', type: 'text', required: true, maxLength: 255 }],
  },
  publishers: {
    label: 'Publishers', singular: 'Publisher', endpoint: '/publishers', idField: 'publisherID', titleField: 'name', subtitleField: 'email',
    fields: [
      { name: 'name', label: 'Name', type: 'text', required: true, maxLength: 255 },
      { name: 'address', label: 'Address', type: 'textarea', required: true },
      { name: 'contactNumber', label: 'Contact number', type: 'text', required: true, maxLength: 15 },
      { name: 'email', label: 'Email', type: 'email', required: true, maxLength: 255 },
    ],
  },
  copies: {
    label: 'Book copies', singular: 'Book copy', endpoint: '/bookcopies', idField: 'bookCopyID', titleField: 'bookID', subtitleField: 'status',
    fields: [
      { name: 'bookID', label: 'Book', type: 'select', required: true, lookup: 'books' },
      { name: 'branchID', label: 'Branch', type: 'select', required: true, lookup: 'branches' },
      { name: 'copyNumber', label: 'Copy number', type: 'number', required: true },
      { name: 'status', label: 'Status', type: 'select', required: true, lookup: 'statuses' },
    ],
  },
};

@Component({
  imports: [FormsModule, NgFor, NgIf],
  selector: 'app-catalog-management',
  styleUrl: './catalog-management.css',
  templateUrl: './catalog-management.html',
})
export class CatalogManagement {
  private readonly http = inject(HttpClient);
  readonly resources: { key: CatalogKey; label: string }[] = [
    { key: 'books', label: 'Books' },
    { key: 'authors', label: 'Authors' },
    { key: 'categories', label: 'Categories' },
    { key: 'publishers', label: 'Publishers' },
    { key: 'copies', label: 'Copies' },
  ];
  readonly selectedResource = signal<CatalogKey>('books');
  readonly records = signal<CatalogRecord[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly message = signal('');
  readonly editorOpen = signal(false);
  readonly editingID = signal<number | null>(null);
  readonly form = signal<Record<string, FormValue>>({});
  readonly lookups = signal<Record<LookupKey, SelectOption[]>>({
    authors: [], categories: [], publishers: [], books: [], branches: [], statuses: [
      { value: 'Available', label: 'Available' }, { value: 'Loaned', label: 'Loaned' },
    ],
  });
  readonly currentConfig = computed(() => resourceConfigs[this.selectedResource()]!);
  readonly currentFields = computed(() => this.currentConfig().fields);
  readonly rows = computed(() => {
    const config = this.currentConfig();
    const resource = this.selectedResource();
    const lookupData = this.lookups();
    return this.records().map(record => {
      const bookID = Number(record['bookID']);
      const branchID = Number(record['branchID']);
      const title = resource === 'copies'
        ? lookupData.books.find(option => option.value === bookID)?.label ?? `Book #${bookID}`
        : String(record[config.titleField] ?? '—');
      const subtitle = resource === 'copies'
        ? `Copy #${record['copyNumber']} · ${lookupData.branches.find(option => option.value === branchID)?.label ?? `Branch #${branchID}`} · ${String(record['status'] ?? 'Available')}`
        : config.subtitleField ? String(record[config.subtitleField] ?? '') : '';
      return { id: record[config.idField] as number, title, subtitle, record };
    });
  });

  constructor() {
    this.loadLookups();
    this.loadRecords();
  }

  selectResource(key: CatalogKey): void {
    this.selectedResource.set(key);
    this.editorOpen.set(false);
    this.message.set('');
    this.loadRecords();
  }

  loadRecords(): void {
    this.loading.set(true);
    this.error.set('');
    this.http.get<CatalogRecord[]>(`${environment.apiUrl}${this.currentConfig().endpoint}`).pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: records => this.records.set(records),
      error: () => this.error.set(`Could not load ${this.currentConfig().label.toLowerCase()}.`),
    });
  }

  openCreate(): void {
    this.editingID.set(null);
    this.form.set(this.createEmptyForm());
    this.message.set('');
    this.editorOpen.set(true);
  }

  openEdit(record: CatalogRecord): void {
    const values: Record<string, FormValue> = {};
    for (const field of this.currentFields()) {
      const value = record[field.name];
      values[field.name] = field.type === 'date' && value ? String(value).slice(0, 10) : (value as FormValue ?? '');
    }
    this.editingID.set(Number(record[this.currentConfig().idField]));
    this.form.set(values);
    this.message.set('');
    this.editorOpen.set(true);
  }

  updateField(name: string, value: FormValue): void {
    this.form.update(current => ({ ...current, [name]: value }));
  }

  options(field: EditorField): SelectOption[] {
    return field.lookup ? this.lookups()[field.lookup] : [];
  }

  save(): void {
    const config = this.currentConfig();
    const payload: Record<string, FormValue> = {};
    for (const field of config.fields) {
      const value = this.form()[field.name];
      payload[field.name] = field.type === 'number' && value !== null && value !== '' ? Number(value) : value;
    }

    this.saving.set(true);
    this.error.set('');
    const id = this.editingID();
    const request = id === null
      ? this.http.post(`${environment.apiUrl}${config.endpoint}`, payload)
      : this.http.put(`${environment.apiUrl}${config.endpoint}/${id}`, payload);
    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.editorOpen.set(false);
        this.message.set(`${config.singular} ${id === null ? 'created' : 'updated'}.`);
        this.loadRecords();
        this.loadLookups();
      },
      error: error => this.error.set(this.errorMessage(error)),
    });
  }

  delete(record: CatalogRecord): void {
    const id = Number(record[this.currentConfig().idField]);
    if (!globalThis.confirm(`Delete this ${this.currentConfig().singular.toLowerCase()}?`)) return;

    this.error.set('');
    this.http.delete(`${environment.apiUrl}${this.currentConfig().endpoint}/${id}`).subscribe({
      next: () => {
        this.message.set('Record deleted.');
        this.loadRecords();
        this.loadLookups();
      },
      error: error => this.error.set(this.errorMessage(error)),
    });
  }

  closeEditor(): void {
    this.editorOpen.set(false);
  }

  private createEmptyForm(): Record<string, FormValue> {
    return Object.fromEntries(this.currentFields().map(field => [
      field.name,
      field.name === 'status' ? 'Available' : field.type === 'select' || field.type === 'number' ? null : '',
    ]));
  }

  private loadLookups(): void {
    forkJoin({
      authors: this.http.get<CatalogRecord[]>(`${environment.apiUrl}/authors`).pipe(catchError(() => of([]))),
      categories: this.http.get<CatalogRecord[]>(`${environment.apiUrl}/categories`).pipe(catchError(() => of([]))),
      publishers: this.http.get<CatalogRecord[]>(`${environment.apiUrl}/publishers`).pipe(catchError(() => of([]))),
      books: this.http.get<CatalogRecord[]>(`${environment.apiUrl}/Book`).pipe(catchError(() => of([]))),
      branches: this.http.get<CatalogRecord[]>(`${environment.apiUrl}/branches`).pipe(catchError(() => of([]))),
    }).subscribe(data => this.lookups.set({
      ...this.lookups(),
      authors: data.authors.map(item => ({ value: Number(item['authorID']), label: String(item['name']) })),
      categories: data.categories.map(item => ({ value: Number(item['bookCategoryID']), label: String(item['categoryName']) })),
      publishers: data.publishers.map(item => ({ value: Number(item['publisherID']), label: String(item['name']) })),
      books: data.books.map(item => ({ value: Number(item['bookID']), label: String(item['title']) })),
      branches: data.branches.map(item => ({ value: Number(item['branchID']), label: String(item['name']) })),
    }));
  }

  private errorMessage(error: { error?: unknown }): string {
    if (typeof error.error === 'string') return error.error;
    return 'The request could not be completed. Check required fields and try again.';
  }
}
