import { Component } from '@angular/core';
import { ColDef, GridReadyEvent } from 'ag-grid-community';
import { ImportErrorsManager } from 'src/app/services/documents/import-errors.manager';


@Component({
  selector: 'app-import-errors',
  templateUrl: './import-errors.component.html'
})
export class ImportErrorsComponent {

  // 🔎 Filters
  filters = {
    session: null as number | null,
    examCode: '',
    centreCode: ''
  };

  quickFilterText = '';
  rowData: any[] = [];

  paginationPageSize = 20;
  paginationPageSizeSelector = [10, 20, 50, 100];

  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true
  };

  columnDefs: ColDef[] = [
    { headerName: '#', valueGetter: 'node.rowIndex + 1', width: 80 },
    { headerName: 'Candidate Name', field: 'candidateName' },
    { headerName: 'CIN', field: 'candidateNumber' },
    { headerName: 'Field', field: 'fieldName' },
    { headerName: 'Error Type', field: 'errorType' },
    { headerName: 'Message', field: 'errorMessage', flex: 2 },
    {
      headerName: 'Actions',
      width: 120,
      field: 'action',
      cellRenderer: () => 'Edit'
    }
  ];

  fixModalOpen = false;
selectedErrorId!: number;

onCellClicked(event: any): void {
  if (event.colDef.headerName === 'Actions') {
    console.log('OPEN FIX MODAL', event.data.errorId);
    this.selectedErrorId = event.data.errorId;
    this.fixModalOpen = true;
  }
}


  constructor(
    private errorsManager: ImportErrorsManager  ) {}

  onGridReady(event: GridReadyEvent): void {}

  onSearch(): void {

    if (!this.filters.session || !this.filters.examCode) {
      alert('Session and Exam Code are required');
      return;
    }

    this.errorsManager
      .loadErrors(
        this.filters.session,
        this.filters.examCode,
        this.filters.centreCode || undefined
      )
      .subscribe({
        next: data => this.rowData = data,
        error: err => {
          console.error(err);
          this.rowData = [];
        }
      });
  }

  centreOptions: string[] = [];

onFiltersChanged(): void {

  this.centreOptions = [];
  this.filters.centreCode = '';

  if (!this.filters.session || !this.filters.examCode) {
    return;
  }

  this.errorsManager
    .loadInvalidCentres(this.filters.session, this.filters.examCode)
    .subscribe({
      next: centres => {
        console.log('Centres chargés:', centres);
        this.centreOptions = centres;
      },
      error: err => {
        console.error('Erreur chargement centres', err);
        this.centreOptions = [];
      }
    });
}

refreshData() {
  this.fixModalOpen = false;
  this.onSearch(); // recharge la liste avec les filtres actuels
}


  themeClass(): string {
    return 'ag-theme-alpine';
  }
}
