using System;
using System.ComponentModel.DataAnnotations;
using Edary.Consts.Items;

namespace Edary.DTOs.Items
{
    public class CreateItemPriceDto
    {
        [Required(ErrorMessage = "اسم الوحدة مطلوب")]
        [StringLength(ItemPriceConsts.MaxUnitNameLength, ErrorMessage = "اسم الوحدة لا يمكن أن يتجاوز {1} حرف")]
        public string UnitName { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "سعر الجملة يجب أن يكون بين {1} و {2}")]
        public decimal? WholePrice { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "سعر التجزئة يجب أن يكون بين {1} و {2}")]
        public decimal? RetailPrice { get; set; }

        [Range(0, 999999999999.99, ErrorMessage = "سعر المستهلك يجب أن يكون بين {1} و {2}")]
        public decimal? ConsumerPrice { get; set; }

        [StringLength(ItemPriceConsts.MaxCurrencyLength, ErrorMessage = "العملة لا يمكن أن تتجاوز {1} حرف")]
        public string Currency { get; set; }

        public DateTime? EffectiveDate { get; set; }
        public bool? IsActive { get; set; } = true;

        [StringLength(ItemPriceConsts.MaxUnitNameEnLength, ErrorMessage = "اسم الوحدة بالإنجليزية لا يمكن أن يتجاوز {1} حرف")]
        public string UnitNameEn { get; set; }

        [StringLength(ItemPriceConsts.MaxCurrencyEnLength, ErrorMessage = "العملة بالإنجليزية لا يمكن أن تتجاوز {1} حرف")]
        public string CurrencyEn { get; set; }
    }
}

