using System.Collections.Generic;

namespace Edary.DTOs.MainAccounts
{
    /// <summary>
    /// عقدة موحّدة في شجرة الحسابات: اسم الحساب ورقمه فقط + الأبناء.
    /// </summary>
    public class ChartOfAccountNodeDto
    {
        /// <summary>اسم الحساب</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>رقم الحساب</summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>الأبناء: main أو sub تحته</summary>
        public List<ChartOfAccountNodeDto> Children { get; set; } = new List<ChartOfAccountNodeDto>();
    }
}
