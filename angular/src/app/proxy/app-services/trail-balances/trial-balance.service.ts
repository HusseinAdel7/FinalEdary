import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { TrialBalanceLineDto } from '../../dtos/trail-balances/models';

@Injectable({
  providedIn: 'root',
})
export class TrialBalanceService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  get = (fromDate: string, toDate: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TrialBalanceLineDto[]>({
      method: 'GET',
      url: '/api/app/trial-balance',
      params: { fromDate, toDate },
    },
    { apiName: this.apiName,...config });
  

  getForAccount = (accountId: string, fromDate: string, toDate: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TrialBalanceLineDto[]>({
      method: 'GET',
      url: `/api/app/trial-balance/for-account/${accountId}`,
      params: { fromDate, toDate },
    },
    { apiName: this.apiName,...config });
}