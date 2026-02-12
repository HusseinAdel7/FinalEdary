using Edary.DTOs.JournalEntries;
using Edary.Entities.JournalEntries;
using Edary.Entities.SubAccounts;
using Edary.IAppServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Users;
using Volo.Abp.Validation;

namespace Edary.AppServices.JournalEntries
{
    public class JournalEntryAppService : 
        CrudAppService<JournalEntry, JournalEntryDto, string, GetJournalEntryListInput, CreateJournalEntryDto, UpdateJournalEntryDto>, 
        IJournalEntryAppService
    {
        private readonly IRepository<JournalEntryDetail, string> _journalEntryDetailRepository;
        private readonly IRepository<JournalEntry, string> _journalEntryRepository;
        private readonly IRepository<SubAccount, string> _subAccountRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentUser _currentUser;

        public JournalEntryAppService(
            IRepository<JournalEntry, string> journalEntryRepository,
            IRepository<JournalEntryDetail, string> journalEntryDetailRepository,
            IRepository<SubAccount, string> subAccountRepository,
            IGuidGenerator guidGenerator,
            ICurrentUser currentUser)
            : base(journalEntryRepository)
        {
            _journalEntryRepository = journalEntryRepository;
            _journalEntryDetailRepository = journalEntryDetailRepository;
            _subAccountRepository = subAccountRepository;
            _guidGenerator = guidGenerator;
            _currentUser = currentUser;
        }

        public override async Task<JournalEntryDto> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف القيد مطلوب");
            var journalEntry = await _journalEntryRepository.WithDetailsAsync(x => x.JournalEntryDetails);
            return ObjectMapper.Map<JournalEntry, JournalEntryDto>(journalEntry.FirstOrDefault(x => x.Id == id));
        }

        public override async Task<PagedResultDto<JournalEntryDto>> GetListAsync(GetJournalEntryListInput input)
        {
            var query = await _journalEntryRepository.WithDetailsAsync(x => x.JournalEntryDetails);

            query = query
                .WhereIf(!input.Filter.IsNullOrWhiteSpace(), 
                    journalEntry => journalEntry.Currency.Contains(input.Filter) ||
                                    journalEntry.Notes.Contains(input.Filter) 
                                    )
                .WhereIf(!input.Currency.IsNullOrWhiteSpace(), 
                    journalEntry => journalEntry.Currency.Contains(input.Currency))
                .WhereIf(!input.Notes.IsNullOrWhiteSpace(), 
                    journalEntry => journalEntry.Notes.Contains(input.Notes));

            var totalCount = await AsyncExecuter.CountAsync(query);

            query = query.OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "Currency asc" : input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var journalEntries = await AsyncExecuter.ToListAsync(query);

            return new PagedResultDto<JournalEntryDto>(totalCount, ObjectMapper.Map<List<JournalEntry>, List<JournalEntryDto>>(journalEntries));
        }

        public override async Task<JournalEntryDto> CreateAsync(CreateJournalEntryDto input)
        {
            ValidateJournalEntryHeader(input.Currency, input.ExchangeRate);
            ValidateJournalEntryDetailsBalance(input.JournalEntryDetails, out var totalDebit, out var totalCredit);
            await ValidateJournalEntryDetailsAsync(input.JournalEntryDetails);

            var journalEntry = new JournalEntry(_guidGenerator.Create().ToString())
            {
                Currency = input.Currency?.Trim(),
                ExchangeRate = input.ExchangeRate,
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
                CurrencyEn = string.IsNullOrWhiteSpace(input.CurrencyEn) ? null : input.CurrencyEn.Trim(),
                JournalEntryDetails = new HashSet<JournalEntryDetail>()
            };

            foreach (var d in input.JournalEntryDetails)
            {
                var subAccountId = string.IsNullOrWhiteSpace(d.SubAccountId) ? null : d.SubAccountId.Trim();
                var detail = new JournalEntryDetail(_guidGenerator.Create().ToString())
                {
                    JournalEntryId = journalEntry.Id,
                    SubAccountId = subAccountId,
                    Description = d.Description?.Trim() ?? "",
                    Debit = d.Debit,
                    Credit = d.Credit
                };
                journalEntry.JournalEntryDetails.Add(detail);
            }

            await _journalEntryRepository.InsertAsync(journalEntry, autoSave: true);
            return ObjectMapper.Map<JournalEntry, JournalEntryDto>(journalEntry);
        }

        public override async Task<JournalEntryDto> UpdateAsync(string id, UpdateJournalEntryDto input)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف القيد مطلوب");

            ValidateJournalEntryHeader(input.Currency, input.ExchangeRate);
            ValidateJournalEntryDetailsBalanceForUpdate(input.JournalEntryDetails, out _, out _);
            await ValidateJournalEntryDetailsForUpdateAsync(input.JournalEntryDetails);

            var journalEntry = await _journalEntryRepository.GetAsync(id);
            journalEntry.Currency = input.Currency?.Trim();
            journalEntry.ExchangeRate = input.ExchangeRate;
            journalEntry.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();
            journalEntry.CurrencyEn = string.IsNullOrWhiteSpace(input.CurrencyEn) ? null : input.CurrencyEn.Trim();

