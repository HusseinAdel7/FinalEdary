import { MainAccountService } from 'src/app/proxy/app-services/main-accounts';
import { MainAccountDto, MainAccountPagedRequestDto } from 'src/app/proxy/dtos/main-accounts';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { LocalizationPipe } from '@abp/ng.core';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';

@Component({
  selector: 'app-add-main-accounts',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LocalizationPipe],
  templateUrl: './add-main-accounts.html',
  styleUrl: './add-main-accounts.scss',
  animations: [
    trigger('cardEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(24px) scale(0.98)' }),
        animate('450ms cubic-bezier(0.34, 1.56, 0.64, 1)', style({ opacity: 1, transform: 'translateY(0) scale(1)' }))
      ])
    ]),
    trigger('formStagger', [
      transition(':enter', [
        query('.form-group-anim', [
          style({ opacity: 0, transform: 'translateX(-20px)' }),
          stagger(50, [
            animate('300ms cubic-bezier(0.25, 0.46, 0.45, 0.94)', style({ opacity: 1, transform: 'translateX(0)' }))
          ])
        ], { optional: true })
      ])
    ]),
    trigger('headerEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-10px)' }),
        animate('350ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class AddMainAccounts implements OnInit {

  form!: FormGroup;
  parentAccounts: MainAccountDto[] = [];
  /** Set when editing; null when creating */
  editingId: string | null = null;

  constructor(
    private mainAccountService: MainAccountService,
    public router: Router,
    private route: ActivatedRoute,
    private fb: FormBuilder
  ) {
    this.buildForm();
  }

  ngOnInit(): void {
    this.loadParentMainAccounts();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editingId = id;
      this.loadMainAccount(id);
    }
  }

  loadMainAccount(id: string): void {
    this.mainAccountService.get(id).subscribe({
      next: (account) => {
        this.form.patchValue({
          accountName: account.accountName ?? '',
          accountNameEn: account.accountNameEn ?? '',
          title: account.title ?? '',
          titleEn: account.titleEn ?? '',
          transferredTo: account.transferredTo ?? '',
          transferredToEn: account.transferredToEn ?? '',
          isActive: account.isActive ?? true,
          notes: account.notes ?? '',
          parentMainAccountId: account.parentMainAccountId ?? null
        });
      },
      error: (err) => console.error(err)
    });
  }

  loadParentMainAccounts(): void {
    const input: MainAccountPagedRequestDto = {
      skipCount: 0,
      maxResultCount: 1000
    };
    this.mainAccountService.getList(input).subscribe(result => {
      this.parentAccounts = result.items ?? [];
    });
  }

  // ✅ Build Form مطابق للـ DTO
  buildForm() {
    this.form = this.fb.group({
      accountName: ['', Validators.required],
      accountNameEn: [''],
      title: [''],
      titleEn: [''],
      transferredTo: [''],
      transferredToEn: [''],
      isActive: [true],
      notes: [''],
      parentMainAccountId: [null]
    });
  }

  submit(): void {
    if (this.form.invalid) return;

    if (this.editingId) {
      this.mainAccountService.update(this.editingId, this.form.value).subscribe({
        next: () => this.router.navigate(['/main-accounts']),
        error: (err) => console.error(err)
      });
    } else {
      this.mainAccountService.create(this.form.value).subscribe({
        next: () => this.router.navigate(['/main-accounts']),
        error: (err) => console.error(err)
      });
    }
  }
}
