import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ImportedBatchesComponent } from './imported-batches/imported-batches.component';
import { ImportMultipleComponent } from './import-multiple/import-multiple.component';

const routes: Routes = [
  {

    path: "",
        children: [
          { path: 'documents/imported-batches',   component: ImportedBatchesComponent},
          { path: 'import-multiple',  component: ImportMultipleComponent}
        ]
    
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DocumentsRoutingModule { }
