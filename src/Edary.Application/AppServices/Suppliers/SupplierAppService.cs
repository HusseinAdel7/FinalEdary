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
            // 1) Validate MainAccount
            var mainAccount = await _mainAccountRepository.FindAsync(input.MainAccountId);
            if (mainAccount == null || !mainAccount.IsActive)
            {
                throw new BusinessException("Edary:MainAccountNotFoundOrInactive")
                    .WithData("MainAccountId", input.MainAccountId);
            }

            // 2) Generate SubAccount number under this MainAccount
            var newSubAccountNumber =
                await _subAccountManager.GenerateNewAccountNumberAsync(input.MainAccountId);

            // 3) Create SubAccount with same name as Supplier
            var newSubAccountId = GuidGenerator.Create().ToString();
            var subAccount = new SubAccount(newSubAccountId, newSubAccountNumber)
            {
                MainAccountId = input.MainAccountId,
                AccountName = input.SupplierName,
                Title = input.SupplierName,
                AccountType = "Supplier",
                // Defaults to satisfy non-nullable columns
                AccountCurrency = "EGP",
                AccountCurrencyEn = "EGP",
                StandardCreditRate = "0",
                IsActive = input.IsActive ?? true,
                AccountNameEn = input.SupplierNameEn,
                TitleEn = input.SupplierNameEn,
                AccountTypeEn = "Supplier",
                Notes = input.Notes
            };

            await _subAccountRepository.InsertAsync(subAccount, autoSave: true);

            // 4) Generate SupplierCode
            var newSupplierCode = await _supplierManager.GenerateNewSupplierCodeAsync();

            // 5) Create Supplier linked to created SubAccount
            var newSupplierId = GuidGenerator.Create().ToString();
            var supplier = ObjectMapper.Map<CreateSupplierDto, Supplier>(input);

            EntityHelper.TrySetId(supplier, () => newSupplierId);
            supplier.SupplierCode = newSupplierCode;
            supplier.SubAccountId = subAccount.Id;

            var created = await Repository.InsertAsync(supplier, autoSave: true);
            return MapToGetOutputDto(created);
        }

        public override async Task<SupplierDto> UpdateAsync(string id, UpdateSupplierDto input)
        {
            var supplier = await Repository.GetAsync(id);

            // SupplierCode must not be changed by user
            supplier.SupplierName = input.SupplierName;
            supplier.Phone = input.Phone;
            supplier.Email = input.Email;
            supplier.Address = input.Address;
            supplier.TaxNumber = input.TaxNumber;
            supplier.Notes = input.Notes;
            supplier.IsActive = input.IsActive;
            supplier.SupplierNameEn = input.SupplierNameEn;

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

