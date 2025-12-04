import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface ImportedBatch {
  id: number;
  fileName: string;
  examYear: number;
  examCode: string;
  centreNumber: string;
  importedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class ImportedBatchesService {

private apiUrl = '/api/document/imported-batches';

  constructor(private http: HttpClient) {}

  getBatches(): Observable<ImportedBatch[]> {
    return this.http.get<ImportedBatch[]>(this.apiUrl);
  }
  
}
