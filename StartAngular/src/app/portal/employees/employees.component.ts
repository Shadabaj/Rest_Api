import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-employees',
  templateUrl: './employees.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class EmployeesComponent implements OnInit {
  rows: any[] = [];
  roles: any[] = [];
  countries: any[] = [];
  states: any[] = [];
  cities: any[] = [];
  departments: any[] = [];
  designations: any[] = [];
  workLocations: any[] = [];
  managers: any[] = [];
  error = '';
  success = '';
  showForm = false;
  saving = false;

  form: any = this.emptyForm();

  constructor(private api: PortalApiService) {}

  ngOnInit(): void {
    this.reload();
    this.api.getRoles().subscribe({ next: (d) => (this.roles = d) });
    this.api.getCountries().subscribe({ next: (d) => (this.countries = d) });
    this.api.getDepartments().subscribe({ next: (d) => (this.departments = d) });
    this.api.getDesignations().subscribe({ next: (d) => (this.designations = d) });
    this.api.getWorkLocations().subscribe({ next: (d) => (this.workLocations = d) });
  }

  emptyForm() {
    return {
      firstName: '',
      lastName: '',
      emailAddress: '',
      mobileNumber: '',
      password: 'Welcome@123',
      roleId: null as number | null,
      countryId: null as number | null,
      stateId: null as number | null,
      cityId: null as number | null,
      departmentId: null as number | null,
      designationId: null as number | null,
      workLocationId: null as number | null,
      reportingManagerId: null as number | null,
      dateOfBirth: '1995-01-01',
      dateOfJoinee: new Date().toISOString().slice(0, 10),
      isActive: true
    };
  }

  reload(): void {
    this.api.getEmployees().subscribe({
      next: (data) => {
        this.rows = data;
        this.managers = data;
      },
      error: () => (this.error = 'Unable to load employees.')
    });
  }

  onCountryChange(): void {
    this.form.stateId = null;
    this.form.cityId = null;
    this.cities = [];
    if (!this.form.countryId) {
      this.states = [];
      return;
    }
    this.api.getStates(this.form.countryId).subscribe({ next: (d) => (this.states = d) });
  }

  onStateChange(): void {
    this.form.cityId = null;
    if (!this.form.stateId) {
      this.cities = [];
      return;
    }
    this.api.getCities(this.form.stateId).subscribe({ next: (d) => (this.cities = d) });
  }

  openForm(): void {
    this.form = this.emptyForm();
    this.showForm = true;
    this.success = '';
    this.error = '';
  }

  save(): void {
    this.error = '';
    this.success = '';
    if (!this.form.firstName || !this.form.lastName || !this.form.emailAddress || !this.form.roleId || !this.form.password) {
      this.error = 'First name, last name, email, role and password are required.';
      return;
    }
    this.saving = true;
    this.api.provisionEmployee(this.form).subscribe({
      next: (res) => {
        this.saving = false;
        this.showForm = false;
        this.success = res?.message || 'Employee created.';
        this.reload();
      },
      error: (err) => {
        this.saving = false;
        this.error = err?.error?.message || 'Unable to create employee.';
      }
    });
  }

  remove(row: any): void {
    if (!confirm(`Deactivate ${row.employeeCode}?`)) return;
    this.api.deleteEmployee(row.rowId).subscribe({
      next: () => this.reload(),
      error: () => (this.error = 'Unable to delete employee.')
    });
  }
}
