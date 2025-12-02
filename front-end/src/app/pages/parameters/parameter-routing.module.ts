import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BranchsComponent } from './company/branchs/branch.component';
import { JobsComponent } from './company/job/job.component';
import { CompaniesComponent } from './company/companies/company.component';
import { CountriesComponent } from './localization/country/country.component';
import { RegionsComponent } from './localization/region/region.component';
import { TownsComponent } from './localization/town/town.component';
import { QuartersComponent } from './localization/quarter/quarter.component';

const routes: Routes = [
  {
    path: '',
    children: [
      { path: 'config/company/agencies', component: BranchsComponent },
      { path: 'config/company/jobs', component: JobsComponent },
      { path: 'config/company/companies', component: CompaniesComponent },

      { path: "config/locality/country", component: CountriesComponent },
      { path: "config/locality/region", component: RegionsComponent },
      { path: "config/locality/town", component: TownsComponent },
      { path: "config/locality/quarter", component: QuartersComponent },

    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ParameterRoutingModule { }
