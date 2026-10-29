import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class DashboardComponent implements OnInit {
  summary: any = null;
  error = '';

  constructor(private api: PortalApiService) {}

  ngOnInit(): void {
    this.api.getDashboard().subscribe({
      next: (data) => (this.summary = data),
      error: () => (this.error = 'Unable to load dashboard.')
    });
  }
}
