import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-locations',
  templateUrl: './locations.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class LocationsComponent implements OnInit {
  countries: any[] = [];
  workLocations: any[] = [];
  error = '';
  constructor(private api: PortalApiService) {}
  ngOnInit(): void {
    this.api.getCountries().subscribe({
      next: (data) => (this.countries = data),
      error: () => (this.error = 'Unable to load locations.')
    });
    this.api.getWorkLocations().subscribe({ next: (data) => (this.workLocations = data) });
  }
}
