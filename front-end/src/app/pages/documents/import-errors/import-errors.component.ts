import { Component, OnInit } from '@angular/core';
import { ColDef, GridReadyEvent } from 'ag-grid-community';
import { ImportErrorsManager } from 'src/app/services/documents/import-errors.manager';

@Component({
  selector: 'app-import-errors',
  templateUrl: './import-errors.component.html',
})
export class ImportErrorsComponent implements OnInit {

  rowData: any[] = [];
  quickFilterText = '';

  session = 2025;
  examCode = '7100';
  centreCode = '11133';

  paginationPageSize = 20;
  paginationPageSizeSelector = [10, 20, 50, 100];

  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true,
    flex: 1,
  };

  columnDefs: ColDef[] = [
    { headerName: '#', valueGetter: 'node.rowIndex + 1', width: 70 },
    { headerName: 'Candidate Name', field: 'candidateName' },
    { headerName: 'CIN', field: 'candidateNumber' },
    { headerName: 'Field', field: 'fieldName' },
    { headerName: 'Error Type', field: 'errorType' },
    { headerName: 'Message', field: 'errorMessage' },
    {
      headerName: 'Action',
      cellRenderer: () => `
        <button class="ti-btn ti-btn-sm ti-btn-primary" disabled>
          Edit
        </button>
      `,
      width: 120
    }
  ];

  constructor(private facade: ImportErrorsManager) {}

  ngOnInit(): void {
    this.loadErrors();
  }

  onGridReady(event: GridReadyEvent) {
    event.api.sizeColumnsToFit();
  }

  loadErrors() {
    this.facade.loadErrors({
      session: this.session,
      examCode: this.examCode,
      centreCode: this.centreCode
    }).subscribe({
      next: data => this.rowData = data,
      error: err => console.error(err)
    });
  }

  themeClass() {
    return 'ag-theme-alpine';
  }
}
