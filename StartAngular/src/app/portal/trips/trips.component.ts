import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../public/services/auth.service';
import { PortalApiService } from '../services/portal-api.service';

@Component({
  selector: 'app-trips',
  templateUrl: './trips.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class TripsComponent implements OnInit {
  trips: any[] = [];
  error = '';
  success = '';
  canCreate = false;
  payAmount: Record<number, number> = {};
  newTrip = {
    title: '',
    description: '',
    startDate: '',
    endDate: '',
    targetAmount: 5000
  };

  constructor(private api: PortalApiService, private auth: AuthService) {}

  ngOnInit(): void {
    this.canCreate = this.auth.hasAnyRole(['Admin', 'HR']);
    this.reload();
  }

  reload(): void {
    this.api.getTrips().subscribe({
      next: (data) => (this.trips = data),
      error: () => (this.error = 'Unable to load trips.')
    });
  }

  create(): void {
    this.error = '';
    this.api.createTrip(this.newTrip).subscribe({
      next: () => {
        this.success = 'Trip plan created.';
        this.newTrip = { title: '', description: '', startDate: '', endDate: '', targetAmount: 5000 };
        this.reload();
      },
      error: (err) => (this.error = err?.error?.message || 'Unable to create trip.')
    });
  }

  pay(trip: any): void {
    const amount = this.payAmount[trip.tripPlanId];
    if (!amount || amount <= 0) {
      this.error = 'Enter a payment amount.';
      return;
    }
    this.api.payTrip(trip.tripPlanId, { amount, paymentMethod: 'UPI' }).subscribe({
      next: (res) => {
        this.success = res?.message || 'Payment recorded.';
        this.payAmount[trip.tripPlanId] = 0;
        this.reload();
      },
      error: (err) => (this.error = err?.error?.message || 'Payment failed.')
    });
  }
}
