import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import {
  CandidateDocumentsService,
  SearchCandidateDocumentsRequest
} from 'src/app/generated';

import { PagedResult } from 'src/app/models/paged-result';
import { CandidateDocumentDto } from 'src/app/models/candidate-document.dto';
import { HttpResponse } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class CandidateDocumentsManager {

  constructor(
    private api: CandidateDocumentsService
  ) {}

  // 🔍 Recherche paginée (AG-Grid infinite)
  search( request: SearchCandidateDocumentsRequest): Observable<PagedResult<CandidateDocumentDto>> {
    return this.api.candidateDocumentsControllerSearch(request) as Observable<PagedResult<CandidateDocumentDto>>;
  }

// 📄 GET PDF (AUTHORIZED, BLOB SAFE)

  getDocumentFile(id: number): Observable<Blob> {
    return this.api.candidateDocumentsControllerGetFile(
      id,
      'response',
      false,
      {
        httpHeaderAccept: 'application/pdf' // ⭐ CLÉ ICI
      }as any   // ✅ CAST INTENTIONNEL (BUG GEN-API)
    ).pipe(
      map((res: HttpResponse<any>) => res.body as Blob)
    );
  }

  // ✅ Récupération du PDF (Blob)
  getFile(id: number): Observable<Blob> {
  return this.api.candidateDocumentsControllerGetFile(
    id,
    'body',
    false,
    { httpHeaderAccept: 'application/pdf' } as any
  ) as Observable<Blob>;
}

  
}
