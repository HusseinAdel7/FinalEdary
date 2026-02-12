using Edary.Domain.Services.Items;
using Edary.DTOs.Items;
using Edary.Entities.Items;
using Edary.IAppServices;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;

namespace Edary.AppServices.Items
{
    public class ItemAppService :
        CrudAppService<
            Item,
            ItemDto,
            string,
            ItemPagedRequestDto,
            CreateItemDto,
            UpdateItemDto>,
        IItemAppService
    {
        private readonly ItemManager _itemManager;
        private readonly IRepository<Item, string> _itemRepository;
        private readonly IRepository<ItemPrice, string> _itemPriceRepository;

        public ItemAppService(
            IRepository<Item, string> itemRepository,
            IRepository<ItemPrice, string> itemPriceRepository,
            ItemManager itemManager)
            : base(itemRepository)
        {
            _itemRepository = itemRepository;
            _itemPriceRepository = itemPriceRepository;
            _itemManager = itemManager;
        }

        public override async Task<ItemDto> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف الصنف مطلوب");
            var query = await _itemRepository
                .WithDetailsAsync(x => x.ItemPrices);
            var item = await query.FirstOrDefaultAsync(x => x.Id == id);
            return ObjectMapper.Map<Item, ItemDto>(item);
        }

        public override async Task<PagedResultDto<ItemDto>> GetListAsync(ItemPagedRequestDto input)
        {
            var query = await _itemRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                query = query.Where(i =>
                    i.ItemCode.Contains(input.Filter) ||
                    i.ItemName.Contains(input.Filter) ||
                    (i.ItemNameEn != null && i.ItemNameEn.Contains(input.Filter)) ||
                    (i.Barcode != null && i.Barcode.Contains(input.Filter)));
            }

            if (input.IsActive.HasValue)
            {
                query = query.Where(i => i.IsActive == input.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.GroupName))
            {
                query = query.Where(i => i.GroupName == input.GroupName);
            }

            if (!string.IsNullOrWhiteSpace(input.ItemType))
            {
                query = query.Where(i => i.ItemType == input.ItemType);
            }

            query = !string.IsNullOrWhiteSpace(input.Sorting)
                ? query.OrderBy(input.Sorting)
                : query.OrderByDescending(i => i.CreationTime);

            var totalCount = await AsyncExecuter.CountAsync(query);
            query = query.PageBy(input.SkipCount, input.MaxResultCount);

            var entities = await AsyncExecuter.ToListAsync(query);
            var dtos = ObjectMapper.Map<List<Item>, List<ItemDto>>(entities);

