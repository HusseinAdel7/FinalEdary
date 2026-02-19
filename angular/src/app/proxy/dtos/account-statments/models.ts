import type { PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface AccountStatementInputDto extends PagedAndSortedResultRequestDto {
  accountId?: string;
  fromDate?: string;
  toDate?: string | null;
}

export interface AccountStatementLineDto {
  mainAccountName?: string;
  subAccountName?: string;
  entryDate?: string | null;
  description?: string;
  debit?: number | null;
  credit?: number | null;
  runningBalance?: number | null;
  sortOrder?: number;
}

export interface AccountStatementPeriodDto {
  fromDate?: string;
  toDate?: string | null;
}
