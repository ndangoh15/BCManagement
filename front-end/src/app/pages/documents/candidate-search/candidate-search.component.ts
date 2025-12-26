import { Component } from '@angular/core';
import { ColDef, GridApi, GridReadyEvent, IDatasource } from 'ag-grid-community';
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
    { field: 'session', headerName: 'Session', width: 100 }
  ];

  defaultColDef: ColDef = {
    sortable: true,
    resizable: true
  };

  constructor(private facade: CandidateDocumentsManager) {}

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
          keyword: this.keyword || undefined,
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
