import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-me-leave',
  templateUrl: './me-leave.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class MeLeaveComponent implements OnInit {
  rows: any[] = [];
  error = '';

  constructor(private api: PortalApiService) {}

  ngOnInit(): void {
    this.api.getMyLeaves().subscribe({
      next: (data) => (this.rows = data),
      error: () => (this.error = 'Unable to load your leave requests.')
    });
  }
}
