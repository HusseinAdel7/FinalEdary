using System.ComponentModel.DataAnnotations;
using Edary.Consts.SubAccounts;

namespace Edary.DTOs.SubAccounts
{
    public class UpdateSubAccountDto
    {
        [Required(ErrorMessage = "اسم الحساب مطلوب")]
        [StringLength(SubAccountConsts.MaxAccountNameLength, ErrorMessage = "اسم الحساب لا يمكن أن يتجاوز {1} حرف")]
        public string AccountName { get; set; }

        [Required(ErrorMessage = "معرّف الحساب الرئيسي مطلوب")]
        public string MainAccountId { get; set; }

        [Required(ErrorMessage = "العنوان مطلوب")]
        [StringLength(SubAccountConsts.MaxTitleLength, ErrorMessage = "العنوان لا يمكن أن يتجاوز {1} حرف")]
        public string Title { get; set; }

        [Required(ErrorMessage = "نوع الحساب مطلوب")]
        [StringLength(SubAccountConsts.MaxAccountTypeLength, ErrorMessage = "نوع الحساب لا يمكن أن يتجاوز {1} حرف")]
        public string AccountType { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "مبلغ الائتمان يجب أن يكون بين {1} و {2}")]
        public decimal? CreditAmount { get; set; }

        [Required(ErrorMessage = "معدل الائتمان القياسي مطلوب")]
        [StringLength(SubAccountConsts.MaxStandardCreditRateLength, ErrorMessage = "معدل الائتمان القياسي لا يمكن أن يتجاوز {1} حرف")]
        public string StandardCreditRate { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "العمولة يجب أن تكون بين {1} و {2}")]
        public decimal? Commission { get; set; }

        [Range(0, 100, ErrorMessage = "النسبة المئوية يجب أن تكون بين {1} و {2}")]
        public decimal? Percentage { get; set; }

        [Required(ErrorMessage = "عملة الحساب مطلوبة")]
        [StringLength(SubAccountConsts.MaxAccountCurrencyLength, ErrorMessage = "عملة الحساب لا يمكن أن تتجاوز {1} حرف")]
        public string AccountCurrency { get; set; }

        [StringLength(SubAccountConsts.MaxNotesLength, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز {1} حرف")]
        public string Notes { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(SubAccountConsts.MaxAccountNameEnLength, ErrorMessage = "اسم الحساب بالإنجليزية لا يمكن أن يتجاوز {1} حرف")]
        public string? AccountNameEn { get; set; }

        [StringLength(SubAccountConsts.MaxTitleEnLength, ErrorMessage = "العنوان بالإنجليزية لا يمكن أن يتجاوز {1} حرف")]
        public string? TitleEn { get; set; }

        [StringLength(SubAccountConsts.MaxAccountTypeEnLength, ErrorMessage = "نوع الحساب بالإنجليزية لا يمكن أن يتجاوز {1} حرف")]
        public string? AccountTypeEn { get; set; }

        [StringLength(SubAccountConsts.MaxAccountCurrencyEnLength, ErrorMessage = "عملة الحساب بالإنجليزية لا يمكن أن تتجاوز {1} حرف")]
        public string? AccountCurrencyEn { get; set; }
    }
}
