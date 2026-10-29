import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-leave-types',
  templateUrl: './leave-types.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class LeaveTypesComponent implements OnInit {
  rows: any[] = [];
  error = '';
  constructor(private api: PortalApiService) {}
  ngOnInit(): void {
    this.api.getLeaveTypes().subscribe({
      next: (data) => (this.rows = data),
      error: () => (this.error = 'Unable to load leave types.')
    });
  }
}
