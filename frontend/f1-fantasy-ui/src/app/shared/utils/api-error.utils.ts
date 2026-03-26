import { HttpErrorResponse } from '@angular/common/http';
import { ApiErrorResponse } from '../models/api-error.model';

export function getApiErrorMessage(
  error: unknown,
  fallbackMessage: string
): string {
  if (error instanceof HttpErrorResponse) {
    const apiError = error.error as ApiErrorResponse | null;

    if (apiError?.detail) {
      return apiError.detail;
    }

    if (typeof error.error === 'string' && error.error.trim()) {
      return error.error;
    }
  }

  return fallbackMessage;
}