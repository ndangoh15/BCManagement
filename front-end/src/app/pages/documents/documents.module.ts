import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DocumentsRoutingModule } from './documents-routing.module';
import { SharedModule } from 'src/app/shared/sharedmodule';
import { AgGridModule } from 'ag-grid-angular';

import { ImportedBatchesComponent } from './imported-batches/imported-batches.component';
import { ImportMultipleComponent } from './imported-batches/import-multiple/import-multiple.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ImportErrorsComponent } from './import-errors/import-errors.component';

@NgModule({
  declarations: [
    ImportedBatchesComponent,
    ImportMultipleComponent,
    ImportErrorsComponent
  ],
  imports: [
    CommonModule,
    SharedModule,
    AgGridModule,
    FormsModule,
    ReactiveFormsModule,
    DocumentsRoutingModule
  ]
})
export class DocumentsModule { }
