import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { LoginModel } from '../../Models/LoginModel';
import { UserModel } from '../../Models/UserModel';
import { Environment } from '../../../Environment/Enviroment';
import { API } from '../../Urls/API';
import {
  AppRole,
  PORTAL_NAV,
  Roles,
  canAccessPath,
  getHomePathForRoles,
  getPrimaryRole
} from '../../constants/roles';
import { ACCESS_TOKEN_KEY, EMPLOYEE_KEY, REFRESH_TOKEN_KEY } from '../../constants/tokenvalues';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly httpheaders: HttpHeaders;

  constructor(private httpClient: HttpClient) {
    this.httpheaders = new HttpHeaders({ 'Content-Type': 'application/json' });
  }

  ValidateUser(user: LoginModel): Observable<HttpResponse<UserModel>> {
    const body = {
      userName: user.userName,
      password: user.password
    };

    return this.httpClient.post<UserModel>(Environment.apiUrl + API.LOGIN, body, {
      headers: this.httpheaders,
      observe: 'response'
    });
  }

  setAuthUser(user: UserModel): void {
    sessionStorage.setItem(ACCESS_TOKEN_KEY, user.token);
    if (user.refreshToken) {
      sessionStorage.setItem(REFRESH_TOKEN_KEY, user.refreshToken);
    }
    sessionStorage.setItem(EMPLOYEE_KEY, JSON.stringify(user));
  }

  getAccessToken(): string | null {
    return sessionStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getCurrentUser(): UserModel | null {
    const raw = sessionStorage.getItem(EMPLOYEE_KEY);
    if (!raw) {
      return null;
    }
    try {
      return JSON.parse(raw) as UserModel;
    } catch {
      return null;
    }
  }

  getRoles(): string[] {
    return this.getCurrentUser()?.roles ?? [];
  }

  getPrimaryRole(): AppRole | null {
    return getPrimaryRole(this.getRoles());
  }

  hasRole(role: string): boolean {
    return this.getRoles().some((r) => r?.toLowerCase() === role.toLowerCase());
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some((role) => this.hasRole(role));
  }

  isAdmin(): boolean {
    return this.hasRole(Roles.Admin);
  }

  /** Role-based landing page after login. */
  getHomePath(): string {
    return getHomePathForRoles(this.getRoles());
  }

  /**
   * Use returnUrl only if the user's roles can open that path;
   * otherwise fall back to their home path.
   */
  resolvePostLoginPath(returnUrl?: string | null): string {
    const home = this.getHomePath();
    if (!returnUrl || returnUrl === '/' || returnUrl === '/login') {
      return home;
    }

    const normalized = returnUrl.startsWith('/') ? returnUrl : `/${returnUrl}`;
    const navMatch = PORTAL_NAV.find(
      (item) =>
        item.path === normalized ||
        (item.path !== '/portal' && normalized.startsWith(item.path + '/'))
    );

    if (navMatch && !canAccessPath(this.getRoles(), navMatch.roles)) {
      return home;
    }

    // Unknown/internal return URLs under /portal are allowed if authenticated;
    // RoleGuard will still block forbidden child routes.
    return normalized;
  }

  logout(): void {
    sessionStorage.removeItem(ACCESS_TOKEN_KEY);
    sessionStorage.removeItem(REFRESH_TOKEN_KEY);
    sessionStorage.removeItem(EMPLOYEE_KEY);
  }
}
