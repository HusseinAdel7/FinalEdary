using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Edary.Domain.Services.Invoices;
using Edary.DTOs.Invoices;
using Edary.Entities.Invoices;
using Edary.Entities.Items;
using Edary.Entities.JournalEntries;
using Edary.Entities.Suppliers;
using Edary.Entities.Warehouses;
using Edary.IAppServices;
using Edary.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;

namespace Edary.AppServices.Invoices
{
    [Authorize(EdaryPermissions.Invoices.Default)]
    public class InvoiceAppService :
        CrudAppService<
            Invoice,
            InvoiceDto,
            string,
            InvoicePagedRequestDto,
            CreateInvoiceDto,
            UpdateInvoiceDto>,
        IInvoiceAppService
    {
        private readonly InvoiceManager _invoiceManager;
        private readonly IRepository<InvoiceDetail, string> _invoiceDetailRepository;
        private readonly IRepository<JournalEntry, string> _journalEntryRepository;
        private readonly IRepository<Warehouse, string> _warehouseRepository;
        private readonly IRepository<Supplier, string> _supplierRepository;
        private readonly IRepository<Item, string> _itemRepository;

        public InvoiceAppService(
            IRepository<Invoice, string> invoiceRepository,
            IRepository<InvoiceDetail, string> invoiceDetailRepository,
            IRepository<JournalEntry, string> journalEntryRepository,
            IRepository<Warehouse, string> warehouseRepository,
            IRepository<Supplier, string> supplierRepository,
            IRepository<Item, string> itemRepository,
            InvoiceManager invoiceManager)
            : base(invoiceRepository)
        {
            _invoiceManager = invoiceManager;
            _invoiceDetailRepository = invoiceDetailRepository;
            _journalEntryRepository = journalEntryRepository;
            _warehouseRepository = warehouseRepository;
            _supplierRepository = supplierRepository;
            _itemRepository = itemRepository;
        }

        public override async Task<InvoiceDto> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف الفاتورة مطلوب");
            var query = await Repository.WithDetailsAsync(x => x.InvoiceDetails);
            var invoice = query.FirstOrDefault(x => x.Id == id);
            return ObjectMapper.Map<Invoice, InvoiceDto>(invoice);
        }

        [Authorize(EdaryPermissions.Invoices.List)]
        public override async Task<PagedResultDto<InvoiceDto>> GetListAsync(InvoicePagedRequestDto input)
        {
            var query = await Repository.WithDetailsAsync(x => x.InvoiceDetails);

            query = query
                .WhereIf(!input.Filter.IsNullOrWhiteSpace(),
                    invoice => invoice.InvoiceNumber.Contains(input.Filter) ||
                               invoice.Notes.Contains(input.Filter) ||
                               invoice.Currency.Contains(input.Filter))
                .WhereIf(!input.InvoiceType.IsNullOrWhiteSpace(),
                    invoice => invoice.InvoiceType.Contains(input.InvoiceType))
                .WhereIf(!input.PaymentStatus.IsNullOrWhiteSpace(),
                    invoice => invoice.PaymentStatus.Contains(input.PaymentStatus));

            var totalCount = await AsyncExecuter.CountAsync(query);

            query = query.OrderBy(input.Sorting.IsNullOrWhiteSpace()
                    ? "InvoiceNumber asc"
                    : input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var invoices = await AsyncExecuter.ToListAsync(query);

            return new PagedResultDto<InvoiceDto>(
                totalCount,
                ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(invoices));
        }

        [Authorize(EdaryPermissions.Invoices.Create)]
        public override async Task<InvoiceDto> CreateAsync(CreateInvoiceDto input)
        {
            await ValidateInvoiceHeaderAsync(input.WarehouseId, input.SupplierId, input.TotalAmount, input.Discount, input.TaxAmount);
            await ValidateInvoiceDetailsAsync(input.InvoiceDetails);

            var invoiceNumber = await _invoiceManager.GenerateNewInvoiceNumberAsync();

            var journalEntryId = GuidGenerator.Create().ToString();
            var journalEntry = new JournalEntry(journalEntryId)
            {
                Currency = string.IsNullOrWhiteSpace(input.Currency) ? null : input.Currency.Trim(),
                ExchangeRate = 1,
                Notes = $"Auto generated from invoice {invoiceNumber}",
                CurrencyEn = string.IsNullOrWhiteSpace(input.Currency) ? null : input.Currency.Trim()
            };
            await _journalEntryRepository.InsertAsync(journalEntry, autoSave: true);

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                InvoiceType = input.InvoiceType?.Trim(),
                SupplierId = string.IsNullOrWhiteSpace(input.SupplierId) ? null : input.SupplierId.Trim(),
                WarehouseId = input.WarehouseId.Trim(),
                Currency = string.IsNullOrWhiteSpace(input.Currency) ? null : input.Currency.Trim(),
                TotalAmount = input.TotalAmount,
                Discount = input.Discount,
                TaxAmount = input.TaxAmount,
                JournalEntryId = journalEntry.Id,
                PaymentStatus = string.IsNullOrWhiteSpace(input.PaymentStatus) ? null : input.PaymentStatus.Trim(),
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
                InvoiceDetails = new HashSet<InvoiceDetail>()
            };
            EntityHelper.TrySetId(invoice, () => GuidGenerator.Create().ToString());

            foreach (var d in input.InvoiceDetails)
            {
                var detail = new InvoiceDetail
                {
                    InvoiceId = invoice.Id,
                    ItemId = d.ItemId.Trim(),
                    UnitName = d.UnitName?.Trim(),
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Discount = d.Discount,
                    TaxRate = d.TaxRate
                };
                EntityHelper.TrySetId(detail, () => GuidGenerator.Create().ToString());
                invoice.InvoiceDetails.Add(detail);
            }

            var created = await Repository.InsertAsync(invoice, autoSave: true);
            return ObjectMapper.Map<Invoice, InvoiceDto>(created);
        }

