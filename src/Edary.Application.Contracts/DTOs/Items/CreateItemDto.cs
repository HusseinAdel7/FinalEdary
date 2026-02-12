using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Edary.Consts.Items;

namespace Edary.DTOs.Items
{
    public class CreateItemDto
    {
        [Required(ErrorMessage = "اسم الصنف مطلوب")]
        [StringLength(ItemConsts.MaxItemNameLength, ErrorMessage = "اسم الصنف لا يمكن أن يتجاوز {1} حرف")]
        public string ItemName { get; set; }

        [StringLength(ItemConsts.MaxItemTypeLength, ErrorMessage = "نوع الصنف لا يمكن أن يتجاوز {1} حرف")]
        public string ItemType { get; set; }

        [StringLength(ItemConsts.MaxGroupNameLength, ErrorMessage = "اسم المجموعة لا يمكن أن يتجاوز {1} حرف")]
        public string GroupName { get; set; }

        [StringLength(ItemConsts.MaxBarcodeLength, ErrorMessage = "الباركود لا يمكن أن يتجاوز {1} حرف")]
        public string Barcode { get; set; }

        [Required(ErrorMessage = "سعر الافتتاح مطلوب")]
        [Range(0, 999999999999.99, ErrorMessage = "سعر الافتتاح يجب أن يكون بين {1} و {2}")]
        public decimal OpeningPrice { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "الحد الأدنى يجب أن يكون بين {1} و {2}")]
        public decimal? MinLimit { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "الحد الأقصى يجب أن يكون بين {1} و {2}")]
        public decimal? MaxLimit { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "كمية إعادة الطلب يجب أن تكون بين {1} و {2}")]
        public decimal? ReorderQty { get; set; }

        [StringLength(ItemConsts.MaxUnitOfMeasureLength, ErrorMessage = "وحدة القياس لا يمكن أن تتجاوز {1} حرف")]
        public string UnitOfMeasure { get; set; }

        [StringLength(ItemConsts.MaxNotesLength, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز {1} حرف")]
        public string Notes { get; set; }

        public bool? IsActive { get; set; } = true;

        [StringLength(ItemConsts.MaxItemNameEnLength, ErrorMessage = "اسم الصنف بالإنجليزية لا يمكن أن يتجاوز {1} حرف")]
        public string ItemNameEn { get; set; }

        [StringLength(ItemConsts.MaxItemTypeEnLength, ErrorMessage = "نوع الصنف بالإنجليزية لا يمكن أن يتجاوز {1} حرف")]
        public string ItemTypeEn { get; set; }

        [StringLength(ItemConsts.MaxGroupNameEnLength, ErrorMessage = "اسم المجموعة بالإنجليزية لا يمكن أن يتجاوز {1} حرف")]
        public string GroupNameEn { get; set; }

        [StringLength(ItemConsts.MaxUnitOfMeasureEnLength, ErrorMessage = "وحدة القياس بالإنجليزية لا يمكن أن تتجاوز {1} حرف")]
        public string UnitOfMeasureEn { get; set; }

        [Required(ErrorMessage = "يجب إدخال سعر واحد على الأقل للصنف")]
        [MinLength(1, ErrorMessage = "يجب إدخال سعر واحد على الأقل للصنف")]
        public ICollection<CreateItemPriceDto> ItemPrices { get; set; }

        public CreateItemDto()
        {
            ItemPrices = new HashSet<CreateItemPriceDto>();
        }
    }
}

