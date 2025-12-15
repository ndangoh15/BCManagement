import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { DocumentService } from "src/app/generated";

@Injectable({ providedIn: 'root' })
export class DocumentUploadService {

  constructor(
    private http: HttpClient,
    private documentApi: DocumentService //  pour récupérer la config
  ) {}

  uploadMultiple(
    files: File[],
    payload: {
      examYear: string;
      examCode: string;
      centerNumber: string;
      uploadedBy: number;
    }
  ) {
    const formData = new FormData();

    files.forEach(f => formData.append('Files', f));
    formData.append('ExamYear', payload.examYear);
    formData.append('ExamCode', payload.examCode);
    formData.append('CenterNumber', payload.centerNumber);

    //  URL dérivée de la config OpenAPI
    const baseUrl = (this.documentApi as any).configuration.basePath;

    return this.http.post(
      `${baseUrl}/api/document/upload-multiple`,
      formData,
      {
        observe: 'events',
        reportProgress: true
      }
    );
  }
}
