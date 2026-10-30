import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-me-leave',
  templateUrl: './me-leave.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class MeLeaveComponent implements OnInit {
  rows: any[] = [];
  leaveTypes: any[] = [];
  error = '';
  success = '';
  saving = false;
  form = {
    leaveTypeId: null as number | null,
    startDate: '',
    endDate: '',
    isHalfDay: false,
    reason: ''
  };

  constructor(private api: PortalApiService) {}

  ngOnInit(): void {
    this.reload();
    this.api.getLeaveTypes().subscribe({ next: (d) => (this.leaveTypes = d) });
  }

  reload(): void {
    this.api.getMyLeaves().subscribe({
      next: (data) => (this.rows = data),
      error: () => (this.error = 'Unable to load your leave requests.')
    });
  }

  submit(): void {
    this.error = '';
    this.success = '';
    if (!this.form.leaveTypeId || !this.form.startDate || !this.form.endDate) {
      this.error = 'Leave type and dates are required.';
      return;
    }
    this.saving = true;
    this.api.submitLeave(this.form).subscribe({
      next: (res) => {
        this.saving = false;
        this.success = res?.message || 'Leave submitted.';
        this.form = { leaveTypeId: null, startDate: '', endDate: '', isHalfDay: false, reason: '' };
        this.reload();
      },
      error: (err) => {
        this.saving = false;
        this.error = err?.error?.message || 'Unable to submit leave.';
      }
    });
  }
}
