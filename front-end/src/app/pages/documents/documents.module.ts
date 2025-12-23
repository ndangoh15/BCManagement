import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DocumentsRoutingModule } from './documents-routing.module';
import { SharedModule } from 'src/app/shared/sharedmodule';
import { AgGridModule } from 'ag-grid-angular';

import { ImportedBatchesComponent } from './imported-batches/imported-batches.component';
import { ImportMultipleComponent } from './imported-batches/import-multiple/import-multiple.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ImportErrorsComponent } from './import-errors/import-errors.component';
import { FixImportErrorComponent } from './import-errors/fix-import-error/fix-import-error.component';
import { MaterialModule } from 'src/app/materialModule/material-module/material-module.module';

@NgModule({
  declarations: [
    ImportedBatchesComponent,
    ImportMultipleComponent,
    ImportErrorsComponent,
    FixImportErrorComponent
  ],
  imports: [
    CommonModule,
    SharedModule,
    AgGridModule,
    FormsModule,
    ReactiveFormsModule,
    DocumentsRoutingModule,
    SharedModule  ,
    MaterialModule    
  ]
})
export class DocumentsModule { }
