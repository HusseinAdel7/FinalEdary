import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { SupplierService } from 'src/app/proxy/app-services/suppliers';
import { SupplierDto, SupplierPagedRequestDto } from 'src/app/proxy/dtos/suppliers';
import { BilingualNamePipe } from 'src/app/shared/pipes/bilingual-name.pipe';
import { LocalizationPipe } from '@abp/ng.core';

@Component({
  selector: 'app-list-supplies',
  standalone: true,
  imports: [CommonModule, BilingualNamePipe, LocalizationPipe],
  templateUrl: './list-supplies.html',
  styleUrl: './list-supplies.scss'
})
export class ListSupplies implements OnInit {
  suppliers: SupplierDto[] = [];
  input: SupplierPagedRequestDto = { maxResultCount: 10, skipCount: 0 };
  constructor(private supplierService: SupplierService) {
  }
  ngOnInit(): void {
    this.loadSuppliers();
    console.log(this.suppliers);
  }
  loadSuppliers() {
    this.supplierService.getList(this.input).subscribe(result => {
      this.suppliers = result.items
    })

  }
}
