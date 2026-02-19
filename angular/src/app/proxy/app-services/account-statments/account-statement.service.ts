import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { AccountStatementInputDto, AccountStatementLineDto, AccountStatementPeriodDto } from '../../dtos/account-statments/models';

@Injectable({
  providedIn: 'root',
})
export class AccountStatementService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  getAll = (input: AccountStatementPeriodDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AccountStatementLineDto[]>({
      method: 'GET',
      url: '/api/app/account-statement',
      params: { fromDate: input.fromDate, toDate: input.toDate },
    },
    { apiName: this.apiName,...config });
  

  getByAccount = (input: AccountStatementInputDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AccountStatementLineDto[]>({
      method: 'GET',
      url: '/api/app/account-statement/by-account',
      params: { accountId: input.accountId, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
}