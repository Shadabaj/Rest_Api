import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-employees',
  templateUrl: './employees.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class EmployeesComponent implements OnInit {
  rows: any[] = [];
  error = '';
  constructor(private api: PortalApiService) {}
  ngOnInit(): void {
    this.api.getEmployees().subscribe({
      next: (data) => (this.rows = data),
      error: () => (this.error = 'Unable to load employees.')
    });
  }
}
