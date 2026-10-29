import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-designations',
  templateUrl: './designations.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class DesignationsComponent implements OnInit {
  rows: any[] = [];
  error = '';
  constructor(private api: PortalApiService) {}
  ngOnInit(): void {
    this.api.getDesignations().subscribe({
      next: (data) => (this.rows = data),
      error: () => (this.error = 'Unable to load designations.')
    });
  }
}
