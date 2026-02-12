using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Edary.Consts.Invoices;

namespace Edary.DTOs.Invoices
{
    public class UpdateInvoiceDto
    {
        [Required(ErrorMessage = "نوع الفاتورة مطلوب")]
        [StringLength(InvoiceConsts.MaxInvoiceTypeLength, ErrorMessage = "نوع الفاتورة لا يمكن أن يتجاوز {1} حرف")]
        public string InvoiceType { get; set; }

        public string? SupplierId { get; set; }

        [Required(ErrorMessage = "المستودع مطلوب")]
        public string WarehouseId { get; set; }

        [StringLength(InvoiceConsts.MaxCurrencyLength, ErrorMessage = "العملة لا يمكن أن تتجاوز {1} حرف")]
        public string Currency { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "المبلغ الإجمالي يجب أن يكون بين {1} و {2}")]
        public decimal? TotalAmount { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "الخصم يجب أن يكون بين {1} و {2}")]
        public decimal? Discount { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "قيمة الضريبة يجب أن تكون بين {1} و {2}")]
        public decimal? TaxAmount { get; set; }

        [StringLength(InvoiceConsts.MaxPaymentStatusLength, ErrorMessage = "حالة الدفع لا يمكن أن تتجاوز {1} حرف")]
        public string PaymentStatus { get; set; }

        [StringLength(InvoiceConsts.MaxNotesLength, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز {1} حرف")]
        public string Notes { get; set; }

        [Required(ErrorMessage = "تفاصيل الفاتورة مطلوبة")]
        [MinLength(1, ErrorMessage = "يجب أن تحتوي الفاتورة على سطر واحد على الأقل")]
        public ICollection<UpdateInvoiceDetailDto> InvoiceDetails { get; set; } = new HashSet<UpdateInvoiceDetailDto>();
    }
}

