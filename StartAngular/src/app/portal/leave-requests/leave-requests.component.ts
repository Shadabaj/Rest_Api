import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-leave-requests',
  templateUrl: './leave-requests.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class LeaveRequestsComponent implements OnInit {
  rows: any[] = [];
  error = '';
  constructor(private api: PortalApiService) {}
  ngOnInit(): void {
    this.api.getLeaveRequests().subscribe({
      next: (data) => (this.rows = data),
      error: () => (this.error = 'Unable to load leave requests.')
    });
  }
}
