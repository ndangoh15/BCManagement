import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ImportedBatchesComponent } from './imported-batches/imported-batches.component';
import { ImportErrorsComponent } from './import-errors/import-errors.component';
import { CandidateSearchComponent } from './candidate-search/candidate-search.component';

const routes: Routes = [
  {
    path: '',
    children: 
    [
      {path: 'documents/imported-batches', component: ImportedBatchesComponent},
      {path: 'documents/import-errors',  component: ImportErrorsComponent},
      {path: 'documents/candidate-search',  component: CandidateSearchComponent}
    ] 
  }
];



@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DocumentsRoutingModule {}
