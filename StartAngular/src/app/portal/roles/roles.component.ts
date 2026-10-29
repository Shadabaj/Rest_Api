import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-roles',
  templateUrl: './roles.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class RolesComponent implements OnInit {
  rows: any[] = [];
  error = '';
  constructor(private api: PortalApiService) {}
  ngOnInit(): void {
    this.api.getRoles().subscribe({
      next: (data) => (this.rows = data),
      error: () => (this.error = 'Unable to load roles.')
    });
  }
}