            return new PagedResultDto<ItemDto>(totalCount, dtos);
        }

        public override async Task<ItemDto> CreateAsync(CreateItemDto input)
        {
            ValidateItemInput(input.ItemName, input.OpeningPrice, input.MinLimit, input.MaxLimit, input.ReorderQty);
            ValidateItemPricesInput(input.ItemPrices);

            var generatedCode = await _itemManager.GenerateNewItemCodeAsync();

            var item = new Item
            {
                Id = GuidGenerator.Create().ToString(),
                ItemCode = generatedCode,
                ItemName = input.ItemName?.Trim(),
                ItemType = string.IsNullOrWhiteSpace(input.ItemType) ? null : input.ItemType.Trim(),
                GroupName = string.IsNullOrWhiteSpace(input.GroupName) ? null : input.GroupName.Trim(),
                Barcode = string.IsNullOrWhiteSpace(input.Barcode) ? null : input.Barcode.Trim(),
                OpeningPrice = input.OpeningPrice,
                MinLimit = input.MinLimit,
                MaxLimit = input.MaxLimit,
                ReorderQty = input.ReorderQty,
                UnitOfMeasure = string.IsNullOrWhiteSpace(input.UnitOfMeasure) ? null : input.UnitOfMeasure.Trim(),
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
                IsActive = input.IsActive ?? true,
                ItemNameEn = string.IsNullOrWhiteSpace(input.ItemNameEn) ? null : input.ItemNameEn.Trim(),
                ItemTypeEn = string.IsNullOrWhiteSpace(input.ItemTypeEn) ? null : input.ItemTypeEn.Trim(),
                GroupNameEn = string.IsNullOrWhiteSpace(input.GroupNameEn) ? null : input.GroupNameEn.Trim(),
                UnitOfMeasureEn = string.IsNullOrWhiteSpace(input.UnitOfMeasureEn) ? null : input.UnitOfMeasureEn.Trim(),
                ItemPrices = new HashSet<ItemPrice>()
            };

            foreach (var p in input.ItemPrices)
            {
                var price = new ItemPrice
                {
                    Id = GuidGenerator.Create().ToString(),
                    ItemId = item.Id,
                    UnitName = p.UnitName?.Trim(),
                    WholePrice = p.WholePrice,
                    RetailPrice = p.RetailPrice,
                    ConsumerPrice = p.ConsumerPrice,
                    Currency = string.IsNullOrWhiteSpace(p.Currency) ? null : p.Currency.Trim(),
                    EffectiveDate = p.EffectiveDate,
                    IsActive = p.IsActive ?? true,
                    UnitNameEn = string.IsNullOrWhiteSpace(p.UnitNameEn) ? null : p.UnitNameEn.Trim(),
                    CurrencyEn = string.IsNullOrWhiteSpace(p.CurrencyEn) ? null : p.CurrencyEn.Trim()
                };
                item.ItemPrices.Add(price);
            }

            var created = await _itemRepository.InsertAsync(item, autoSave: true);
            return ObjectMapper.Map<Item, ItemDto>(created);
        }

        public override async Task<ItemDto> UpdateAsync(string id, UpdateItemDto input)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف الصنف مطلوب");

            ValidateItemInput(input.ItemName, input.OpeningPrice, input.MinLimit, input.MaxLimit, input.ReorderQty);
            ValidateItemPricesInputForUpdate(input.ItemPrices);

            var query = await _itemRepository.WithDetailsAsync(x => x.ItemPrices);
            var item = await query.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                throw new EntityNotFoundException(typeof(Item), id);

            item.ItemName = input.ItemName?.Trim();
            item.ItemType = string.IsNullOrWhiteSpace(input.ItemType) ? null : input.ItemType.Trim();
            item.GroupName = string.IsNullOrWhiteSpace(input.GroupName) ? null : input.GroupName.Trim();
            item.Barcode = string.IsNullOrWhiteSpace(input.Barcode) ? null : input.Barcode.Trim();
            item.OpeningPrice = input.OpeningPrice;
            item.MinLimit = input.MinLimit;
            item.MaxLimit = input.MaxLimit;
            item.ReorderQty = input.ReorderQty;
            item.UnitOfMeasure = string.IsNullOrWhiteSpace(input.UnitOfMeasure) ? null : input.UnitOfMeasure.Trim();
            item.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();
            item.IsActive = input.IsActive;
            item.ItemNameEn = string.IsNullOrWhiteSpace(input.ItemNameEn) ? null : input.ItemNameEn.Trim();
            item.ItemTypeEn = string.IsNullOrWhiteSpace(input.ItemTypeEn) ? null : input.ItemTypeEn.Trim();
            item.GroupNameEn = string.IsNullOrWhiteSpace(input.GroupNameEn) ? null : input.GroupNameEn.Trim();
            item.UnitOfMeasureEn = string.IsNullOrWhiteSpace(input.UnitOfMeasureEn) ? null : input.UnitOfMeasureEn.Trim();

            var existingPricesById = item.ItemPrices
                .Where(p => !string.IsNullOrEmpty(p.Id))
                .ToDictionary(p => p.Id, p => p);

            foreach (var priceDto in input.ItemPrices)
            {
                if (string.IsNullOrEmpty(priceDto.Id))
                {
                    var newPrice = new ItemPrice
                    {
                        Id = GuidGenerator.Create().ToString(),
                        ItemId = id,
                        UnitName = priceDto.UnitName?.Trim(),
                        WholePrice = priceDto.WholePrice,
                        RetailPrice = priceDto.RetailPrice,
                        ConsumerPrice = priceDto.ConsumerPrice,
                        Currency = string.IsNullOrWhiteSpace(priceDto.Currency) ? null : priceDto.Currency.Trim(),
                        EffectiveDate = priceDto.EffectiveDate,
                        IsActive = priceDto.IsActive,
                        UnitNameEn = string.IsNullOrWhiteSpace(priceDto.UnitNameEn) ? null : priceDto.UnitNameEn.Trim(),
                        CurrencyEn = string.IsNullOrWhiteSpace(priceDto.CurrencyEn) ? null : priceDto.CurrencyEn.Trim()
                    };
                    item.ItemPrices.Add(newPrice);
                }
                else if (existingPricesById.TryGetValue(priceDto.Id, out var existingPrice))
                {
                    existingPrice.UnitName = priceDto.UnitName?.Trim();
                    existingPrice.WholePrice = priceDto.WholePrice;
                    existingPrice.RetailPrice = priceDto.RetailPrice;
                    existingPrice.ConsumerPrice = priceDto.ConsumerPrice;
                    existingPrice.Currency = string.IsNullOrWhiteSpace(priceDto.Currency) ? null : priceDto.Currency.Trim();
                    existingPrice.EffectiveDate = priceDto.EffectiveDate;
                    existingPrice.IsActive = priceDto.IsActive;
                    existingPrice.UnitNameEn = string.IsNullOrWhiteSpace(priceDto.UnitNameEn) ? null : priceDto.UnitNameEn.Trim();
                    existingPrice.CurrencyEn = string.IsNullOrWhiteSpace(priceDto.CurrencyEn) ? null : priceDto.CurrencyEn.Trim();
                }
            }

            var inputPriceIds = input.ItemPrices
                .Where(p => !string.IsNullOrEmpty(p.Id))
                .Select(p => p.Id)
                .ToHashSet();
            var pricesToRemove = item.ItemPrices
                .Where(p => !string.IsNullOrEmpty(p.Id) && !inputPriceIds.Contains(p.Id))
                .ToList();
            foreach (var price in pricesToRemove)
                item.ItemPrices.Remove(price);

            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<Item, ItemDto>(item);
        }

        public override async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف الصنف مطلوب");
            var priceIdsToDelete = await (await _itemPriceRepository.GetQueryableAsync())
                .Where(p => p.ItemId == id)
                .Select(p => p.Id)
                .ToListAsync();
            if (priceIdsToDelete.Any())
                await _itemPriceRepository.DeleteManyAsync(priceIdsToDelete);
            await _itemRepository.DeleteAsync(id);
        }

        private static void ValidateItemInput(
            string itemName,
            decimal openingPrice,
            decimal? minLimit,
            decimal? maxLimit,
            decimal? reorderQty)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                throw new AbpValidationException("اسم الصنف مطلوب");
            if (openingPrice < 0)
                throw new AbpValidationException("سعر الافتتاح لا يمكن أن يكون سالباً");
            if (minLimit.HasValue && minLimit.Value < 0)
                throw new AbpValidationException("الحد الأدنى لا يمكن أن يكون سالباً");
            if (maxLimit.HasValue && maxLimit.Value < 0)
                throw new AbpValidationException("الحد الأقصى لا يمكن أن يكون سالباً");
            if (minLimit.HasValue && maxLimit.HasValue && minLimit.Value > maxLimit.Value)
                throw new AbpValidationException("الحد الأدنى لا يمكن أن يكون أكبر من الحد الأقصى");
            if (reorderQty.HasValue && reorderQty.Value < 0)
                throw new AbpValidationException("كمية إعادة الطلب لا يمكن أن تكون سالبة");
        }

        private static void ValidateItemPricesInput(ICollection<CreateItemPriceDto> itemPrices)
        {
            if (itemPrices == null || itemPrices.Count == 0)
                throw new AbpValidationException("يجب إدخال سعر واحد على الأقل للصنف");
            var index = 0;
            foreach (var p in itemPrices)
            {
                if (string.IsNullOrWhiteSpace(p.UnitName))
                    throw new AbpValidationException($"السطر {index + 1} (الأسعار): اسم الوحدة مطلوب.");
                if (p.WholePrice.HasValue && p.WholePrice.Value < 0)
                    throw new AbpValidationException($"السطر {index + 1} (الأسعار): سعر الجملة لا يمكن أن يكون سالباً.");
                if (p.RetailPrice.HasValue && p.RetailPrice.Value < 0)
                    throw new AbpValidationException($"السطر {index + 1} (الأسعار): سعر التجزئة لا يمكن أن يكون سالباً.");
                if (p.ConsumerPrice.HasValue && p.ConsumerPrice.Value < 0)
                    throw new AbpValidationException($"السطر {index + 1} (الأسعار): سعر المستهلك لا يمكن أن يكون سالباً.");
                index++;
            }
        }

        private static void ValidateItemPricesInputForUpdate(ICollection<UpdateItemPriceDto> itemPrices)
        {
            if (itemPrices == null || itemPrices.Count == 0)
                throw new AbpValidationException("يجب إدخال سعر واحد على الأقل للصنف");
            var index = 0;
            foreach (var p in itemPrices)
            {
                if (string.IsNullOrWhiteSpace(p.UnitName))
                    throw new AbpValidationException($"السطر {index + 1} (الأسعار): اسم الوحدة مطلوب.");
                if (p.WholePrice.HasValue && p.WholePrice.Value < 0)
                    throw new AbpValidationException($"السطر {index + 1} (الأسعار): سعر الجملة لا يمكن أن يكون سالباً.");
                if (p.RetailPrice.HasValue && p.RetailPrice.Value < 0)
                    throw new AbpValidationException($"السطر {index + 1} (الأسعار): سعر التجزئة لا يمكن أن يكون سالباً.");
                if (p.ConsumerPrice.HasValue && p.ConsumerPrice.Value < 0)
                    throw new AbpValidationException($"السطر {index + 1} (الأسعار): سعر المستهلك لا يمكن أن يكون سالباً.");
                index++;
            }
        }
    }
}

