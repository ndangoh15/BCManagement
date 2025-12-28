import { Component } from '@angular/core';
import { ColDef, GridApi, GridReadyEvent, IDatasource , ICellRendererParams} from 'ag-grid-community';
import { CandidateDocumentsManager } from 'src/app/services/documents/candidate-documents.manager';

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
      width: 110,
      cellRenderer: (params: ICellRendererParams) => {
        const btn = document.createElement('button');
        btn.innerText = 'View';
        btn.classList.add('btn-view');
        btn.onclick = () => this.openPdf(params.data.id);
        return btn;
      }
    }
  ];

  defaultColDef: ColDef = {
    sortable: true,
    resizable: true
  };

  constructor(private facade: CandidateDocumentsManager) {}

/*openPdf(row: any): void {
  if (!row?.id) {
    alert('Document not available');
    return;
  }
  const url = this.facade.getDocumentFileUrl(row.id);
  window.open(url, '_blank');
}*/

openPdf(id: number): void {
  this.facade.getDocumentFile(id).subscribe({
    next: blob => {
      const url = URL.createObjectURL(blob);
      window.open(url);
    },
    error: err => {
      console.error('Unable to open PDF', err);
      alert('You are not authorized to view this document');
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
