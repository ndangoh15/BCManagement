import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { NgApexchartsModule } from 'ng-apexcharts';
import { CarouselModule } from 'ngx-owl-carousel-o';
import { SharedModule } from 'src/app/shared/sharedmodule';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { NgSelectModule } from '@ng-select/ng-select';

import { AgGridModule } from 'ag-grid-angular';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { BranchsComponent } from './company/branchs/branch.component';
import { BranchsFormComponent } from './company/branchs/branch-form/branch-form.component';
import { JobsComponent } from './company/job/job.component';
import { JobsFormComponent } from './company/job/job-form/job-form.component';
import { CompaniesComponent } from './company/companies/company.component';
import { CompanyFormComponent } from './company/companies/company-form/company-form.component';

import { CountriesComponent } from './localization/country/country.component';
import { CountriesFormComponent } from './localization/country/country-form/country-form.component';
import { RegionsComponent } from './localization/region/region.component';
import { RegionsFormComponent } from './localization/region/region-form/region-form.component';
import { TownsComponent } from './localization/town/town.component';
import { TownsFormComponent } from './localization/town/town-form/town-form.component';
import { QuartersComponent } from './localization/quarter/quarter.component';
import { QuartersFormComponent } from './localization/quarter/quarter-form/quarter-form.component';

import { ParameterRoutingModule } from './parameter-routing.module';
import { TranslocoModule } from '@ngneat/transloco';
import { NgxDropzoneModule } from 'ngx-dropzone';
import { FormCompentModule } from '../component/form-components/form-compnents.module';

@NgModule({
  declarations: [
    // Company
    BranchsComponent,
    BranchsFormComponent,
    JobsComponent,
    JobsFormComponent,
    CompaniesComponent,
    CompanyFormComponent,

    // Localization
    CountriesComponent,
    CountriesFormComponent,
    RegionsComponent,
    RegionsFormComponent,
    TownsComponent,
    TownsFormComponent,
    QuartersComponent,
    QuartersFormComponent,
  ],

  imports: [
    CommonModule,
    NgApexchartsModule,
    CarouselModule,
    SharedModule,
    FormsModule,
    NgxChartsModule,
    NgSelectModule,
    AgGridModule,
    ReactiveFormsModule,
    ParameterRoutingModule,
    TranslocoModule,
    NgxDropzoneModule,
    FormCompentModule,
  ],
})
export class ParameterModule {}
