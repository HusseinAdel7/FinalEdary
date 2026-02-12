using System.ComponentModel.DataAnnotations;
using Edary.Consts.Suppliers;

namespace Edary.DTOs.Suppliers
{
    public class UpdateSupplierDto
    {
        [Required(ErrorMessage = "اسم المورد مطلوب")]
        [StringLength(SupplierConsts.MaxSupplierNameLength, MinimumLength = 1, ErrorMessage = "اسم المورد لا يمكن أن يتجاوز {1} حرف")]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(SupplierConsts.MaxPhoneLength, ErrorMessage = "رقم الهاتف لا يمكن أن يتجاوز {1} حرف")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(SupplierConsts.MaxEmailLength, ErrorMessage = "البريد الإلكتروني لا يمكن أن يتجاوز {1} حرف")]
        public string? Email { get; set; }

        [StringLength(SupplierConsts.MaxAddressLength, ErrorMessage = "العنوان لا يمكن أن يتجاوز {1} حرف")]
        public string? Address { get; set; }

        [StringLength(SupplierConsts.MaxTaxNumberLength, ErrorMessage = "الرقم الضريبي لا يمكن أن يتجاوز {1} حرف")]
        public string? TaxNumber { get; set; }

        [StringLength(SupplierConsts.MaxNotesLength, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز {1} حرف")]
        public string? Notes { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(SupplierConsts.MaxSupplierNameEnLength, ErrorMessage = "اسم المورد بالإنجليزية لا يمكن أن يتجاوز {1} حرف")]
        public string? SupplierNameEn { get; set; }
    }
}

