import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import * as CONST from '../../app-const';

@Injectable()
export class AddTokenInterceptor implements HttpInterceptor {

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    const token = localStorage.getItem(CONST.TOKEN_VALUE);

    // 🚫 Ignore ONLY exact login endpoints
    if (
      req.url.includes('/login') ||
      req.url.includes('/auth/login') ||
      req.url.includes('/authentication/login')
    ) {
      return next.handle(req);
    }

    // 🚫 Upload/import → pas de token obligatoire
    /*if (req.url.includes('/upload') || req.url.includes('/import')) {
      return next.handle(req);
    }*/

    // ✔ attach token for every secured call
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }

    return next.handle(req);
  }
}
