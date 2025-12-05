import { Component, OnInit, ViewChild } from '@angular/core';
import { ColDef, GridApi } from 'ag-grid-community';

import { DocumentService } from 'src/app/generated';
import { DocumentManagerService } from 'src/app/services/documents/document-manager.service';
import { AgGridThemeService } from 'src/app/services/theme/theme.service';


@Component({
  selector: 'app-imported-batches',
  templateUrl: './imported-batches.component.html',
  styleUrls: ['./imported-batches.component.scss'],
})
export class ImportedBatchesComponent implements OnInit {

  rowData: any[] = [];
  importModalOpen = false;
  public gridApi!: GridApi;

  public quickFilterText = '';

  public columnDefs: ColDef[] = [
    { field: "fileName" },
    { field: "examYear" },
    { field: "examCode" },
    { field: "centreNumber" },
    { field: "importedAt" },
  ];

    //public columnDefs = USER_COLUMNS_DEFS;
  
    public defaultColDef: ColDef = {
      filter: "agTextColumnFilter",
      floatingFilter: true,
      resizable: true,
      sortable: true,
      flex: 1,
    };

  @ViewChild('importModal') importModal: any;


  constructor(private docManager: DocumentManagerService, 
    private docService: DocumentService,
  private theme: AgGridThemeService,

) {}
/*
  public defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    flex: 1,
  };*/
  public rowSelection: "single" | "multiple" = "multiple";
  public paginationPageSize = 10;
  public paginationPageSizeSelector: number[] | boolean = [10, 25, 50]; // [10, 25, 50];
  public themeClass = this.theme.theme;


ngOnInit(): void {
  this.refreshData();

  this.docManager.documentListSubject.subscribe(() => {
    this.refreshData();
  });
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
