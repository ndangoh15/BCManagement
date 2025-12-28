import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { CandidateDocumentsManager } from 'src/app/services/documents/candidate-documents.manager';

@Component({
  selector: 'app-pdf-preview',
  templateUrl: './pdf-preview.component.html',
  styleUrls: ['./pdf-preview.component.scss']
})
export class PdfPreviewComponent implements OnInit, OnDestroy {

  pdfUrl?: SafeResourceUrl;
  private rawUrl?: string;
  loading = true;

  constructor(
    private dialogRef: MatDialogRef<PdfPreviewComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { documentId: number },
    private documentsFacade: CandidateDocumentsManager,
    private sanitizer: DomSanitizer
  ) {}

    ngOnInit(): void {
    this.documentsFacade.getFile(this.data.documentId).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);

        const viewerParams =
      '#pagemode=thumbs' +     // ✅ sidebar ouverte
      '&toolbar=1' +
      '&navpanes=1' +
      '&scrollbar=1' +
      '&view=FitH';

    this.pdfUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
      `${url}${viewerParams}`);

        /*this.pdfUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
          `${url}#toolbar=1&navpanes=0&scrollbar=1&view=FitH`
        );*/
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        alert('Unable to open PDF');
      }
    });
  }


  /*ngOnInit(): void {
    
    const fileUrl = this.data.fileUrl;
    this.documentsFacade.getDocumentFile(this.data.documentId).subscribe({
      next: blob => {
        this.rawUrl = URL.createObjectURL(blob);
        //this.pdfUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.rawUrl);
    this.pdfUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
      `${fileUrl}#toolbar=1&navpanes=0&scrollbar=1&view=FitH`
    );

        this.loading = false;
      },
      error: () => {
        alert('Unable to load PDF');
        this.dialogRef.close();
      }
    });
  }*/

  close(): void {
    this.dialogRef.close();
  }

  ngOnDestroy(): void {
    if (this.rawUrl) {
      URL.revokeObjectURL(this.rawUrl);
    }
  }
}
