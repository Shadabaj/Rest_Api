import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { ApiErrorService } from '../public/services/api-error.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(
    private router: Router,
    private apiErrorService: ApiErrorService
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        const isLoginCall = request.url.toLowerCase().includes('login/validate');
        const status = error.status || 0;
        const message =
          error.error?.message ||
          error.message ||
          'Something went wrong while talking to the server.';

        // Let the page handle validation / business errors (e.g. already clocked in)
        if (status === 400 || (isLoginCall && status === 401)) {
          return throwError(() => error);
        }

        this.apiErrorService.setError({
          status,
          message,
          url: request.url
        });

        if (status === 401 || status === 403) {
          this.router.navigate(['/unauthorized']);
        } else {
          // network (0) and server errors
          this.router.navigate(['/error']);
        }

        return throwError(() => error);
      })
    );
  }
}
