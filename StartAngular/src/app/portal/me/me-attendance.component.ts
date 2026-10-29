import { Component, OnDestroy, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-me-attendance',
  templateUrl: './me-attendance.component.html',
  styleUrls: ['../shared/portal-table.css', './me-attendance.component.css']
})
export class MeAttendanceComponent implements OnInit, OnDestroy {
  daily: any[] = [];
  punches: any[] = [];
  isClockedIn = false;
  lastPunchAt: string | null = null;
  nowLabel = '';
  loading = false;
  actionLoading = false;
  error = '';
  success = '';
  private timerId: any;

  constructor(private api: PortalApiService) {}

  ngOnInit(): void {
    this.tick();
    this.timerId = setInterval(() => this.tick(), 1000);
    this.refresh();
  }

  ngOnDestroy(): void {
    if (this.timerId) {
      clearInterval(this.timerId);
    }
  }

  refresh(): void {
    this.loading = true;
    this.error = '';

    this.api.getClockStatus().subscribe({
      next: (status) => {
        this.isClockedIn = !!status.isClockedIn;
        this.lastPunchAt = status.lastPunchAt || null;
      },
      error: () => (this.error = 'Unable to load clock status.')
    });

    this.api.getMyAttendance().subscribe({
      next: (data) => (this.daily = data),
      error: () => (this.error = 'Unable to load your attendance.'),
      complete: () => (this.loading = false)
    });

    this.api.getMyPunches().subscribe({ next: (data) => (this.punches = data) });
  }

  clockIn(): void {
    this.actionLoading = true;
    this.error = '';
    this.success = '';
    this.api.clockIn().subscribe({
      next: (res) => {
        this.actionLoading = false;
        this.success = res?.message || 'Clocked in.';
        this.refresh();
      },
      error: (err) => {
        this.actionLoading = false;
        this.error = err?.error?.message || 'Clock in failed.';
      }
    });
  }

  clockOut(): void {
    this.actionLoading = true;
    this.error = '';
    this.success = '';
    this.api.clockOut().subscribe({
      next: (res) => {
        this.actionLoading = false;
        this.success = res?.message || 'Clocked out.';
        this.refresh();
      },
      error: (err) => {
        this.actionLoading = false;
        this.error = err?.error?.message || 'Clock out failed.';
      }
    });
  }

  private tick(): void {
    this.nowLabel = new Date().toLocaleString();
  }
}
