import { Component, OnInit, ViewChild } from '@angular/core';
import { ColDef, GridApi } from 'ag-grid-community';
import { DocumentService } from 'src/app/generated';

@Component({
  selector: 'app-imported-batches',
  templateUrl: './imported-batches.component.html',
})
export class ImportedBatchesComponent implements OnInit {

  rowData: any[] = [];
  importModalOpen = false;
  public gridApi!: GridApi;

  public columnDefs: ColDef[] = [
    { field: "fileName" },
    { field: "examYear" },
    { field: "examCode" },
    { field: "centreNumber" },
    { field: "importedAt" },
  ];
@ViewChild('importModal') importModal: any;

  public defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    flex: 1,
  };
  public paginationPageSize = 10;

  constructor(private docService: DocumentService) {}

  ngOnInit(): void {
    this.refreshData();
  }

  onGridReady(params: any) {
    this.gridApi = params.api;
    params.api.sizeColumnsToFit();
  }

  refreshData() {
    this.docService.documentControllerGetImportedBatches()
      .subscribe(res => this.rowData = res);
  }

  openImportModal() {
    this.importModalOpen = true;
  }
}
