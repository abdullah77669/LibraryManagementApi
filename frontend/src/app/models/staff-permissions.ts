export type StaffRole = 'Admin' | 'Librarian';
export type ManagementArea = 'dashboard' | 'catalog' | 'loans' | 'borrowers' | 'branches';

const permissions: Record<ManagementArea, readonly StaffRole[]> = {
  dashboard: ['Admin', 'Librarian'],
  catalog: ['Admin', 'Librarian'],
  loans: ['Admin', 'Librarian'],
  borrowers: ['Admin'],
  branches: ['Admin'],
};

export function canAccessManagementArea(role: string | undefined, area: ManagementArea): boolean {
  return !!role && permissions[area].some(allowedRole => allowedRole.toLowerCase() === role.toLowerCase());
}
