using Edary.DTOs.MainAccounts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Edary.IAppServices
{
    public interface IMainAccountAppService :
       ICrudAppService<
           MainAccountDto,              
           string,                     
           MainAccountPagedRequestDto,  
           CreateMainAccountDto,        
           UpdateMainAccountDto         
       >
    {
        /// <summary>
        /// يرجع شجرة الحسابات: اسم الحساب الرئيسي (الأب) وتحته أسماء الحسابات الفرعية (الأبناء) فقط.
        /// </summary>
        Task<List<ChartOfAccountNodeDto>> GetChartOfAccountsAsync();
    }
}
