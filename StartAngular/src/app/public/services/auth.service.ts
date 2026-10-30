import { HttpBackend, HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError, tap } from 'rxjs';
import { LoginModel } from '../../Models/LoginModel';
import { UserModel } from '../../Models/UserModel';
import { TokenResponse } from '../../Models/TokenResponse';

export interface LoginOtpPendingResponse {
  requiresOtp: boolean;
  userName: string;
  maskedEmail: string;
  message: string;
  expiresInMinutes: number;
}
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
  /** Bypasses interceptors so refresh/logout cannot loop on 401. */
  private readonly rawHttp: HttpClient;

  constructor(private httpClient: HttpClient, httpBackend: HttpBackend) {
    this.httpheaders = new HttpHeaders({ 'Content-Type': 'application/json' });
    this.rawHttp = new HttpClient(httpBackend);
  }

  /** Step 1: password check → server enqueues OTP email (no JWT yet). */
  ValidateUser(user: LoginModel): Observable<HttpResponse<LoginOtpPendingResponse>> {
    const body = {
      userName: user.userName,
      password: user.password
    };

    return this.httpClient.post<LoginOtpPendingResponse>(Environment.apiUrl + API.LOGIN, body, {
      headers: this.httpheaders,
      observe: 'response'
    });
  }

  /** Step 2: verify OTP → JWT + refresh token. */
  VerifyOtp(userName: string, otp: string): Observable<HttpResponse<UserModel>> {
    return this.httpClient.post<UserModel>(
      Environment.apiUrl + API.VERIFY_OTP,
      { userName, otp },
      { headers: this.httpheaders, observe: 'response' }
    );
  }

  /** Exchange refresh token for a new access + refresh pair (rotation). */
  refreshTokens(): Observable<TokenResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available.'));
    }

    return this.rawHttp
      .post<TokenResponse>(
        Environment.apiUrl + API.REFRESH,
        { refreshToken },
        { headers: this.httpheaders }
      )
      .pipe(tap((tokens) => this.updateTokens(tokens)));
  }

  setAuthUser(user: UserModel): void {
    sessionStorage.setItem(ACCESS_TOKEN_KEY, user.token);
    if (user.refreshToken) {
      sessionStorage.setItem(REFRESH_TOKEN_KEY, user.refreshToken);
    }
    sessionStorage.setItem(EMPLOYEE_KEY, JSON.stringify(user));
  }

  updateTokens(tokens: TokenResponse): void {
    sessionStorage.setItem(ACCESS_TOKEN_KEY, tokens.accessToken);
    sessionStorage.setItem(REFRESH_TOKEN_KEY, tokens.refreshToken);

    const user = this.getCurrentUser();
    if (user) {
      user.token = tokens.accessToken;
      user.refreshToken = tokens.refreshToken;
      user.expiresIn = tokens.expiresIn;
      sessionStorage.setItem(EMPLOYEE_KEY, JSON.stringify(user));
    }
  }

  getAccessToken(): string | null {
    return sessionStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return sessionStorage.getItem(REFRESH_TOKEN_KEY);
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
