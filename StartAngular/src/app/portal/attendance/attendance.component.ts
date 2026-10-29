import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-attendance',
  templateUrl: './attendance.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class AttendanceComponent implements OnInit {
  daily: any[] = [];
  punches: any[] = [];
  adjustments: any[] = [];
  error = '';
  constructor(private api: PortalApiService) {}
  ngOnInit(): void {
    this.api.getAttendanceDaily().subscribe({
      next: (data) => (this.daily = data),
      error: () => (this.error = 'Unable to load attendance.')
    });
    this.api.getAttendancePunches().subscribe({ next: (data) => (this.punches = data) });
    this.api.getAttendanceAdjustments().subscribe({ next: (data) => (this.adjustments = data) });
  }
}
