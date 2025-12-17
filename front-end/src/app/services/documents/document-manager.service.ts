import { Injectable } from '@angular/core';

import { Observable, Subject } from 'rxjs';
import { HttpClient, HttpEvent } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { DocumentService } from 'src/app/generated';

@Injectable({
  providedIn: 'root'
})
export class DocumentManagerService {

  // 🔔 Event bus pour refresh UI
  private documentListSubject = new Subject<void>();
  documentList$ = this.documentListSubject.asObservable();

  constructor(
    private documentService: DocumentService,
    private http: HttpClient
  ) {}

  // 🔄 notifier les composants
  emitRefreshList(): void {
    this.documentListSubject.next();
  }

  // ✅ Upload avec FormData + progress
  uploadSingleFile(
    file: File,
    decoded: any
  ): Observable<HttpEvent<any>> {

    const formData = new FormData();
    formData.append('Files', file);
    formData.append('ExamYear', decoded.year);
    formData.append('ExamCode', decoded.examCode);
    formData.append('CenterNumber', decoded.center);

    const url = `${environment.apiUrl}${environment.apiPrefix}/document/upload-multiple`;

    return this.http.post<any>(url, formData, {
      reportProgress: true,
      observe: 'events'
    });
  }

  // ✔ Vérifier doublons
  checkAlreadyImported(fileNames: string[]): Observable<string[]> {
    return this.documentService.documentControllerCheckIfAlreadyImported(fileNames);
  }
}
