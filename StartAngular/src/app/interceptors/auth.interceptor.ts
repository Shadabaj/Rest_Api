import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, finalize, map, shareReplay, switchMap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../public/services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  /** Shared in-flight refresh so parallel 401s wait on one call. */
  private refresh$: Observable<string> | null = null;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (this.isAuthEndpoint(request.url)) {
      return next.handle(request);
    }

    const token = this.authService.getAccessToken();
    const authRequest = token ? this.withBearer(request, token) : request;

    return next.handle(authRequest).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status !== 401) {
          return throwError(() => error);
        }
        return this.retryWithRefresh(authRequest, next);
      })
    );
  }

  private retryWithRefresh(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    if (!this.authService.getRefreshToken()) {
      this.forceLogin();
      return throwError(() => new HttpErrorResponse({ status: 401, statusText: 'Unauthorized' }));
    }

    return this.refreshAccessToken().pipe(
      switchMap((accessToken) => next.handle(this.withBearer(request, accessToken))),
      catchError((refreshError) => throwError(() => refreshError))
    );
  }

  private refreshAccessToken(): Observable<string> {
    if (!this.refresh$) {
      this.refresh$ = this.authService.refreshTokens().pipe(
        map((tokens) => tokens.accessToken),
        catchError((err) => {
          this.forceLogin();
          return throwError(() => err);
        }),
        finalize(() => {
          this.refresh$ = null;
        }),
        shareReplay({ bufferSize: 1, refCount: true })
      );
    }
    return this.refresh$;
  }

  private withBearer(request: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
    return request.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  private isAuthEndpoint(url: string): boolean {
    const lower = url.toLowerCase();
    return (
      lower.includes('login/validate') ||
      lower.includes('login/verify-otp') ||
      lower.includes('login/refresh') ||
      lower.includes('login/logout')
    );
  }

  private forceLogin(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
