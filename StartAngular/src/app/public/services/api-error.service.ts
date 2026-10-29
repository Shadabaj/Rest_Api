import { Injectable } from '@angular/core';

export interface ApiErrorState {
  status: number | undefined;
  message: string | undefined;
  url?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ApiErrorService {
  private lastError: ApiErrorState | null = null;

  setError(error: ApiErrorState): void {
    this.lastError = error;
  }

  getError(): ApiErrorState | null {
    return this.lastError;
  }

  clear(): void {
    this.lastError = null;
  }
}
