using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Edary.Domain.Services.Suppliers;
using Edary.Domain.Services.SubAccounts;
using Edary.DTOs.Suppliers;
using Edary.Entities.MainAccounts;
using Edary.Entities.SubAccounts;
using Edary.Entities.Suppliers;
using Edary.IAppServices;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;

namespace Edary.AppServices.Suppliers
{
    public class SupplierAppService :
        CrudAppService<
            Supplier,
            SupplierDto,
            string,
            SupplierPagedRequestDto,
            CreateSupplierDto,
            UpdateSupplierDto>,
        ISupplierAppService
    {
        private readonly SupplierManager _supplierManager;
        private readonly SubAccountManager _subAccountManager;
        private readonly IRepository<SubAccount, string> _subAccountRepository;
        private readonly IRepository<MainAccount, string> _mainAccountRepository;

        public SupplierAppService(
            IRepository<Supplier, string> repository,
            SupplierManager supplierManager,
            SubAccountManager subAccountManager,
            IRepository<SubAccount, string> subAccountRepository,
            IRepository<MainAccount, string> mainAccountRepository)
            : base(repository)
        {
            _supplierManager = supplierManager;
            _subAccountManager = subAccountManager;
            _subAccountRepository = subAccountRepository;
            _mainAccountRepository = mainAccountRepository;
        }

        public override async Task<SupplierDto> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new AbpValidationException("معرّف المورد مطلوب");
            }

            return await base.GetAsync(id);
        }

        public override async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new AbpValidationException("معرّف المورد مطلوب");
            }

            await base.DeleteAsync(id);
        }

        public override async Task<SupplierDto> CreateAsync(CreateSupplierDto input)
        {
            if (string.IsNullOrWhiteSpace(input.SupplierName))
                throw new AbpValidationException("اسم المورد مطلوب");
            if (string.IsNullOrWhiteSpace(input.MainAccountId))
                throw new AbpValidationException("معرّف الحساب الرئيسي مطلوب");

            var mainAccount = await _mainAccountRepository.FindAsync(input.MainAccountId.Trim());
            if (mainAccount == null || !mainAccount.IsActive)
                throw new BusinessException("Edary:MainAccountNotFoundOrInactive")
                    .WithData("MainAccountId", input.MainAccountId);

            var newSubAccountNumber =
                await _subAccountManager.GenerateNewAccountNumberAsync(input.MainAccountId.Trim());

            var newSubAccountId = GuidGenerator.Create().ToString();
            var supplierName = input.SupplierName.Trim();
            var subAccount = new SubAccount(newSubAccountId, newSubAccountNumber)
            {
                MainAccountId = input.MainAccountId.Trim(),
                AccountName = supplierName,
                Title = supplierName,
                AccountType = "Supplier",
                AccountCurrency = "EGP",
                AccountCurrencyEn = "EGP",
                StandardCreditRate = "0",
                IsActive = input.IsActive ?? true,
                AccountNameEn = string.IsNullOrWhiteSpace(input.SupplierNameEn) ? null : input.SupplierNameEn.Trim(),
                TitleEn = string.IsNullOrWhiteSpace(input.SupplierNameEn) ? null : input.SupplierNameEn.Trim(),
                AccountTypeEn = "Supplier",
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim()
            };

            await _subAccountRepository.InsertAsync(subAccount, autoSave: true);

            var newSupplierCode = await _supplierManager.GenerateNewSupplierCodeAsync();
            var newSupplierId = GuidGenerator.Create().ToString();
            var supplier = new Supplier
            {
                Id = newSupplierId,
                SupplierCode = newSupplierCode,
                SubAccountId = subAccount.Id,
                SupplierName = supplierName,
                Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim(),
                Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim(),
                Address = string.IsNullOrWhiteSpace(input.Address) ? null : input.Address.Trim(),
                TaxNumber = string.IsNullOrWhiteSpace(input.TaxNumber) ? null : input.TaxNumber.Trim(),
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
                IsActive = input.IsActive ?? true,
                SupplierNameEn = string.IsNullOrWhiteSpace(input.SupplierNameEn) ? null : input.SupplierNameEn.Trim()
            };

            var created = await Repository.InsertAsync(supplier, autoSave: true);
            return MapToGetOutputDto(created);
        }

        public override async Task<SupplierDto> UpdateAsync(string id, UpdateSupplierDto input)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف المورد مطلوب");
            if (string.IsNullOrWhiteSpace(input.SupplierName))
                throw new AbpValidationException("اسم المورد مطلوب");

            var supplier = await Repository.GetAsync(id);

            supplier.SupplierName = input.SupplierName.Trim();
            supplier.Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim();
            supplier.Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();
            supplier.Address = string.IsNullOrWhiteSpace(input.Address) ? null : input.Address.Trim();
            supplier.TaxNumber = string.IsNullOrWhiteSpace(input.TaxNumber) ? null : input.TaxNumber.Trim();
            supplier.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();
            supplier.IsActive = input.IsActive;
            supplier.SupplierNameEn = string.IsNullOrWhiteSpace(input.SupplierNameEn) ? null : input.SupplierNameEn.Trim();

            var updated = await Repository.UpdateAsync(supplier, autoSave: true);
            return MapToGetOutputDto(updated);
        }

        public override async Task<PagedResultDto<SupplierDto>> GetListAsync(SupplierPagedRequestDto input)
        {
            var query = await Repository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                query = query.Where(s =>
                    s.SupplierCode.Contains(input.Filter) ||
                    s.SupplierName.Contains(input.Filter) ||
                    (s.SupplierNameEn != null && s.SupplierNameEn.Contains(input.Filter)) ||
                    (s.Phone != null && s.Phone.Contains(input.Filter)) ||
                    (s.Email != null && s.Email.Contains(input.Filter)) ||
                    (s.TaxNumber != null && s.TaxNumber.Contains(input.Filter))
                );
            }

            if (input.IsActive.HasValue)
            {
                query = query.Where(s => s.IsActive == input.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.SubAccountId))
            {
                query = query.Where(s => s.SubAccountId == input.SubAccountId);
            }

            query = !string.IsNullOrWhiteSpace(input.Sorting)
                ? query.OrderBy(input.Sorting)
                : query.OrderByDescending(s => s.CreationTime);

            var totalCount = await AsyncExecuter.CountAsync(query);
            query = query.PageBy(input.SkipCount, input.MaxResultCount);

            var entities = await AsyncExecuter.ToListAsync(query);
            var dtos = entities.Select(MapToGetOutputDto).ToList();

            return new PagedResultDto<SupplierDto>(totalCount, dtos);
        }
    }
}

