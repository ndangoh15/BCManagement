import { Injectable } from "@angular/core";
import { ImportErrorsService } from "src/app/generated";

@Injectable({ providedIn: 'root' })
export class ImportErrorsManager {

  constructor(
    private api: ImportErrorsService
  ) {}

  loadErrors(filter: {
    session?: number;
    examCode?: string;
    centreCode?: string;
  }) {
    return this.api.importErrorsControllerGetImportErrors(
      filter.session,
      filter.examCode,
      filter.centreCode
    );
  }
}
