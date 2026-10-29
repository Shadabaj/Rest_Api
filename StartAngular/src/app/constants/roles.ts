export const Roles = {
  Admin: 'Admin',
  HR: 'HR',
  Manager: 'Manager',
  Employee: 'Employee'
} as const;

export type AppRole = (typeof Roles)[keyof typeof Roles];

/** Priority when a user has multiple roles (highest first). */
export const ROLE_PRIORITY: AppRole[] = [
  Roles.Admin,
  Roles.HR,
  Roles.Manager,
  Roles.Employee
];

/**
 * Where each role lands after login.
 * Admin  → /portal            (full admin dashboard)
 * HR     → /portal            (HR operations dashboard)
 * Manager→ /portal            (team approvals dashboard)
 * Employee→ /portal/me        (self-service workspace)
 */
export const ROLE_HOME_PATH: Record<AppRole, string> = {
  [Roles.Admin]: '/portal/dashboard',
  [Roles.HR]: '/portal/dashboard',
  [Roles.Manager]: '/portal/dashboard',
  [Roles.Employee]: '/portal/me'
};

export interface NavItem {
  label: string;
  path: string;
  roles: AppRole[];
  exact?: boolean;
}

/** Sidebar items and which roles can see them */
export const PORTAL_NAV: NavItem[] = [
  { label: 'Dashboard', path: '/portal/dashboard', roles: [Roles.Admin, Roles.HR, Roles.Manager], exact: true },
  { label: 'My workspace', path: '/portal/me', roles: [Roles.Employee], exact: true },
  { label: 'My attendance', path: '/portal/me/attendance', roles: [Roles.Employee] },
  { label: 'My leave', path: '/portal/me/leave', roles: [Roles.Employee] },
  { label: 'Holidays', path: '/portal/holidays', roles: [Roles.Admin, Roles.HR, Roles.Manager, Roles.Employee] },
  { label: 'Employees', path: '/portal/employees', roles: [Roles.Admin, Roles.HR] },
  { label: 'Departments', path: '/portal/departments', roles: [Roles.Admin, Roles.HR] },
  { label: 'Designations', path: '/portal/designations', roles: [Roles.Admin, Roles.HR] },
  { label: 'Leave types', path: '/portal/leave-types', roles: [Roles.Admin, Roles.HR] },
  { label: 'Leave requests', path: '/portal/leave-requests', roles: [Roles.Admin, Roles.HR, Roles.Manager] },
  { label: 'Shifts', path: '/portal/shifts', roles: [Roles.Admin, Roles.HR] },
  { label: 'Attendance', path: '/portal/attendance', roles: [Roles.Admin, Roles.HR, Roles.Manager] },
  { label: 'Locations', path: '/portal/locations', roles: [Roles.Admin, Roles.HR] },
  { label: 'Roles', path: '/portal/roles', roles: [Roles.Admin] }
];

/** Resolve primary role from assigned roles using ROLE_PRIORITY. */
export function getPrimaryRole(userRoles: string[] | null | undefined): AppRole | null {
  const normalized = (userRoles || []).map((r) => r?.trim().toLowerCase());
  for (const role of ROLE_PRIORITY) {
    if (normalized.includes(role.toLowerCase())) {
      return role;
    }
  }
  return null;
}

/** Landing path for the user's highest-priority role. */
export function getHomePathForRoles(userRoles: string[] | null | undefined): string {
  const primary = getPrimaryRole(userRoles);
  return primary ? ROLE_HOME_PATH[primary] : '/login';
}

/** Whether a role list is allowed to open a portal path (from PORTAL_NAV / route data). */
export function canAccessPath(userRoles: string[] | null | undefined, allowedRoles: string[]): boolean {
  if (!allowedRoles?.length) {
    return true;
  }
  const normalized = (userRoles || []).map((r) => r?.trim().toLowerCase());
  return allowedRoles.some((role) => normalized.includes(role.toLowerCase()));
}
