using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Edary.Consts.JournalEntries;

namespace Edary.DTOs.JournalEntries
{
    public class UpdateJournalEntryDto
    {
        [Required(ErrorMessage = "العملة مطلوبة")]
        [StringLength(JournalEntryConsts.MaxCurrencyLength, ErrorMessage = "العملة لا يمكن أن تتجاوز {1} حرف")]
        public string Currency { get; set; }

        [Required(ErrorMessage = "سعر الصرف مطلوب")]
        [Range(0.000001, 999999.99, ErrorMessage = "سعر الصرف يجب أن يكون بين {1} و {2}")]
        public decimal ExchangeRate { get; set; }

        [StringLength(JournalEntryConsts.MaxNotesLength, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز {1} حرف")]
        public string Notes { get; set; }

        [StringLength(JournalEntryConsts.MaxCurrencyEnLength, ErrorMessage = "العملة بالإنجليزية لا يمكن أن تتجاوز {1} حرف")]
        public string CurrencyEn { get; set; }

        [Required(ErrorMessage = "تفاصيل القيد مطلوبة")]
        [MinLength(2, ErrorMessage = "يجب أن يحتوي القيد على سطرين على الأقل (مدين و دائن)")]
        public ICollection<UpdateJournalEntryDetailDto> JournalEntryDetails { get; set; }

        public UpdateJournalEntryDto()
        {
            JournalEntryDetails = new HashSet<UpdateJournalEntryDetailDto>();
        }
    }
}
