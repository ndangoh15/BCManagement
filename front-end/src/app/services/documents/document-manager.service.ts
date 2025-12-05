import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { DocumentService } from 'src/app/generated';
import { HttpEvent, HttpEventType } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class DocumentManagerService {

  // Liste des fichiers importés (pour rafraîchir UI)
  documentListSubject = new Subject<void>();

  constructor(private documentService: DocumentService) {}

  emitRefreshList(): void {
    this.documentListSubject.next();
  }

  // Upload SEQUENTIEL avec reportProgress
  uploadSingleFile(file: File, decoded: any) {
    return this.documentService.documentControllerUploadMultiple(
      [file],
      decoded.year,
      decoded.examCode,
      decoded.center,
      2,         // uploadedBy si tu veux
      'events',          // clé pour activer event upload
      true               // reportProgress = true
    );
  }

  checkAlreadyImported(fileNames: string[]): Observable<string[]> {
    return this.documentService.documentControllerCheckIfAlreadyImported(fileNames);
  }
}
