import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MainAccountService } from 'src/app/proxy/app-services/main-accounts';
import { MainAccountDto, MainAccountPagedRequestDto } from 'src/app/proxy/dtos/main-accounts';
import { BilingualNamePipe } from 'src/app/shared/pipes/bilingual-name.pipe';
import { LocalizationPipe } from '@abp/ng.core';

@Component({
  selector: 'app-list-main-accounts',
  standalone: true,
  imports: [CommonModule, BilingualNamePipe, LocalizationPipe],
  templateUrl: './list-main-accounts.html',
  styleUrl: './list-main-accounts.scss'
})

export class ListMainAccounts implements OnInit {
  accounts: MainAccountDto[] = [];
  input: MainAccountPagedRequestDto = { maxResultCount: 10, skipCount: 0 };
  constructor(private mainAccountService: MainAccountService) {


  }
  ngOnInit(): void {
    this.loadMainAccounts();
  }
  loadMainAccounts() {
    this.mainAccountService.getList(this.input).subscribe(result => {
      this.accounts = result.items;
    })
  }
}