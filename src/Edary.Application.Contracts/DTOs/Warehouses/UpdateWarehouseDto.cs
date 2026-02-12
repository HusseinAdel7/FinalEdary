using System.ComponentModel.DataAnnotations;
using Edary.Consts.Warehouses;

namespace Edary.DTOs.Warehouses
{
    public class UpdateWarehouseDto
    {
        [Required(ErrorMessage = "اسم المستودع مطلوب")]
        [StringLength(WarehouseConsts.MaxWarehouseNameLength, MinimumLength = 1, ErrorMessage = "اسم المستودع لا يمكن أن يتجاوز {1} حرف")]
        public string WarehouseName { get; set; }

        [StringLength(WarehouseConsts.MaxLocationLength, ErrorMessage = "الموقع لا يمكن أن يتجاوز {1} حرف")]
        public string? Location { get; set; }

        [StringLength(WarehouseConsts.MaxManagerNameLength, ErrorMessage = "اسم المدير لا يمكن أن يتجاوز {1} حرف")]
        public string? ManagerName { get; set; }

        [StringLength(WarehouseConsts.MaxNotesLength, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز {1} حرف")]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(WarehouseConsts.MaxWarehouseNameEnLength, ErrorMessage = "اسم المستودع بالإنجليزية لا يمكن أن يتجاوز {1} حرف")]
        public string? WarehouseNameEn { get; set; }

        [StringLength(WarehouseConsts.MaxManagerNameEnLength, ErrorMessage = "اسم المدير بالإنجليزية لا يمكن أن يتجاوز {1} حرف")]
        public string? ManagerNameEn { get; set; }
    }
}

