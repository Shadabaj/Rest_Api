import { Component, OnInit } from '@angular/core';
import { PortalApiService } from '../services/portal-api.service';
import { AuthService } from '../../public/services/auth.service';

@Component({
  selector: 'app-me-home',
  templateUrl: './me-home.component.html',
  styleUrls: ['../shared/portal-table.css']
})
export class MeHomeComponent implements OnInit {
  profile: any = null;
  error = '';
  userName = '';

  constructor(private api: PortalApiService, private auth: AuthService) {}

  ngOnInit(): void {
    this.userName = this.auth.getCurrentUser()?.userName || '';
    this.api.getMyProfile().subscribe({
      next: (data) => (this.profile = data),
      error: () => (this.error = 'Unable to load your profile.')
    });
  }
}
