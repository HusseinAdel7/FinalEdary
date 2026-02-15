using Edary.DTOs.AccountStatments;
using Edary.Entities.JournalEntries;
using Edary.Entities.MainAccounts;
using Edary.Entities.SubAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Edary.Services.AccountStatments
{
    public class AccountStatementManager : DomainService
    {
        private readonly IRepository<SubAccount, string> _subRepo;
        private readonly IRepository<MainAccount, string> _mainRepo;
        private readonly IRepository<JournalEntryDetail, string> _detailRepo;
        private readonly IRepository<JournalEntry, string> _journalRepo;

        public AccountStatementManager(
            IRepository<SubAccount, string> subRepo,
            IRepository<MainAccount, string> mainRepo,
            IRepository<JournalEntryDetail, string> detailRepo,
            IRepository<JournalEntry, string> journalRepo)
        {
            _subRepo = subRepo;
            _mainRepo = mainRepo;
            _detailRepo = detailRepo;
            _journalRepo = journalRepo;
        }

        public async Task<List<AccountStatementLineDto>> GenerateByAccountAsync(
            string accountId,
            DateTime fromDate,
            DateTime toDate)
        {
            return await GenerateInternalAsync(fromDate, toDate, accountId);
        }

        public async Task<List<AccountStatementLineDto>> GenerateAllAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            return await GenerateInternalAsync(fromDate, toDate, null);
        }

        private async Task<List<AccountStatementLineDto>> GenerateInternalAsync(
       DateTime fromDate,
       DateTime toDate,
       string accountId = null)
        {
            var subQuery = await _subRepo.GetQueryableAsync();
            var mainQuery = await _mainRepo.GetQueryableAsync();
            var detailQuery = await _detailRepo.GetQueryableAsync();
            var journalQuery = await _journalRepo.GetQueryableAsync();

            // check account type if provided
            List<string> subAccountIds = null;

            if (!string.IsNullOrWhiteSpace(accountId))
            {
                var isMain = await _mainRepo.AnyAsync(x => x.Id == accountId);

                if (isMain)
                {
                    var subQueryable = await _subRepo.GetQueryableAsync();

                    subAccountIds = await AsyncExecuter.ToListAsync(
                        subQueryable
                            .Where(x => x.MainAccountId == accountId)
                            .Select(x => x.Id)
                    );

                }
                else
                {
                    subAccountIds = new List<string> { accountId };
                }
            }

            var movementsQuery =
                from d in detailQuery
                join j in journalQuery on d.JournalEntryId equals j.Id
                join s in subQuery on d.SubAccountId equals s.Id
                join m in mainQuery on s.MainAccountId equals m.Id
                where j.CreationTime >= fromDate
                      && j.CreationTime <= toDate
                select new
                {
                    MainId = m.Id,
                    MainName = m.AccountName,
                    SubId = s.Id,
                    SubName = s.AccountName,
                    j.CreationTime,
                    d.Description,
                    d.Debit,
                    d.Credit
                };

            // apply account filter
            if (subAccountIds != null && subAccountIds.Any())
            {
                movementsQuery = movementsQuery
                    .Where(x => subAccountIds.Contains(x.SubId));
            }

            var movements = await AsyncExecuter.ToListAsync(
                movementsQuery
                    .OrderBy(x => x.MainName)
                    .ThenBy(x => x.CreationTime)
            );

            var result = new List<AccountStatementLineDto>();

            var grouped = movements.GroupBy(x => x.MainName);

            foreach (var group in grouped)
            {
                decimal running = 0;

                foreach (var item in group)
                {
                    running += item.Credit - item.Debit;

                    result.Add(new AccountStatementLineDto
                    {
                        MainAccountName = group.Key,
                        SubAccountName = item.SubName,
                        EntryDate = item.CreationTime,
                        Description = item.Description,
                        Debit = item.Debit,
                        Credit = item.Credit,
                        RunningBalance = running,
                        SortOrder = 1
                    });
                }

                result.Add(new AccountStatementLineDto
                {
                    MainAccountName = group.Key,
                    Description = "إجمالي الفترة",
                    Debit = group.Sum(x => x.Debit),
                    Credit = group.Sum(x => x.Credit),
                    SortOrder = 2
                });

                result.Add(new AccountStatementLineDto
                {
                    MainAccountName = group.Key,
                    Description = running > 0 ? "رصيد ختامي دائن"
                                : running < 0 ? "رصيد ختامي مدين"
                                : "رصيد ختامي",
                    RunningBalance = Math.Abs(running),
                    SortOrder = 3
                });
            }

            return result;
        }
    }
}