            foreach (var detailDto in input.JournalEntryDetails)
            {
                if (string.IsNullOrWhiteSpace(detailDto.Id))
                {
                    var newDetail = new JournalEntryDetail(_guidGenerator.Create().ToString())
                    {
                        JournalEntryId = id,
                        SubAccountId = string.IsNullOrWhiteSpace(detailDto.SubAccountId) ? null : detailDto.SubAccountId.Trim(),
                        Description = detailDto.Description?.Trim() ?? "",
                        Debit = detailDto.Debit,
                        Credit = detailDto.Credit
                    };
                    await _journalEntryDetailRepository.InsertAsync(newDetail);
                }
                else
                {
                    var existingDetail = await _journalEntryDetailRepository.GetAsync(detailDto.Id);
                    existingDetail.SubAccountId = string.IsNullOrWhiteSpace(detailDto.SubAccountId) ? null : detailDto.SubAccountId.Trim();
                    existingDetail.Description = detailDto.Description?.Trim() ?? "";
                    existingDetail.Debit = detailDto.Debit;
                    existingDetail.Credit = detailDto.Credit;
                    await _journalEntryDetailRepository.UpdateAsync(existingDetail);
                }
            }

            var detailIdsToRemove = (await _journalEntryDetailRepository.GetQueryableAsync())
                .Where(x => x.JournalEntryId == id)
                .Select(x => x.Id)
                .Except(input.JournalEntryDetails.Where(x => !string.IsNullOrWhiteSpace(x.Id)).Select(x => x.Id))
                .ToList();

            foreach (var detailId in detailIdsToRemove)
                await _journalEntryDetailRepository.DeleteAsync(detailId);

            await _journalEntryRepository.UpdateAsync(journalEntry, autoSave: true);
            return ObjectMapper.Map<JournalEntry, JournalEntryDto>(journalEntry);
        }

        public override async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف القيد مطلوب");
            var detailIdsToDelete = await (await _journalEntryDetailRepository.GetQueryableAsync())
                .Where(x => x.JournalEntryId == id)
                .Select(x => x.Id)
                .ToListAsync();
            await _journalEntryDetailRepository.DeleteManyAsync(detailIdsToDelete);
            await _journalEntryRepository.DeleteAsync(id);
        }

        private static void ValidateJournalEntryHeader(string currency, decimal exchangeRate)
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new AbpValidationException("العملة مطلوبة");
            if (exchangeRate <= 0)
                throw new AbpValidationException("سعر الصرف يجب أن يكون أكبر من صفر");
        }

        private static void ValidateJournalEntryDetailsBalance(
            ICollection<CreateJournalEntryDetailDto> details,
            out decimal totalDebit,
            out decimal totalCredit)
        {
            totalDebit = 0;
            totalCredit = 0;
            var index = 0;
            foreach (var d in details)
            {
                if (d.Debit < 0 || d.Credit < 0)
                    throw new AbpValidationException($"السطر {index + 1}: المدين والدائن لا يمكن أن يكونا سالبين.");
                if (d.Debit > 0 && d.Credit > 0)
                    throw new AbpValidationException($"السطر {index + 1}: لا يمكن أن يكون السطر مديناً ودائناً معاً. إما مدين أو دائن فقط.");
                if (d.Debit == 0 && d.Credit == 0)
                    throw new AbpValidationException($"السطر {index + 1}: يجب إدخال مبلغ في المدين أو الدائن.");
                totalDebit += d.Debit;
                totalCredit += d.Credit;
                index++;
            }
            if (Math.Abs(totalDebit - totalCredit) > 0.001m)
                throw new BusinessException("Edary:JournalEntryUnbalanced", "مجموع المدين يجب أن يساوي مجموع الدائن.");
        }

        private async Task ValidateJournalEntryDetailsAsync(ICollection<CreateJournalEntryDetailDto> details)
        {
            var index = 0;
            foreach (var d in details)
            {
                if (string.IsNullOrWhiteSpace(d.Description))
                    throw new AbpValidationException($"السطر {index + 1}: الوصف مطلوب.");
                if (!string.IsNullOrWhiteSpace(d.SubAccountId))
                {
                    var subAccount = await _subAccountRepository.FindAsync(d.SubAccountId.Trim());
                    if (subAccount == null)
                        throw new AbpValidationException($"السطر {index + 1}: الحساب الفرعي غير موجود.");
                }
                index++;
            }
        }

        private static void ValidateJournalEntryDetailsBalanceForUpdate(
            ICollection<UpdateJournalEntryDetailDto> details,
            out decimal totalDebit,
            out decimal totalCredit)
        {
            totalDebit = 0;
            totalCredit = 0;
            var index = 0;
            foreach (var d in details)
            {
                if (d.Debit < 0 || d.Credit < 0)
                    throw new AbpValidationException($"السطر {index + 1}: المدين والدائن لا يمكن أن يكونا سالبين.");
                if (d.Debit > 0 && d.Credit > 0)
                    throw new AbpValidationException($"السطر {index + 1}: لا يمكن أن يكون السطر مديناً ودائناً معاً. إما مدين أو دائن فقط.");
                if (d.Debit == 0 && d.Credit == 0)
                    throw new AbpValidationException($"السطر {index + 1}: يجب إدخال مبلغ في المدين أو الدائن.");
                totalDebit += d.Debit;
                totalCredit += d.Credit;
                index++;
            }
            if (Math.Abs(totalDebit - totalCredit) > 0.001m)
                throw new BusinessException("Edary:JournalEntryUnbalanced", "مجموع المدين يجب أن يساوي مجموع الدائن.");
        }

        private async Task ValidateJournalEntryDetailsForUpdateAsync(ICollection<UpdateJournalEntryDetailDto> details)
        {
            var index = 0;
            foreach (var d in details)
            {
                if (string.IsNullOrWhiteSpace(d.Description))
                    throw new AbpValidationException($"السطر {index + 1}: الوصف مطلوب.");
                if (!string.IsNullOrWhiteSpace(d.SubAccountId))
                {
                    var subAccount = await _subAccountRepository.FindAsync(d.SubAccountId.Trim());
                    if (subAccount == null)
                        throw new AbpValidationException($"السطر {index + 1}: الحساب الفرعي غير موجود.");
                }
                index++;
            }
        }
    }
}
