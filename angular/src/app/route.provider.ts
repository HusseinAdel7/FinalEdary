import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);

  routes.add([
    {
      path: '/',
      name: '::Menu:Home',
      iconClass: 'fas fa-home',
      order: 1,
      layout: eLayoutType.application,
    },


    {
      path: '/accounting',
      name: '::Menu:Accounting',
      iconClass: 'fas fa-calculator',
      order: 2,
      layout: eLayoutType.application,
    },
    {
      path: 'main-accounts',
      name: '::Menu:MainAccounts',
      iconClass: 'fa-solid fa-sitemap',
      parentName: '::Menu:Accounting',
      order: 1,
      layout: eLayoutType.application,
    },
    {
      path: 'sub-accounts',
      name: '::Menu:SubAccounts',
      iconClass: 'fa-solid fa-diagram-project',
      parentName: '::Menu:Accounting',
      order: 2,
      layout: eLayoutType.application,
    },
    {
      path: 'journal-entries',
      name: 'Journal Entries',
      iconClass: 'fa-solid fa-book-open',
      parentName: 'Accounting',
      order: 3,
      layout: eLayoutType.application,
    },


    {
      path: '/purchases',
      name: '::Menu:Purchases',
      iconClass: 'fas fa-shopping-cart',
      order: 3,
      layout: eLayoutType.application,
    },
    {
      path: 'suppliers',
      name: '::Menu:Suppliers',
      iconClass: 'fa-solid fa-truck',
      parentName: '::Menu:Purchases',
      order: 1,
      layout: eLayoutType.application,
    },


    {
      path: '/inventory',
      name: 'Inventory',
      iconClass: 'fas fa-boxes',
      order: 4,
      layout: eLayoutType.application,
    },
    {
      path: 'items',
      name: 'Items',
      iconClass: 'fa-solid fa-box-open',
      parentName: 'Inventory',
      order: 1,
      layout: eLayoutType.application,
    },
    {
      path: 'warehouses',
      name: 'Warehouses',
      iconClass: 'fa-solid fa-warehouse',
      parentName: 'Inventory',
      order: 2,
      layout: eLayoutType.application,
    },
  ]);
}