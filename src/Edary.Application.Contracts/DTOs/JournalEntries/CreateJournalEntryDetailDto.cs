using System;
using System.ComponentModel.DataAnnotations;
using Edary.Consts.JournalEntries;

namespace Edary.DTOs.JournalEntries
{
    public class CreateJournalEntryDetailDto
    {
        public string? SubAccountId { get; set; }

        [Required(ErrorMessage = "الوصف مطلوب")]
        [StringLength(JournalEntryDetailConsts.MaxDescriptionLength, ErrorMessage = "الوصف لا يمكن أن يتجاوز {1} حرف")]
        public string Description { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "المدين يجب أن يكون بين {1} و {2}")]
        public decimal Debit { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "الدائن يجب أن يكون بين {1} و {2}")]
        public decimal Credit { get; set; }
    }
}
