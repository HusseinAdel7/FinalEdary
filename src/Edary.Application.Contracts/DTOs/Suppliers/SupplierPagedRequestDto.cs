using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Edary.DTOs.Suppliers
{
    public class SupplierPagedRequestDto : PagedAndSortedResultRequestDto
    {
        [StringLength(500, ErrorMessage = "نص البحث لا يمكن أن يتجاوز 500 حرف")]
        public string? Filter { get; set; }

        public bool? IsActive { get; set; }

        public string? SubAccountId { get; set; }
    }
}

