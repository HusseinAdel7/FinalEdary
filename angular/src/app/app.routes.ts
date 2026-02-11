import { Routes } from '@angular/router';
import { authGuard, permissionGuard } from '@abp/ng.core';

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./home/home.component').then(c => c.HomeComponent),
  },

  {
    path: 'main-accounts',
    // canActivate: [authGuard],
    loadChildren: () =>
      import('./modules/main-accounts/main-accounts-module')
        .then(m => m.MainAccountsModule),
  },

  {
    path: 'sub-accounts',
    // canActivate: [authGuard],
    loadChildren: () =>
      import('./modules/sub-accounts/sub-accounts-module')
        .then(m => m.SubAccountsModule),
  },

  {
    path: 'journal-entries',
    // canActivate: [authGuard],
    loadChildren: () =>
      import('./modules/journal-entries/journal-entries-module')
        .then(m => m.JournalEntriesModule),
  },

  {
    path: 'items',
    // canActivate: [authGuard],
    loadChildren: () =>
      import('./modules/items/items-module')
        .then(m => m.ItemsModule),
  },

  {
    path: 'warehouses',
    // canActivate: [authGuard],
    loadChildren: () =>
      import('./modules/warehouses/warehouses-module')
        .then(m => m.WarehousesModule),
  },

  {
    path: 'suppliers',
    // canActivate: [authGuard],
    loadChildren: () =>
      import('./modules/suppliers/suppliers-module')
        .then(m => m.SuppliersModule),
  },

  // ABP Built-in Modules
  {
    path: 'account',
    loadChildren: () =>
      import('@abp/ng.account').then(m => m.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () =>
      import('@abp/ng.identity').then(m => m.createRoutes()),
  },
  {
    path: 'tenant-management',
    loadChildren: () =>
      import('@abp/ng.tenant-management').then(m => m.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () =>
      import('@abp/ng.setting-management').then(m => m.createRoutes()),
  },
];