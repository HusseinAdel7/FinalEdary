using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Edary.Domain.Services.Warehouses;
using Edary.DTOs.Warehouses;
using Edary.Entities.Warehouses;
using Edary.IAppServices;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;

namespace Edary.AppServices.Warehouses
{
    public class WarehouseAppService :
        CrudAppService<
            Warehouse,
            WarehouseDto,
            string,
            WarehousePagedRequestDto,
            CreateWarehouseDto,
            UpdateWarehouseDto>,
        IWarehouseAppService
    {
        private readonly WarehouseManager _warehouseManager;

        public WarehouseAppService(
            IRepository<Warehouse, string> repository,
            WarehouseManager warehouseManager)
            : base(repository)
        {
            _warehouseManager = warehouseManager;
        }

        public override async Task<WarehouseDto> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف المستودع مطلوب");
            return await base.GetAsync(id);
        }

        public override async Task<WarehouseDto> CreateAsync(CreateWarehouseDto input)
        {
            if (string.IsNullOrWhiteSpace(input.WarehouseName))
                throw new AbpValidationException("اسم المستودع مطلوب");

            var generatedCode = await _warehouseManager.GenerateNewWarehouseCodeAsync();

            var warehouse = new Warehouse
            {
                Id = GuidGenerator.Create().ToString(),
                WarehouseCode = generatedCode,
                WarehouseName = input.WarehouseName.Trim(),
                Location = string.IsNullOrWhiteSpace(input.Location) ? null : input.Location.Trim(),
                ManagerName = string.IsNullOrWhiteSpace(input.ManagerName) ? null : input.ManagerName.Trim(),
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
                IsActive = input.IsActive,
                WarehouseNameEn = string.IsNullOrWhiteSpace(input.WarehouseNameEn) ? null : input.WarehouseNameEn.Trim(),
                ManagerNameEn = string.IsNullOrWhiteSpace(input.ManagerNameEn) ? null : input.ManagerNameEn.Trim()
            };

            var created = await Repository.InsertAsync(warehouse, autoSave: true);
            return MapToGetOutputDto(created);
        }

        public override async Task<WarehouseDto> UpdateAsync(string id, UpdateWarehouseDto input)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف المستودع مطلوب");
            if (string.IsNullOrWhiteSpace(input.WarehouseName))
                throw new AbpValidationException("اسم المستودع مطلوب");

            var warehouse = await Repository.GetAsync(id);

            warehouse.WarehouseName = input.WarehouseName.Trim();
            warehouse.Location = string.IsNullOrWhiteSpace(input.Location) ? null : input.Location.Trim();
            warehouse.ManagerName = string.IsNullOrWhiteSpace(input.ManagerName) ? null : input.ManagerName.Trim();
            warehouse.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();
            warehouse.IsActive = input.IsActive;
            warehouse.WarehouseNameEn = string.IsNullOrWhiteSpace(input.WarehouseNameEn) ? null : input.WarehouseNameEn.Trim();
            warehouse.ManagerNameEn = string.IsNullOrWhiteSpace(input.ManagerNameEn) ? null : input.ManagerNameEn.Trim();

            var updated = await Repository.UpdateAsync(warehouse, autoSave: true);
            return MapToGetOutputDto(updated);
        }

        public override async Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new AbpValidationException("معرّف المستودع مطلوب");
            await base.DeleteAsync(id);
        }

        public override async Task<PagedResultDto<WarehouseDto>> GetListAsync(WarehousePagedRequestDto input)
        {
            var query = await Repository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                query = query.Where(w =>
                    w.WarehouseCode.Contains(input.Filter) ||
                    w.WarehouseName.Contains(input.Filter) ||
                    (w.WarehouseNameEn != null && w.WarehouseNameEn.Contains(input.Filter)) ||
                    (w.Location != null && w.Location.Contains(input.Filter)) ||
                    (w.ManagerName != null && w.ManagerName.Contains(input.Filter)) ||
                    (w.ManagerNameEn != null && w.ManagerNameEn.Contains(input.Filter))
                );
            }

            if (input.IsActive.HasValue)
            {
                query = query.Where(w => w.IsActive == input.IsActive.Value);
            }

            query = !string.IsNullOrWhiteSpace(input.Sorting)
                ? query.OrderBy(input.Sorting)
                : query.OrderByDescending(w => w.CreationTime);

            var totalCount = await AsyncExecuter.CountAsync(query);
            query = query.PageBy(input.SkipCount, input.MaxResultCount);

            var entities = await AsyncExecuter.ToListAsync(query);
            var dtos = entities.Select(MapToGetOutputDto).ToList();

            return new PagedResultDto<WarehouseDto>(totalCount, dtos);
        }
    }
}

