using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Domain.Repositories;
using Edary.Entities.MainAccounts;
using Volo.Abp.Uow;
using Volo.Abp;

namespace Edary.Domain.Services.MainAccounts
{
    public class MainAccountManager : DomainService
    {
        private readonly IRepository<MainAccount, string> _mainAccountRepository;

        public MainAccountManager(IRepository<MainAccount, string> mainAccountRepository)
        {
            _mainAccountRepository = mainAccountRepository;
        }

        [UnitOfWork]
        public virtual async Task<string> GenerateNewAccountNumberAsync(string? parentMainAccountId)
        {
            long newAccountNumberValue;

            if (string.IsNullOrEmpty(parentMainAccountId))
            {
                var queryable = await _mainAccountRepository.GetQueryableAsync().ConfigureAwait(false);
                var maxRootAccountNumber = queryable
                    .Where(ma => ma.ParentMainAccountId == null)
                    .Select(ma => ma.AccountNumber)
                    .OrderByDescending(an => an)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(maxRootAccountNumber))
                {
                    newAccountNumberValue = 1; 
                }
                else
                {
                    newAccountNumberValue = long.Parse(maxRootAccountNumber) + 1;
                }
            }
            else
            {
                var parentAccount = await _mainAccountRepository.GetAsync(parentMainAccountId).ConfigureAwait(false);

                if (parentAccount == null || !parentAccount.IsActive)
                {
                    throw new BusinessException("Edary:ParentAccountNotFoundOrInactive", $"Parent account with ID {parentMainAccountId} not found or is inactive.");
                }

                var queryable = await _mainAccountRepository.GetQueryableAsync().ConfigureAwait(false);
                var maxChildAccountNumber = queryable
                    .Where(ma => ma.ParentMainAccountId == parentMainAccountId)
                    .Select(ma => ma.AccountNumber) .OrderByDescending(an => an)
                    .OrderByDescending(an => an)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(maxChildAccountNumber))
                {
                    newAccountNumberValue = (long.Parse(parentAccount.AccountNumber) * 10) + 1;
                }
                else
                {
                    newAccountNumberValue = long.Parse(maxChildAccountNumber) + 1;
                }
            }
            return newAccountNumberValue.ToString();
        }
    }
}

