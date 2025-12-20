import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ImportErrorsService } from 'src/app/generated';

@Injectable({
  providedIn: 'root'
})
export class ImportErrorsManager {

  constructor(
    private api: ImportErrorsService
  ) {}

  loadErrors(
    session: number,
    examCode: string,
    centreCode?: string
  ): Observable<any[]> {

    return this.api.importErrorsControllerGetImportErrors(
      session,
      examCode,
      centreCode
    ).pipe(
      map(res => res ?? [])
    );
  }

  loadInvalidCentres(
  session: number,
  examCode: string
) {
  return this.api.importErrorsControllerGetInvalidCentres(
    session,
    examCode
  );
}
}
