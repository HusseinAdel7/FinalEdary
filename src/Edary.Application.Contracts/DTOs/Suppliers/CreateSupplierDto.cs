using System.ComponentModel.DataAnnotations;

namespace Edary.DTOs.Suppliers
{
    public class CreateSupplierDto
    {
        [Required]
        public string MainAccountId { get; set; }

        [Required]
        public string SupplierName { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string TaxNumber { get; set; }
        public string Notes { get; set; }
        public bool? IsActive { get; set; }
        public string SupplierNameEn { get; set; }
    }
}

