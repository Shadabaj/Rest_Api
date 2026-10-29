import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../public/services/auth.service';
import { NavItem, PORTAL_NAV } from '../../constants/roles';

@Component({
  selector: 'app-portal-layout',
  templateUrl: './portal-layout.component.html',
  styleUrls: ['./portal-layout.component.css']
})
export class PortalLayoutComponent implements OnInit {
  brandName = 'Employee 360';
  menuOpen = false;
  nav: NavItem[] = [];
  displayName = '';
  roleLabel = '';

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    this.displayName = user ? `${user.firstName} ${user.lastName}`.trim() || user.userName : '';
    this.roleLabel = (user?.roles || []).join(', ');
    this.nav = PORTAL_NAV.filter((item) => this.authService.hasAnyRole(item.roles));
  }

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }

  closeMenu(): void {
    this.menuOpen = false;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
