using System.ComponentModel.DataAnnotations;
using Edary.Consts.Suppliers;

namespace Edary.DTOs.Suppliers
{
    public class CreateSupplierDto
    {
        [Required(ErrorMessage = "معرّف الحساب الرئيسي مطلوب")]
        public string MainAccountId { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المورد مطلوب")]
        [StringLength(SupplierConsts.MaxSupplierNameLength, ErrorMessage = "اسم المورد لا يمكن أن يتجاوز {1} حرف")]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(SupplierConsts.MaxPhoneLength, ErrorMessage = "رقم الهاتف لا يمكن أن يتجاوز {1} حرف")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(SupplierConsts.MaxEmailLength, ErrorMessage = "البريد الإلكتروني لا يمكن أن يتجاوز {1} حرف")]
        public string Email { get; set; } = string.Empty;

        [StringLength(SupplierConsts.MaxAddressLength, ErrorMessage = "العنوان لا يمكن أن يتجاوز {1} حرف")]
        public string Address { get; set; } = string.Empty;

        [StringLength(SupplierConsts.MaxTaxNumberLength, ErrorMessage = "الرقم الضريبي لا يمكن أن يتجاوز {1} حرف")]
        public string TaxNumber { get; set; } = string.Empty;

        [StringLength(SupplierConsts.MaxNotesLength, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز {1} حرف")]
        public string Notes { get; set; } = string.Empty;

        public bool? IsActive { get; set; }

        [StringLength(SupplierConsts.MaxSupplierNameEnLength, ErrorMessage = "اسم المورد بالإنجليزية لا يمكن أن يتجاوز {1} حرف")]
        public string SupplierNameEn { get; set; } = string.Empty;
    }
}

