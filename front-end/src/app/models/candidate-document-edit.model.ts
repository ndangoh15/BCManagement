// models/candidate-document-edit.model.ts
export interface ImportErrorDto {
  id: number;
  fieldName: string;
  errorType: string;
  errorMessage: string;
}

export interface CandidateDocumentEditDto {
  id: number;
  session: number;
  examCode: string;
  centreCode: string;
  candidateNumber: string;
  candidateName: string;
  filePath: string;
  pdfUrl: string;
  errors: ImportErrorDto[];
}
