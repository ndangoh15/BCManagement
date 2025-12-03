import { Component, OnInit } from '@angular/core';
import { ColDef } from 'ag-grid-community';
import { openModal } from 'src/app/helper/helper-function';
import { ImportedBatch, ImportedBatchesService } from 'src/app/services/documents/imported-batches.service';

@Component({
  selector: 'app-imported-batches',
  templateUrl: './imported-batches.component.html',
  styleUrls: ['./imported-batches.component.scss']
})
export class ImportedBatchesComponent implements OnInit {

  batches: ImportedBatch[] = [];

  columnDefs: ColDef[] = [
    { headerName: 'ID', field: 'id', width: 80 },
    { headerName: 'File Name', field: 'fileName', flex: 2 },
    { headerName: 'Exam Year', field: 'examYear' },
    { headerName: 'Exam Code', field: 'examCode' },
    { headerName: 'Center Number', field: 'centreNumber' },
    { 
      headerName: 'Imported At', 
      field: 'importedAt',
      valueFormatter: p => new Date(p.value).toLocaleString()
    }
  ];

  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true
  };

  constructor(
    private batchService: ImportedBatchesService
  ) {}

  ngOnInit(): void {
    this.loadBatches();
  }

  loadBatches() {
    this.batchService.getBatches().subscribe({
      next: (data) => this.batches = data,
      error: (err) => console.error('Error loading batches', err)
    });
  }

  importNewFiles() {
    alert("TODO → ouvrir la popup d'import 👌");
  }
  openImportModal() {
    openModal('import-multiple-modal');
  }
}
