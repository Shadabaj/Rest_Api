import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../public/services/auth.service';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-holidays',
  templateUrl: './holidays.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class HolidaysComponent implements OnInit {
  rows: any[] = [];
  error = '';
  success = '';
  canSeed = false;

  constructor(private api: PortalApiService, private auth: AuthService) {}

  ngOnInit(): void {
    this.canSeed = this.auth.hasAnyRole(['Admin', 'HR']);
    this.reload();
  }

  reload(): void {
    this.api.getHolidays(new Date().getFullYear()).subscribe({
      next: (data) => (this.rows = data),
      error: () => (this.error = 'Unable to load holidays.')
    });
  }

  seedIndia(): void {
    this.api.seedIndiaHolidays().subscribe({
      next: (res) => {
        this.success = res?.message || 'India holidays seeded.';
        this.reload();
      },
      error: (err) => (this.error = err?.error?.message || 'Seed failed.')
    });
  }
}
