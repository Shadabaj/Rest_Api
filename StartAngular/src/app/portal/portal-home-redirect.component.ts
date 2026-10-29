import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../public/services/auth.service';
import { Roles } from '../constants/roles';

@Component({
  selector: 'app-portal-home-redirect',
  template: `<p>Redirecting…</p>`
})
export class PortalHomeRedirectComponent implements OnInit {
  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    const target = this.authService.getHomePath();
    this.router.navigateByUrl(target || '/login', { replaceUrl: true });
  }
}
