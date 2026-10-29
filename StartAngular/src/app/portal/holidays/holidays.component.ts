import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-holidays',
  templateUrl: './holidays.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class HolidaysComponent implements OnInit {
  rows: any[] = [];
  error = '';

  constructor(private api: PortalApiService) {}

  ngOnInit(): void {
    this.api.getHolidays(new Date().getFullYear()).subscribe({
      next: (data) => (this.rows = data),
      error: () => (this.error = 'Unable to load holidays.')
    });
  }
}
