// ===============================
// candidate-documents.manager.ts
// ===============================
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CandidateDocumentsService } from 'src/app/generated/api/candidateDocuments.service';
import { SearchCandidateDocumentsRequest } from 'src/app/generated/model/searchCandidateDocumentsRequest';


export interface PagedResult<T> {
items: T[];
totalCount: number;
}


@Injectable({ providedIn: 'root' })
export class CandidateDocumentsManager {


constructor(private api: CandidateDocumentsService) {}


search(request: SearchCandidateDocumentsRequest): Observable<PagedResult<any>> {
return this.api.candidateDocumentsControllerSearch(request);
}
}