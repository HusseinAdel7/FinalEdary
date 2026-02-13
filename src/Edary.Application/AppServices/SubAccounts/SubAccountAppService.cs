using Edary.DTOs.SubAccounts;
using Edary.Entities.SubAccounts;
using Edary.IAppServices;
using Edary.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Edary.Domain.Services.SubAccounts;
using System.Threading.Tasks;
using System.Linq;
using Volo.Abp.Domain.Entities;
using System.Linq.Dynamic.Core;
using Volo.Abp.Application.Dtos;
using Edary.Entities.MainAccounts;
using Volo.Abp;
using Volo.Abp.Validation;

namespace Edary.AppServices.SubAccounts
{
    [Authorize(EdaryPermissions.SubAccounts.Default)]
    public class SubAccountAppService
    : CrudAppService<
        SubAccount,
        SubAccountDto,
        string,
        SubAccountPagedRequestDto,
        CreateSubAccountDto,
        UpdateSubAccountDto>,
      ISubAccountAppService
    {
        private readonly SubAccountManager _subAccountManager;
        private readonly IRepository<MainAccount, string> _mainAccountRepository;

        public SubAccountAppService(
            IRepository<SubAccount, string> repository,
            SubAccountManager subAccountManager,
            IRepository<MainAccount, string> mainAccountRepository)
            : base(repository)
        {
            _subAccountManager = subAccountManager;
            _mainAccountRepository = mainAccountRepository;
        }

        [Authorize(EdaryPermissions.SubAccounts.Create)]
        public override async Task<SubAccountDto> CreateAsync(CreateSubAccountDto input)
        {
            // Validate MainAccount exists and is active
            var mainAccount = await _mainAccountRepository.FindAsync(input.MainAccountId);
            if (mainAccount == null)
            {
                throw new EntityNotFoundException(typeof(MainAccount), input.MainAccountId);
            }

            if (!mainAccount.IsActive)
            {
                throw new BusinessException("Edary:MainAccountInactive")
                    .WithData("MainAccountId", input.MainAccountId);
            }

            // Validate decimal ranges (business logic validation)
            if (input.CreditAmount.HasValue && input.CreditAmount.Value < 0)
            {
                throw new AbpValidationException("مبلغ الائتمان لا يمكن أن يكون سالباً");
            }

            if (input.Commission.HasValue && input.Commission.Value < 0)
            {
                throw new AbpValidationException("العمولة لا يمكن أن تكون سالبة");
            }

            if (input.Percentage.HasValue && (input.Percentage.Value < 0 || input.Percentage.Value > 100))
            {
                throw new AbpValidationException("النسبة المئوية يجب أن تكون بين 0 و 100");
            }

            // Validate required strings are not empty/whitespace
            if (string.IsNullOrWhiteSpace(input.AccountName))
            {
                throw new AbpValidationException("اسم الحساب مطلوب");
            }

            if (string.IsNullOrWhiteSpace(input.Title))
            {
                throw new AbpValidationException("العنوان مطلوب");
            }

            if (string.IsNullOrWhiteSpace(input.AccountType))
            {
                throw new AbpValidationException("نوع الحساب مطلوب");
            }

            if (string.IsNullOrWhiteSpace(input.StandardCreditRate))
            {
                throw new AbpValidationException("معدل الائتمان القياسي مطلوب");
            }

            if (string.IsNullOrWhiteSpace(input.AccountCurrency))
            {
                throw new AbpValidationException("عملة الحساب مطلوبة");
            }

            var newAccountId = GuidGenerator.Create().ToString();
            var newAccountNumber = await _subAccountManager.GenerateNewAccountNumberAsync(input.MainAccountId);

            var subAccount = new SubAccount(newAccountId, newAccountNumber)
            {
                AccountName = input.AccountName.Trim(),
                MainAccountId = input.MainAccountId,
                Title = input.Title.Trim(),
                AccountType = input.AccountType.Trim(),
                CreditAmount = input.CreditAmount,
                StandardCreditRate = input.StandardCreditRate.Trim(),
                Commission = input.Commission,
                Percentage = input.Percentage,
                AccountCurrency = input.AccountCurrency.Trim(),
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
                IsActive = input.IsActive ?? true,
                AccountNameEn = string.IsNullOrWhiteSpace(input.AccountNameEn) ? null : input.AccountNameEn.Trim(),
                TitleEn = string.IsNullOrWhiteSpace(input.TitleEn) ? null : input.TitleEn.Trim(),
                AccountTypeEn = string.IsNullOrWhiteSpace(input.AccountTypeEn) ? null : input.AccountTypeEn.Trim(),
                AccountCurrencyEn = string.IsNullOrWhiteSpace(input.AccountCurrencyEn) ? null : input.AccountCurrencyEn.Trim()
            };

            var createdAccount = await Repository.InsertAsync(subAccount, autoSave: true);

            return MapToGetOutputDto(createdAccount);
        }

        [Authorize(EdaryPermissions.SubAccounts.Update)]
        public override async Task<SubAccountDto> UpdateAsync(string id, UpdateSubAccountDto input)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new AbpValidationException("معرّف الحساب الفرعي مطلوب");
            }

