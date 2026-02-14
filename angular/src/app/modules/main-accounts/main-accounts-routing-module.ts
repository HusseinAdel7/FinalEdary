import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ListMainAccounts } from './list-main-accounts/list-main-accounts';
import { AddMainAccounts } from './add-main-accounts/add-main-accounts';

const routes: Routes = [
  {
    path: '',
    component: ListMainAccounts
  },
  {
    path: 'create',
    component: AddMainAccounts
  },
  {
    path: 'edit/:id',
    component: AddMainAccounts
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MainAccountsRoutingModule {}
