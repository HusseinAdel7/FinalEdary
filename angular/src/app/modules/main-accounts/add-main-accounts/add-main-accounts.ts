import { MainAccountService } from 'src/app/proxy/app-services/main-accounts';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-add-main-accounts',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './add-main-accounts.html',
  styleUrl: './add-main-accounts.scss'
})
export class AddMainAccounts {

  form!: FormGroup;

  constructor(
    private mainAccountService: MainAccountService,
    public router: Router,
    private fb: FormBuilder
  ) {
    this.buildForm();
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

  // ✅ Submit
  submit() {
    if (this.form.invalid) return;

    this.mainAccountService.create(this.form.value).subscribe({
      next: () => {
        this.router.navigate(['/main-accounts']);
      },
      error: (err) => {
        console.error(err);
      }
    });
  }
}