            var subAccount = await Repository.GetAsync(id);

            // Validate MainAccount exists and is active (if changed)
            if (subAccount.MainAccountId != input.MainAccountId)
            {
                var mainAccount = await _mainAccountRepository.FindAsync(input.MainAccountId);
                if (mainAccount == null)
                {
                    throw new EntityNotFoundException(typeof(MainAccount), input.MainAccountId);
                }

                if (!mainAccount.IsActive)
                {
                    throw new BusinessException("Edary:MainAccountInactive")
                        .WithData("MainAccountId", input.MainAccountId);
                }
            }

            // Validate decimal ranges (business logic validation)
            if (input.CreditAmount.HasValue && input.CreditAmount.Value < 0)
            {
                throw new AbpValidationException("مبلغ الائتمان لا يمكن أن يكون سالباً");
            }

            if (input.Commission.HasValue && input.Commission.Value < 0)
            {
                throw new AbpValidationException("العمولة لا يمكن أن تكون سالبة");
            }

            if (input.Percentage.HasValue && (input.Percentage.Value < 0 || input.Percentage.Value > 100))
            {
                throw new AbpValidationException("النسبة المئوية يجب أن تكون بين 0 و 100");
            }

            // Validate required strings are not empty/whitespace
            if (string.IsNullOrWhiteSpace(input.AccountName))
            {
                throw new AbpValidationException("اسم الحساب مطلوب");
            }

            if (string.IsNullOrWhiteSpace(input.Title))
            {
                throw new AbpValidationException("العنوان مطلوب");
            }

            if (string.IsNullOrWhiteSpace(input.AccountType))
            {
                throw new AbpValidationException("نوع الحساب مطلوب");
            }

            if (string.IsNullOrWhiteSpace(input.StandardCreditRate))
            {
                throw new AbpValidationException("معدل الائتمان القياسي مطلوب");
            }

            if (string.IsNullOrWhiteSpace(input.AccountCurrency))
            {
                throw new AbpValidationException("عملة الحساب مطلوبة");
            }

            subAccount.AccountName = input.AccountName.Trim();
            subAccount.MainAccountId = input.MainAccountId;
            subAccount.Title = input.Title.Trim();
            subAccount.AccountType = input.AccountType.Trim();
            subAccount.CreditAmount = input.CreditAmount;
            subAccount.StandardCreditRate = input.StandardCreditRate.Trim();
            subAccount.Commission = input.Commission;
            subAccount.Percentage = input.Percentage;
            subAccount.AccountCurrency = input.AccountCurrency.Trim();
            subAccount.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();
            subAccount.IsActive = input.IsActive;
            subAccount.AccountNameEn = string.IsNullOrWhiteSpace(input.AccountNameEn) ? null : input.AccountNameEn.Trim();
            subAccount.TitleEn = string.IsNullOrWhiteSpace(input.TitleEn) ? null : input.TitleEn.Trim();
            subAccount.AccountTypeEn = string.IsNullOrWhiteSpace(input.AccountTypeEn) ? null : input.AccountTypeEn.Trim();
            subAccount.AccountCurrencyEn = string.IsNullOrWhiteSpace(input.AccountCurrencyEn) ? null : input.AccountCurrencyEn.Trim();

            var updatedAccount = await Repository.UpdateAsync(subAccount, autoSave: true);

            return MapToGetOutputDto(updatedAccount);
        }

        [Authorize(EdaryPermissions.SubAccounts.List)]
        public override async Task<PagedResultDto<SubAccountDto>> GetListAsync(SubAccountPagedRequestDto input)
        {
            var query = await Repository.GetQueryableAsync();

            if (!string.IsNullOrEmpty(input.Filter))
            {
                query = query.Where(sa =>
                    sa.AccountName.Contains(input.Filter) ||
                    (sa.AccountNameEn != null && sa.AccountNameEn.Contains(input.Filter)) ||
                    (sa.AccountNumber != null && sa.AccountNumber.Contains(input.Filter)) ||
                    (sa.Title != null && sa.Title.Contains(input.Filter)) ||
                    (sa.TitleEn != null && sa.TitleEn.Contains(input.Filter))
                );
            }

            if (input.IsActive.HasValue)
            {
                query = query.Where(sa => sa.IsActive == input.IsActive.Value);
            }

            if (!string.IsNullOrEmpty(input.MainAccountId))
            {
                query = query.Where(sa => sa.MainAccountId == input.MainAccountId);
            }

            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                query = query.OrderBy(input.Sorting);
            }
            else
            {
                query = query.OrderByDescending(sa => sa.CreationTime); 
            }

            var totalCount = await AsyncExecuter.CountAsync(query);

            query = query.PageBy(input.SkipCount, input.MaxResultCount);

            var entities = await AsyncExecuter.ToListAsync(query);
            var dtos = entities.Select(MapToGetOutputDto).ToList();

            return new PagedResultDto<SubAccountDto>(totalCount, dtos);
        }
    }
}
