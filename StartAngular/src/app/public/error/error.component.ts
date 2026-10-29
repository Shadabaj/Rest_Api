import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiErrorService, ApiErrorState } from '../services/api-error.service';

@Component({
  selector: 'app-error',
  templateUrl: './error.component.html',
  styleUrls: ['./error.component.css']
})
export class ErrorComponent implements OnInit {
  error: ApiErrorState | null = null;

  constructor(
    private apiErrorService: ApiErrorService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.error = this.apiErrorService.getError();
  }

  goHome(): void {
    this.apiErrorService.clear();
    this.router.navigate(['/']);
  }

  retryLogin(): void {
    this.apiErrorService.clear();
    this.router.navigate(['/login']);
  }
}
