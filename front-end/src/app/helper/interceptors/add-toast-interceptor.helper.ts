import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Injectable()
export class ToastInterceptor implements HttpInterceptor {

  constructor(private toastr: ToastrService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    // 🚫 No toast for upload/import
    if (req.url.includes('/upload') || req.url.includes('/import')) {
      return next.handle(req);
    }

    // catch & show error toast
    return next.handle(req).pipe(
      tap(() => {}),
      catchError((error: HttpErrorResponse) => {
        this.toastr.error(error.error?.message || 'Unexpected error');
        return throwError(() => error);
      })
    );
  }
}
