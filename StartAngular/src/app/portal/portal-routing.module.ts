import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../guards/auth.guard';
import { RoleGuard } from '../guards/role.guard';
import { Roles } from '../constants/roles';
import { PortalLayoutComponent } from './portal-layout/portal-layout.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { HolidaysComponent } from './holidays/holidays.component';
import { EmployeesComponent } from './employees/employees.component';
import { DepartmentsComponent } from './departments/departments.component';
import { DesignationsComponent } from './designations/designations.component';
import { LeaveTypesComponent } from './leave-types/leave-types.component';
import { LeaveRequestsComponent } from './leave-requests/leave-requests.component';
import { ShiftsComponent } from './shifts/shifts.component';
import { AttendanceComponent } from './attendance/attendance.component';
import { LocationsComponent } from './locations/locations.component';
import { RolesComponent } from './roles/roles.component';
import { MeHomeComponent } from './me/me-home.component';
import { MeAttendanceComponent } from './me/me-attendance.component';
import { MeLeaveComponent } from './me/me-leave.component';
import { PortalHomeRedirectComponent } from './portal-home-redirect.component';

const routes: Routes = [
  {
    path: '',
    component: PortalLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      // Decides Admin/HR/Manager → dashboard, Employee → /portal/me
      { path: '', component: PortalHomeRedirectComponent, pathMatch: 'full' },
      {
        path: 'dashboard',
        component: DashboardComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Admin, Roles.HR, Roles.Manager] }
      },
      {
        path: 'me',
        component: MeHomeComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Employee] }
      },
      {
        path: 'me/attendance',
        component: MeAttendanceComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Employee] }
      },
      {
        path: 'me/leave',
        component: MeLeaveComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Employee] }
      },
      {
        path: 'holidays',
        component: HolidaysComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Admin, Roles.HR, Roles.Manager, Roles.Employee] }
      },
      {
        path: 'employees',
        component: EmployeesComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Admin, Roles.HR] }
      },
      {
        path: 'departments',
        component: DepartmentsComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Admin, Roles.HR] }
      },
      {
        path: 'designations',
        component: DesignationsComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Admin, Roles.HR] }
      },
      {
        path: 'leave-types',
        component: LeaveTypesComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Admin, Roles.HR] }
      },
      {
        path: 'leave-requests',
        component: LeaveRequestsComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Admin, Roles.HR, Roles.Manager] }
      },
      {
        path: 'shifts',
        component: ShiftsComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Admin, Roles.HR] }
      },
      {
        path: 'attendance',
        component: AttendanceComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Admin, Roles.HR, Roles.Manager] }
      },
      {
        path: 'locations',
        component: LocationsComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Admin, Roles.HR] }
      },
      {
        path: 'roles',
        component: RolesComponent,
        canActivate: [RoleGuard],
        data: { roles: [Roles.Admin] }
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PortalRoutingModule {}
