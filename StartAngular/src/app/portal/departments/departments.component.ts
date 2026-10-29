import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-departments',
  templateUrl: './departments.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class DepartmentsComponent implements OnInit {
  rows: any[] = [];
  error = '';
  constructor(private api: PortalApiService) {}
  ngOnInit(): void {
    this.api.getDepartments().subscribe({
      next: (data) => (this.rows = data),
      error: () => (this.error = 'Unable to load departments.')
    });
  }
}
