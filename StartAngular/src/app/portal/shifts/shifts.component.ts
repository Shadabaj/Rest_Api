import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-shifts',
  templateUrl: './shifts.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class ShiftsComponent implements OnInit {
  shifts: any[] = [];
  assignments: any[] = [];
  error = '';
  constructor(private api: PortalApiService) {}
  ngOnInit(): void {
    this.api.getShifts().subscribe({
      next: (data) => (this.shifts = data),
      error: () => (this.error = 'Unable to load shifts.')
    });
    this.api.getShiftAssignments().subscribe({
      next: (data) => (this.assignments = data),
      error: () => {}
    });
  }
}
