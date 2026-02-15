using Edary.DTOs;
using Edary.DTOs.AccountStatments;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Edary.IAppServices
{

    namespace Edary.IAppServices
    {
        public interface IAccountStatementAppService : IApplicationService
        {
            // 1️⃣ كشف حساب لحساب معين
            Task<List<AccountStatementLineDto>> GetByAccountAsync(AccountStatementInputDto input);

            // 2️⃣ كشف حساب لكل الحسابات
            Task<List<AccountStatementLineDto>> GetAllAsync(AccountStatementPeriodDto input);
        }
    }
}

