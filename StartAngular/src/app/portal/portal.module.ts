import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PortalRoutingModule } from './portal-routing.module';
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
import { TripsComponent } from './trips/trips.component';
import { PortalHomeRedirectComponent } from './portal-home-redirect.component';

@NgModule({
  declarations: [
    PortalLayoutComponent,
    PortalHomeRedirectComponent,
    DashboardComponent,
    HolidaysComponent,
    EmployeesComponent,
    DepartmentsComponent,
    DesignationsComponent,
    LeaveTypesComponent,
    LeaveRequestsComponent,
    ShiftsComponent,
    AttendanceComponent,
    LocationsComponent,
    RolesComponent,
    MeHomeComponent,
    MeAttendanceComponent,
    MeLeaveComponent,
    TripsComponent
  ],
  imports: [CommonModule, FormsModule, RouterModule, PortalRoutingModule]
})
export class PortalModule {}
