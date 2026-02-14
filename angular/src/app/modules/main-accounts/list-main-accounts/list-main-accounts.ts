import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';
import { LocalizationService } from '@abp/ng.core';
import { MainAccountService } from 'src/app/proxy/app-services/main-accounts';
import { MainAccountDto, MainAccountPagedRequestDto } from 'src/app/proxy/dtos/main-accounts';
import { BilingualNamePipe } from 'src/app/shared/pipes/bilingual-name.pipe';
import { LocalizationPipe } from '@abp/ng.core';

@Component({
  selector: 'app-list-main-accounts',
  standalone: true,
  imports: [CommonModule, RouterLink, BilingualNamePipe, LocalizationPipe],
  templateUrl: './list-main-accounts.html',
  styleUrl: './list-main-accounts.scss',
  animations: [
    trigger('pageEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(12px)' }),
        animate('400ms cubic-bezier(0.35, 0, 0.25, 1)', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('buttonEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.9)' }),
        animate('300ms 100ms cubic-bezier(0.34, 1.56, 0.64, 1)', style({ opacity: 1, transform: 'scale(1)' }))
      ])
    ]),
    trigger('tableEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('350ms 150ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('rowStagger', [
      transition(':enter', [
        query('tr', [
          style({ opacity: 0, transform: 'translateX(-16px)' }),
          stagger(45, [
            animate('280ms cubic-bezier(0.25, 0.46, 0.45, 0.94)', style({ opacity: 1, transform: 'translateX(0)' }))
          ])
        ], { optional: true })
      ])
    ]),
    trigger('noDataEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.95)' }),
        animate('350ms ease-out', style({ opacity: 1, transform: 'scale(1)' }))
      ])
    ])
  ]
})

export class ListMainAccounts implements OnInit {
  accounts: MainAccountDto[] = [];
  input: MainAccountPagedRequestDto = { maxResultCount: 10, skipCount: 0 };

  constructor(
    private mainAccountService: MainAccountService,
    private localizationService: LocalizationService
  ) {}

  ngOnInit(): void {
    this.loadMainAccounts();
  }

  loadMainAccounts(): void {
    this.mainAccountService.getList(this.input).subscribe(result => {
      this.accounts = result.items ?? [];
    });
  }

  trackById(_index: number, item: MainAccountDto): string {
    return item.id ?? '';
  }

  deleteAccount(account: MainAccountDto): void {
    if (!account?.id) return;
    const message = this.localizationService.instant('MainAccount::ConfirmDelete');
    if (!confirm(message)) return;
    this.mainAccountService.delete(account.id).subscribe({
      next: () => this.loadMainAccounts(),
      error: (err) => console.error(err)
    });
  }
}