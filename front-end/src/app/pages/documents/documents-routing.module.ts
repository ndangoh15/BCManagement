import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ImportedBatchesComponent } from './imported-batches/imported-batches.component';

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: 'documents/imported-batches', component: ImportedBatchesComponent
      }
    ]
  }
];


@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DocumentsRoutingModule {}
