// src/app/models/import-error.model.ts
export interface ImportError {
  id: number;
  session?: number;
  examCode?: string;
  centreCode?: string;
  candidateNumber: string;
  candidateName: string;
  fieldName: string;
  errorType: string;
  errorMessage: string;
  filePath: string;
}
