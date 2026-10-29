import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, UrlTree } from '@angular/router';
import { AuthService } from '../public/services/auth.service';

@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean | UrlTree {
    if (!this.authService.getAccessToken()) {
      return this.router.createUrlTree(['/login']);
    }

    const roles = (route.data['roles'] as string[]) || [];
    if (!roles.length || this.authService.hasAnyRole(roles)) {
      return true;
    }

    // Wrong role for this page → send them to their own home
    return this.router.createUrlTree([this.authService.getHomePath()]);
  }
}
