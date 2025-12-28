import { Component } from '@angular/core';
import { ColDef, GridApi, GridReadyEvent, IDatasource , ICellRendererParams} from 'ag-grid-community';
import { CandidateDocumentsManager } from 'src/app/services/documents/candidate-documents.manager';
import { MatDialog } from '@angular/material/dialog';
import { PdfPreviewComponent } from 'src/app/shared/pdf-preview/pdf-preview.component';

@Component({
  selector: 'app-candidate-search',
  templateUrl: './candidate-search.component.html',
  styleUrls: ['./candidate-search.component.scss']
})
export class CandidateSearchComponent {

  // ---- Search criteria ----
  session!: number;
  examCode!: string;
  keyword = '';
  centerNumber = '';

  private gridApi!: GridApi;

  columnDefs: ColDef[] = [
    { field: 'candidateNumber', headerName: 'CIN', width: 150 },
    { field: 'candidateName', headerName: 'Candidate Name', flex: 1 },
    { field: 'centreCode', headerName: 'Centre', width: 120 },
    { field: 'examCode', headerName: 'Exam', width: 120 },
    { field: 'session', headerName: 'Session', width: 100 },
    {
      headerName: 'Document',
      cellRenderer: (params: any) => {
        const button = document.createElement('button');
        button.innerText = 'View';
        button.classList.add('view-btn');

        button.addEventListener('click', () => {
          this.openPdf(params.data.id);
        });

        return button;
      }
    }

  ];

  defaultColDef: ColDef = {
    sortable: true,
    resizable: true
  };

  constructor(private facade: CandidateDocumentsManager,
    private dialog: MatDialog) {}


openPdf(documentId: number): void {
  this.dialog.open(PdfPreviewComponent, {
    width: '100vw',
    height: '100vh',
    maxWidth: '100vw',
    panelClass: 'pdf-dialog',
    data: {
      documentId
    }
  });
}





  onGridReady(event: GridReadyEvent): void {
    this.gridApi = event.api;
  }

  search(): void {
     if (!this.keyword || this.keyword.trim().length < 2) {
    alert('Please enter at least 2 characters of the candidate name');
    return;
  }

    // ✅ API AG-Grid v32
    this.gridApi.setGridOption('datasource', this.createDatasource());
  }

  private createDatasource(): IDatasource {
    return {
      getRows: params => {

        const startRow = params.startRow ?? 0;
        const endRow = params.endRow ?? 50;
        const pageSize = endRow - startRow;
        const page = Math.floor(startRow / pageSize) + 1;

        const request = {
          session: this.session,
          examCode: this.examCode,
          candidateName: this.keyword.trim(),
          centerNumber: this.centerNumber || undefined,
          page,
          pageSize
        };

        this.facade.search(request).subscribe({
          next: res => {
            // ✅ Infinite Row Model callback
            params.successCallback(res.items, res.totalCount);
          },
          error: () => {
            params.failCallback();
          }
        });
      }
    };
  }
}
