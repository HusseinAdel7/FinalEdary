using Edary.DTOs.MainAccounts;
using Edary.Entities.MainAccounts;
using Edary.Entities.SubAccounts;
using Edary.IAppServices;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Edary.Domain.Services.MainAccounts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Linq;
using System.Linq.Dynamic.Core;

namespace Edary.AppServices.MainAccounts
{
    public class MainAccountAppService
    : CrudAppService<
        MainAccount,
        MainAccountDto,
        string,
        MainAccountPagedRequestDto,
        CreateMainAccountDto,
        UpdateMainAccountDto>,
      IMainAccountAppService
    {
        private readonly MainAccountManager _mainAccountManager;
        private readonly IRepository<SubAccount, string> _subAccountRepository;

        public MainAccountAppService(
            IRepository<MainAccount, string> repository,
            MainAccountManager mainAccountManager,
            IRepository<SubAccount, string> subAccountRepository)
            : base(repository)
        {
            _mainAccountManager = mainAccountManager;
            _subAccountRepository = subAccountRepository;
        }

        public override async Task<MainAccountDto> CreateAsync(CreateMainAccountDto input)
        {
            var newAccountId = GuidGenerator.Create().ToString();
            var newAccountNumber = await _mainAccountManager.GenerateNewAccountNumberAsync(input.ParentMainAccountId);

            var mainAccount = new MainAccount(newAccountId, newAccountNumber)
            {
                AccountName = input.AccountName,
                AccountNameEn = input.AccountNameEn,
                Title = input.Title,
                TitleEn = input.TitleEn,
                TransferredTo = input.TransferredTo,
                TransferredToEn = input.TransferredToEn,
                IsActive = input.IsActive,
                Notes = input.Notes,
                ParentMainAccountId = input.ParentMainAccountId
            };

            var createdAccount = await Repository.InsertAsync(mainAccount, autoSave: true);

            return MapToGetOutputDto(createdAccount);
        }

        public override async Task<MainAccountDto> UpdateAsync(string id, UpdateMainAccountDto input)
        {
            var mainAccount = await Repository.GetAsync(id);



            mainAccount.AccountName = input.AccountName;
            mainAccount.AccountNameEn = input.AccountNameEn;
            mainAccount.Title = input.Title;
            mainAccount.TitleEn = input.TitleEn;
            mainAccount.TransferredTo = input.TransferredTo;
            mainAccount.TransferredToEn = input.TransferredToEn;
            mainAccount.IsActive = input.IsActive;
            mainAccount.Notes = input.Notes;
            mainAccount.ParentMainAccountId = input.ParentMainAccountId;

            var updatedAccount = await Repository.UpdateAsync(mainAccount, autoSave: true);

            return MapToGetOutputDto(updatedAccount);
        }

        public override async Task<PagedResultDto<MainAccountDto>> GetListAsync(MainAccountPagedRequestDto input)
        {
            var query = await Repository.GetQueryableAsync();

            if (!string.IsNullOrEmpty(input.Filter))
            {
                query = query.Where(ma =>
                    ma.AccountName.Contains(input.Filter) ||
                    (ma.AccountNameEn != null && ma.AccountNameEn.Contains(input.Filter)) ||
                    (ma.AccountNumber != null && ma.AccountNumber.Contains(input.Filter)) ||
                    (ma.Title != null && ma.Title.Contains(input.Filter)) ||
                    (ma.TitleEn != null && ma.TitleEn.Contains(input.Filter))
                );
            }

            if (input.IsActive.HasValue)
            {
                query = query.Where(ma => ma.IsActive == input.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                query = query.OrderBy(input.Sorting);
            }
            else
            {
                query = query.OrderByDescending(ma => ma.CreationTime);
            }

            var totalCount = await AsyncExecuter.CountAsync(query);

            query = query.PageBy(input.SkipCount, input.MaxResultCount);

            var entities = await AsyncExecuter.ToListAsync(query);
            var dtos = entities.Select(MapToGetOutputDto).ToList();

            return new PagedResultDto<MainAccountDto>(totalCount, dtos);
        }

        public virtual async Task<List<ChartOfAccountNodeDto>> GetChartOfAccountsAsync()
        {
            var mainQuery = await Repository.GetQueryableAsync();
            var mainAccounts = await AsyncExecuter.ToListAsync(mainQuery);

            var subQuery = await _subAccountRepository.GetQueryableAsync();
            var allSubAccounts = await AsyncExecuter.ToListAsync(subQuery);
            var subAccountsByMainId = allSubAccounts
                .Where(s => s.MainAccountId != null)
                .GroupBy(s => s.MainAccountId)
                .ToDictionary(g => g.Key!, g => g.OrderBy(s => s.AccountNumber).ToList());

            var mainsByParentId = mainAccounts
                .Where(m => m.ParentMainAccountId != null)
                .GroupBy(m => m.ParentMainAccountId!)
                .ToDictionary(g => g.Key, g => g.OrderBy(m => m.AccountNumber).ToList());

            var roots = mainAccounts
                .Where(m => m.ParentMainAccountId == null)
                .OrderBy(m => m.AccountNumber)
                .ToList();

            var result = roots
                .Select(root => BuildChartNode(root, mainsByParentId, subAccountsByMainId))
                .ToList();

            return result;
        }

        private static ChartOfAccountNodeDto BuildChartNode(
            MainAccount main,
            IReadOnlyDictionary<string, List<MainAccount>> mainsByParentId,
            IReadOnlyDictionary<string, List<SubAccount>> subAccountsByMainId)
        {
            List<ChartOfAccountNodeDto> children;

            if (mainsByParentId.TryGetValue(main.Id, out var childMains) && childMains.Count > 0)
            {
                // فيه main تحته → نرجعهم كعقد (ونفس المنطق يتكرر تحتهم)
                children = childMains
                    .Select(child => BuildChartNode(child, mainsByParentId, subAccountsByMainId))
                    .ToList();
            }
            else
            {
                // مفيش main تحته → نرجع الـ sub كأوراق (children فاضية)
                var subs = subAccountsByMainId.TryGetValue(main.Id, out var subList)
                    ? subList.OrderBy(s => s.AccountNumber).ToList()
                    : new List<SubAccount>();
                children = subs
                    .Select(sub => new ChartOfAccountNodeDto
                    {
                        Name = sub.AccountName ?? string.Empty,
                        AccountNumber = sub.AccountNumber ?? string.Empty,
                        Children = new List<ChartOfAccountNodeDto>()
                    })
                    .ToList();
            }

            return new ChartOfAccountNodeDto
            {
                Name = main.AccountName ?? string.Empty,
                AccountNumber = main.AccountNumber ?? string.Empty,
                Children = children
            };
        }
    }
}