        [Authorize(EdaryPermissions.Invoices.Update)]
        public override async Task<InvoiceDto> UpdateAsync(string id, UpdateInvoiceDto input)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف الفاتورة مطلوب");

            await ValidateInvoiceHeaderAsync(input.WarehouseId, input.SupplierId, input.TotalAmount, input.Discount, input.TaxAmount);
            await ValidateInvoiceDetailsForUpdateAsync(input.InvoiceDetails);

            var invoice = await Repository.GetAsync(id);
            invoice.InvoiceType = input.InvoiceType?.Trim();
            invoice.SupplierId = string.IsNullOrWhiteSpace(input.SupplierId) ? null : input.SupplierId.Trim();
            invoice.WarehouseId = input.WarehouseId.Trim();
            invoice.Currency = string.IsNullOrWhiteSpace(input.Currency) ? null : input.Currency.Trim();
            invoice.TotalAmount = input.TotalAmount;
            invoice.Discount = input.Discount;
            invoice.TaxAmount = input.TaxAmount;
            invoice.PaymentStatus = string.IsNullOrWhiteSpace(input.PaymentStatus) ? null : input.PaymentStatus.Trim();
            invoice.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();

            foreach (var detailDto in input.InvoiceDetails)
            {
                if (string.IsNullOrEmpty(detailDto.Id))
                {
                    var newDetail = new InvoiceDetail
                    {
                        InvoiceId = id,
                        ItemId = detailDto.ItemId.Trim(),
                        UnitName = detailDto.UnitName?.Trim(),
                        Quantity = detailDto.Quantity,
                        UnitPrice = detailDto.UnitPrice,
                        Discount = detailDto.Discount,
                        TaxRate = detailDto.TaxRate
                    };
                    EntityHelper.TrySetId(newDetail, () => GuidGenerator.Create().ToString());
                    await _invoiceDetailRepository.InsertAsync(newDetail);
                }
                else
                {
                    var existingDetail = await _invoiceDetailRepository.GetAsync(detailDto.Id);
                    existingDetail.ItemId = detailDto.ItemId.Trim();
                    existingDetail.UnitName = detailDto.UnitName?.Trim();
                    existingDetail.Quantity = detailDto.Quantity;
                    existingDetail.UnitPrice = detailDto.UnitPrice;
                    existingDetail.Discount = detailDto.Discount;
                    existingDetail.TaxRate = detailDto.TaxRate;
                    await _invoiceDetailRepository.UpdateAsync(existingDetail);
                }
            }

            var existingDetailIds = await (await _invoiceDetailRepository.GetQueryableAsync())
                .Where(x => x.InvoiceId == id)
                .Select(x => x.Id)
                .ToListAsync();
            var inputDetailIds = input.InvoiceDetails
                .Where(x => !string.IsNullOrEmpty(x.Id))
                .Select(x => x.Id)
                .ToHashSet();
            var detailIdsToRemove = existingDetailIds.Where(x => !inputDetailIds.Contains(x)).ToList();
            foreach (var detailId in detailIdsToRemove)
                await _invoiceDetailRepository.DeleteAsync(detailId);

