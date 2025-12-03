import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DocumentsRoutingModule } from './documents-routing.module';
import { ImportedBatchesComponent } from './imported-batches/imported-batches.component';
import { ImportMultipleComponent } from './import-multiple/import-multiple.component';

import { SharedModule } from 'src/app/shared/sharedmodule';
import { AgGridModule } from 'ag-grid-angular';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';


@NgModule({
  declarations: [
    ImportedBatchesComponent,
    ImportMultipleComponent
  ],
  imports: [
    CommonModule,
    FormsModule,             // 🔥 important pour les forms
    ReactiveFormsModule,     // 🔥 important pour FormBuilder + formGroup
    SharedModule,            // 🔥 indispensable pour app-modal + app-form-input
    AgGridModule,
    DocumentsRoutingModule
]
})
export class DocumentsModule { }
