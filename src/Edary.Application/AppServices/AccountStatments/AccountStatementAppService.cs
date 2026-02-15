using Edary.DTOs;
using Edary.DTOs.AccountStatments;
using Edary.IAppServices.Edary.IAppServices;
using Edary.Permissions;
using Edary.Services.AccountStatments;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Timing;
using Volo.Abp.Validation;

namespace Edary.AppServices.AccountStatments
{
    public class AccountStatementAppService :
       ApplicationService,
       IAccountStatementAppService
    {
        private readonly AccountStatementManager _manager;

        public AccountStatementAppService(AccountStatementManager manager)
        {
            _manager = manager;
        }

        public async Task<List<AccountStatementLineDto>> GetByAccountAsync(
            AccountStatementInputDto input)
        {
            return await _manager.GenerateByAccountAsync(
                input.AccountId,
                input.FromDate,
                input.ToDate ?? Clock.Now
            );
        }

        public async Task<List<AccountStatementLineDto>> GetAllAsync(
            AccountStatementPeriodDto input)
        {
            return await _manager.GenerateAllAsync(
                input.FromDate,
                input.ToDate ?? Clock.Now
            );
        }
    }
}