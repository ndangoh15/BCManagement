import { Routes } from '@angular/router';
import { PrivilegeGuard } from 'src/app/guard/privilege.guard';

export const content: Routes = [
  {
    path: '',
    loadChildren: () => import('../../pages/parameters/parameter.module').then(m => m.ParameterModule),
    canActivate: [PrivilegeGuard]
  },
  {
    path: '',
    loadChildren: () => import('../../pages/security/security.module').then(m => m.SecurityModule),
    canActivate: [PrivilegeGuard]

  },

  {
    path: '',
    loadChildren: () => import('../../pages/dashboard/dashboard.module').then(m => m.DashboardModule),
  },

  {
    path: '',
    loadChildren: () => import('../../pages/user-settings/profile.module').then(m => m.ProfileModule),
  },


  {
    path: '',
    loadChildren: () => import('../../components/authentication/errorpages/errorpages.module').then(m => m.ErrorpagesModule),
  },

  {
  path: '',
  loadChildren: () =>
      import('../../pages/documents/documents.module').then(m => m.DocumentsModule)
  },

];