            var updated = await Repository.UpdateAsync(invoice, autoSave: true);
            return ObjectMapper.Map<Invoice, InvoiceDto>(updated);
        }

        [Authorize(EdaryPermissions.Invoices.Delete)]
        public override async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف الفاتورة مطلوب");
            var detailIdsToDelete = await (await _invoiceDetailRepository.GetQueryableAsync())
                .Where(x => x.InvoiceId == id)
                .Select(x => x.Id)
                .ToListAsync();
            await _invoiceDetailRepository.DeleteManyAsync(detailIdsToDelete);
            await Repository.DeleteAsync(id);
        }

        private async Task ValidateInvoiceHeaderAsync(
            string warehouseId,
            string supplierId,
            decimal? totalAmount,
            decimal? discount,
            decimal? taxAmount)
        {
            if (string.IsNullOrWhiteSpace(warehouseId))
                throw new AbpValidationException("المستودع مطلوب");
            var warehouse = await _warehouseRepository.FindAsync(warehouseId.Trim());
            if (warehouse == null)
                throw new EntityNotFoundException(typeof(Warehouse), warehouseId);

            if (!string.IsNullOrWhiteSpace(supplierId))
            {
                var supplier = await _supplierRepository.FindAsync(supplierId.Trim());
                if (supplier == null)
                    throw new EntityNotFoundException(typeof(Supplier), supplierId);
            }

            if (totalAmount.HasValue && totalAmount.Value < 0)
                throw new AbpValidationException("المبلغ الإجمالي لا يمكن أن يكون سالباً");
            if (discount.HasValue && discount.Value < 0)
                throw new AbpValidationException("الخصم لا يمكن أن يكون سالباً");
            if (taxAmount.HasValue && taxAmount.Value < 0)
                throw new AbpValidationException("قيمة الضريبة لا يمكن أن تكون سالبة");
        }

        private async Task ValidateInvoiceDetailsAsync(ICollection<CreateInvoiceDetailDto> details)
        {
            if (details == null || details.Count == 0)
                throw new AbpValidationException("يجب أن تحتوي الفاتورة على سطر واحد على الأقل");
            var index = 0;
            foreach (var d in details)
            {
                if (string.IsNullOrWhiteSpace(d.ItemId))
                    throw new AbpValidationException($"السطر {index + 1}: الصنف مطلوب.");
                var item = await _itemRepository.FindAsync(d.ItemId.Trim());
                if (item == null)
                    throw new AbpValidationException($"السطر {index + 1}: الصنف غير موجود.");
                if (string.IsNullOrWhiteSpace(d.UnitName))
                    throw new AbpValidationException($"السطر {index + 1}: اسم الوحدة مطلوب.");
                if (d.Quantity <= 0)
                    throw new AbpValidationException($"السطر {index + 1}: الكمية يجب أن تكون أكبر من صفر.");
                if (d.UnitPrice < 0)
                    throw new AbpValidationException($"السطر {index + 1}: سعر الوحدة لا يمكن أن يكون سالباً.");
                if (d.Discount.HasValue && d.Discount.Value < 0)
                    throw new AbpValidationException($"السطر {index + 1}: الخصم لا يمكن أن يكون سالباً.");
                if (d.TaxRate.HasValue && (d.TaxRate.Value < 0 || d.TaxRate.Value > 100))
                    throw new AbpValidationException($"السطر {index + 1}: نسبة الضريبة يجب أن تكون بين 0 و 100.");
                index++;
            }
        }

        private async Task ValidateInvoiceDetailsForUpdateAsync(ICollection<UpdateInvoiceDetailDto> details)
        {
            if (details == null || details.Count == 0)
                throw new AbpValidationException("يجب أن تحتوي الفاتورة على سطر واحد على الأقل");
            var index = 0;
            foreach (var d in details)
            {
                if (string.IsNullOrWhiteSpace(d.ItemId))
                    throw new AbpValidationException($"السطر {index + 1}: الصنف مطلوب.");
                var item = await _itemRepository.FindAsync(d.ItemId.Trim());
                if (item == null)
                    throw new AbpValidationException($"السطر {index + 1}: الصنف غير موجود.");
                if (string.IsNullOrWhiteSpace(d.UnitName))
                    throw new AbpValidationException($"السطر {index + 1}: اسم الوحدة مطلوب.");
                if (d.Quantity <= 0)
                    throw new AbpValidationException($"السطر {index + 1}: الكمية يجب أن تكون أكبر من صفر.");
                if (d.UnitPrice < 0)
                    throw new AbpValidationException($"السطر {index + 1}: سعر الوحدة لا يمكن أن يكون سالباً.");
                if (d.Discount.HasValue && d.Discount.Value < 0)
                    throw new AbpValidationException($"السطر {index + 1}: الخصم لا يمكن أن يكون سالباً.");
                if (d.TaxRate.HasValue && (d.TaxRate.Value < 0 || d.TaxRate.Value > 100))
                    throw new AbpValidationException($"السطر {index + 1}: نسبة الضريبة يجب أن تكون بين 0 و 100.");
                index++;
            }
        }
    }
}

