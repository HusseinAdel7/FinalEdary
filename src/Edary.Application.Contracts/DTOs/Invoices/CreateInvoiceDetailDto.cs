using System.ComponentModel.DataAnnotations;
using Edary.Consts.Invoices;

namespace Edary.DTOs.Invoices
{
    public class CreateInvoiceDetailDto
    {
        [Required(ErrorMessage = "الصنف مطلوب")]
        public string ItemId { get; set; }

        [Required(ErrorMessage = "اسم الوحدة مطلوب")]
        [StringLength(InvoiceDetailConsts.MaxUnitNameLength, ErrorMessage = "اسم الوحدة لا يمكن أن يتجاوز {1} حرف")]
        public string UnitName { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Range(0.0001, 999999999.99, ErrorMessage = "الكمية يجب أن تكون بين {1} و {2}")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "سعر الوحدة مطلوب")]
        [Range(0, 999999999999.99, ErrorMessage = "سعر الوحدة يجب أن يكون بين {1} و {2}")]
        public decimal UnitPrice { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "الخصم يجب أن يكون بين {1} و {2}")]
        public decimal? Discount { get; set; }

        [Range(0, 100, ErrorMessage = "نسبة الضريبة يجب أن تكون بين {1} و {2}")]
        public decimal? TaxRate { get; set; }
    }
}

