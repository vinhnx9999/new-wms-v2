using ExcelDataReader;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.MultiTenancy;
using WMSSolution.Core.Services;
using WMSSolution.Shared;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.MasterData;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Sku;
using WMSSolution.WMS.IServices;
using WMSSolution.WMS.IServices.Sku;

namespace WMSSolution.WMS.Services.Sku;

/// <summary>
/// Get all Sku Uoms
/// </summary>
public partial class SkuService(SqlDBContext dbContext, ILogger<SkuService> logger,
    ISpuService spuService, ICategoryService categoryService) :
    BaseService<SkuEntity>, ISkuService
{
    private readonly SqlDBContext _dbContext = dbContext;
    private readonly ILogger<SkuService> _logger = logger;
    private readonly ISpuService _spuService = spuService;
    private readonly ICategoryService _categoryService = categoryService;

    /// <summary>
    /// Get all Sku Uoms
    /// </summary>
    /// <returns></returns>
    public async Task<List<SkuUomDTO>> GetAllSkuUomsAsync(long tenantId)
    {
        var entities = await _dbContext.GetDbSet<SkuUomEntity>(tenantId).ToListAsync();
        return entities.Adapt<List<SkuUomDTO>>();
    }

    #region Dat

    /// <summary>
    /// Creates a new SKU with associated UOMs and validates specification constraints.
    /// </summary>
    /// <param name="request">The SKU creation data transfer object.</param>
    /// <param name="currentUser">The current user information.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The created SkuEntity.</returns>
    public async Task<int> CreateSkuAsync(SkuCreateRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        int targetSpuId = 0;
        bool hasCategory = request.CategoryId.HasValue && request.CategoryId > 0;
        bool hasSpu = request.SpuId.HasValue && request.SpuId > 0;
        if (hasCategory && hasSpu)
        {
            var spu = await _dbContext.GetDbSet<SpuEntity>(currentUser.tenant_id)
                .FirstOrDefaultAsync(s => s.Id == request.SpuId && s.category_id == request.CategoryId, cancellationToken);

            if (spu == null)
            {
                _logger.LogWarning("Tenant {TenantId}: Spu {SpuId} not found or does not match the category {CategoryId}.", currentUser.tenant_id, request.SpuId, request.CategoryId);
                return 0;
            }
            targetSpuId = spu.Id;
        }
        else if (!hasCategory && !hasSpu)
        {
            var globalUnknownSpu = await GetOrCreateUnknownGroupAsync(currentUser.tenant_id, cancellationToken);
            targetSpuId = globalUnknownSpu.Id;
        }
        else
        {
            _logger.LogWarning("Tenant {TenantId}: Incomplete request. Both CategoryId and SpuId must be provided, or both must be empty.", currentUser.tenant_id);
            return 0;
        }

        var specifications = await ValidateSpecificationsAsync(request.SpecificationCodes, cancellationToken);
        if (specifications == null)
        {
            return 0;
        }

        var skuEntity = request.Adapt<SkuEntity>();
        skuEntity.spu_id = targetSpuId;

        await _dbContext.GetDbSet<SkuEntity>().AddAsync(skuEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await LinkSpecificationsToSkuAsync(skuEntity.Id, specifications);

        bool isUomLinked = await LinkUomsToSkuAsync(skuEntity.Id, request.SkuUomID, currentUser.tenant_id, cancellationToken);
        if (!isUomLinked)
        {
            return 0;
        }

        await transaction.CommitAsync(cancellationToken);
        return skuEntity.Id;
    }


    private async Task<SpuEntity> GetOrCreateUnknownGroupAsync(long tenantId, CancellationToken cancellationToken)
    {

        var unknownCategory = await _dbContext.GetDbSet<CategoryEntity>(tenantId)
            .FirstOrDefaultAsync(c => c.category_name == "Unknown_Category", cancellationToken);

        if (unknownCategory == null)
        {
            unknownCategory = new CategoryEntity { category_name = "Unknown_Category", TenantId = tenantId };
            await _dbContext.GetDbSet<CategoryEntity>().AddAsync(unknownCategory, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var unknownSpu = await _dbContext.GetDbSet<SpuEntity>(tenantId)
            .FirstOrDefaultAsync(s => s.spu_code == "UNKNOWN_SPU", cancellationToken);

        if (unknownSpu == null)
        {
            unknownSpu = new SpuEntity
            {
                spu_code = "UNKNOWN_SPU",
                spu_name = "UNKNOWN_SPU",
                category_id = unknownCategory.Id,
                TenantId = tenantId
            };
            await _dbContext.GetDbSet<SpuEntity>().AddAsync(unknownSpu, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return unknownSpu;
    }

    private async Task LinkSpecificationsToSkuAsync(int skuId, List<SpecificationEntity> specs)
    {
        if (specs == null || !specs.Any()) return;

        var skuSpecs = specs.Select(s => new SkuSpecificationEntity
        {
            sku_id = skuId,
            specification_id = s.Id
        });

        await _dbContext.GetDbSet<SkuSpecificationEntity>().AddRangeAsync(skuSpecs);
        await _dbContext.SaveChangesAsync();
    }

    private async Task<bool> LinkUomsToSkuAsync(int skuId, int skuUomId, long tenantId, CancellationToken cancellationToken)
    {

        if (skuUomId <= 0)
        {
            _logger.LogWarning("Tenant {TenantId}: At least one Unit of Measure (UOM) must be provided for the SKU.", tenantId);
            return false;
        }

        bool uomExists = await _dbContext.GetDbSet<SkuUomEntity>()
                .AnyAsync(u => u.Id == skuUomId && u.TenantId == tenantId, cancellationToken);

        if (!uomExists)
        {
            _logger.LogWarning("Tenant {TenantId}: SkuUomId {SkuUomId} is not existing in the system.", tenantId, skuUomId);
            return false;
        }

        var uomLink = new SkuUomLinkEntity
        {
            SkuId = skuId,
            SkuUomId = skuUomId
        };

        await _dbContext.GetDbSet<SkuUomLinkEntity>().AddAsync(uomLink, cancellationToken);

        return true;
    }

    /// <summary>
    /// Validates specifications and checks 'is_duplicate' constraints.
    /// </summary>
    private async Task<List<SpecificationEntity>?> ValidateSpecificationsAsync(List<string> specCodes, CancellationToken cancellationToken)
    {
        if (specCodes == null || !specCodes.Any()) return new List<SpecificationEntity>();

        var distinctCodes = specCodes.Distinct().ToList();
        var specs = await _dbContext.GetDbSet<SpecificationEntity>()
                                    .Where(s => distinctCodes.Contains(s.specification_code))
                                    .ToListAsync(cancellationToken);

        if (specs.Count != distinctCodes.Count)
        {
            var foundCodes = specs.Select(s => s.specification_code).ToList();
            var invalidCodes = distinctCodes.Except(foundCodes);
            _logger.LogWarning("specifications is not valid: {InvalidCodes}", string.Join(", ", invalidCodes));
            return null;
        }

        var nonDuplicateSpecIds = specs.Where(s => !s.is_duplicate).Select(s => s.Id).ToList();

        if (nonDuplicateSpecIds.Any())
        {
            var alreadyUsedIds = await _dbContext.GetDbSet<SkuSpecificationEntity>()
                .Where(link => nonDuplicateSpecIds.Contains(link.specification_id))
                .Select(link => link.specification_id)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (alreadyUsedIds.Any())
            {
                var usedNames = specs.Where(s => alreadyUsedIds.Contains(s.Id)).Select(s => s.specification_name);
                _logger.LogWarning("Non-duplicate specifications are already used: {UsedNames}", string.Join(", ", usedNames));
                return null;
            }
        }

        return specs;
    }

    private class ExcelSkuDto
    {
        public string SkuCode { get; set; } = string.Empty;
        public string SkuName { get; set; } = string.Empty;
        public string SkuFeatured { get; set; } = string.Empty;
        public string SpuCode { get; set; } = string.Empty;
        public string SkuUom { get; set; } = string.Empty;
        public bool IsTracking { get; set; } = false;
        public string SpecCode { get; set; } = string.Empty;
        public string SpecName { get; set; } = string.Empty;
        public bool IsDuplicate { get; set; } = false;
    }

    /// <summary>
    /// Handle Create Sku with Excel Async
    /// </summary>
    /// <param name="file"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<List<SkuEntity>> CreateSkuWithExcelAsync(IFormFile file, CurrentUser currentUser)
    {
        if (file == null || file.Length <= 0)
            throw new Exception("Excel or CSV file is empty.");

        var excelData = ReadSkuDataFromExcel(file);
        var createdSkus = new List<SkuEntity>();

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            foreach (var row in excelData)
            {
                var spu = await GetOrCreateSpuAsync(row.SpuCode, currentUser.tenant_id);

                var skuEntity = await _dbContext.GetDbSet<SkuEntity>()
                    .FirstOrDefaultAsync(s => s.sku_code == row.SkuCode && s.Spu.TenantId == currentUser.tenant_id);

                bool isNewSku = false;
                if (skuEntity == null)
                {
                    skuEntity = new SkuEntity
                    {
                        sku_code = row.SkuCode,
                        sku_name = row.SkuName,
                        is_tracking = row.IsTracking,
                        spu_id = spu.Id,
                        Spu = spu
                    };
                    await _dbContext.GetDbSet<SkuEntity>().AddAsync(skuEntity);
                    isNewSku = true;
                }
                else
                {
                    skuEntity.sku_name = row.SkuName;
                    skuEntity.is_tracking = row.IsTracking;
                    skuEntity.spu_id = spu.Id;
                    _dbContext.GetDbSet<SkuEntity>().Update(skuEntity);
                }

                await _dbContext.SaveChangesAsync();

                if (!string.IsNullOrEmpty(row.SpecCode))
                {
                    var spec = await GetOrCreateSpecificationAsync(row.SpecCode, row.SpecName, row.IsDuplicate);
                    await LinkSpecificationToSkuExcelAsync(skuEntity.Id, spec);
                }

                if (!string.IsNullOrEmpty(row.SkuUom))
                {
                    await LinkUomToSkuExcelAsync(skuEntity, row.SkuUom, currentUser.tenant_id);
                }

                if (isNewSku)
                {
                    createdSkus.Add(skuEntity);
                }
            }

            await transaction.CommitAsync();
            return createdSkus;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Excel/CSV import failed");
            throw;
        }
    }

    private List<ExcelSkuDto> ReadSkuDataFromExcel(IFormFile file)
    {
        var requests = new List<ExcelSkuDto>();
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using var stream = file.OpenReadStream();
        using IExcelDataReader reader = file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
            ? ExcelReaderFactory.CreateCsvReader(stream)
            : ExcelReaderFactory.CreateReader(stream);

        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
        });

        var table = result.Tables[0];

        foreach (DataRow row in table.Rows)
        {
            var skuCode = row[0]?.ToString();
            if (string.IsNullOrEmpty(skuCode)) continue;

            var request = new ExcelSkuDto
            {
                SkuCode = skuCode,
                SkuName = row.ItemArray.Length > 1 ? row[1]?.ToString() ?? string.Empty : string.Empty,
                SkuFeatured = row.ItemArray.Length > 2 ? row[2]?.ToString() ?? string.Empty : string.Empty,
                SpuCode = row.ItemArray.Length > 3 ? row[3]?.ToString() ?? string.Empty : string.Empty,
                SkuUom = row.ItemArray.Length > 4 ? row[4]?.ToString() ?? string.Empty : string.Empty,
                IsTracking = row.ItemArray.Length > 5 && (row[5]?.ToString() == "1" || row[5]?.ToString()?.ToLower() == "true"),
                SpecCode = row.ItemArray.Length > 6 ? row[6]?.ToString() ?? string.Empty : string.Empty,
                SpecName = row.ItemArray.Length > 7 ? row[7]?.ToString() ?? string.Empty : string.Empty,
                IsDuplicate = row.ItemArray.Length > 8 && (row[8]?.ToString() == "1" || row[8]?.ToString()?.ToLower() == "true")
            };

            requests.Add(request);
        }

        return requests;
    }

    private async Task<SpuEntity> GetOrCreateSpuAsync(string spuCode, long tenantId)
    {
        if (string.IsNullOrEmpty(spuCode))
        {
            spuCode = "DEFAULT_SPU";
        }

        var spu = await _dbContext.GetDbSet<SpuEntity>()
            .FirstOrDefaultAsync(s => s.spu_code == spuCode && s.TenantId == tenantId);

        if (spu == null)
        {
            var defaultCategoryName = "default";
            var category = await _dbContext.GetDbSet<CategoryEntity>()
                .FirstOrDefaultAsync(s => s.category_name == defaultCategoryName && s.TenantId == tenantId);

            if (category == null)
            {
                category = new CategoryEntity
                {
                    category_name = defaultCategoryName,
                    TenantId = tenantId
                };
                await _dbContext.GetDbSet<CategoryEntity>().AddAsync(category);
                await _dbContext.SaveChangesAsync();
            }

            spu = new SpuEntity
            {
                spu_code = spuCode,
                spu_name = spuCode,
                category_id = category.Id,
                TenantId = tenantId,
            };
            await _dbContext.GetDbSet<SpuEntity>().AddAsync(spu);
            await _dbContext.SaveChangesAsync();
        }
        return spu;
    }

    private async Task<SpecificationEntity> GetOrCreateSpecificationAsync(string specCode, string specName, bool isDuplicate)
    {
        var spec = await _dbContext.GetDbSet<SpecificationEntity>()
            .FirstOrDefaultAsync(s => s.specification_code == specCode);

        if (spec == null)
        {
            spec = new SpecificationEntity
            {
                specification_code = specCode,
                specification_name = string.IsNullOrEmpty(specName) ? specCode : specName,
                is_duplicate = isDuplicate
            };
            await _dbContext.GetDbSet<SpecificationEntity>().AddAsync(spec);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            bool isUpdated = false;
            if (!string.IsNullOrEmpty(specName) && spec.specification_name != specName)
            {
                spec.specification_name = specName;
                isUpdated = true;
            }
            if (spec.is_duplicate != isDuplicate)
            {
                spec.is_duplicate = isDuplicate;
                isUpdated = true;
            }

            if (isUpdated)
            {
                _dbContext.GetDbSet<SpecificationEntity>().Update(spec);
                await _dbContext.SaveChangesAsync();
            }
        }
        return spec;
    }

    private async Task LinkSpecificationToSkuExcelAsync(int skuId, SpecificationEntity spec)
    {
        if (!spec.is_duplicate)
        {
            var alreadyUsed = await _dbContext.GetDbSet<SkuSpecificationEntity>()
                .AnyAsync(link => link.specification_id == spec.Id && link.sku_id != skuId);

            if (alreadyUsed)
                throw new Exception($"Specification '{spec.specification_code}' does not allow duplicates and is already assigned to another SKU.");
        }

        var linkExists = await _dbContext.GetDbSet<SkuSpecificationEntity>()
            .AnyAsync(link => link.sku_id == skuId && link.specification_id == spec.Id);

        if (!linkExists)
        {
            var skuSpec = new SkuSpecificationEntity
            {
                sku_id = skuId,
                specification_id = spec.Id
            };
            await _dbContext.GetDbSet<SkuSpecificationEntity>().AddAsync(skuSpec);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task LinkUomToSkuExcelAsync(SkuEntity sku, string unitName, long tenantId)
    {
        var uomMaster = await _dbContext.GetDbSet<SkuUomEntity>()
            .FirstOrDefaultAsync(u => u.UnitName == unitName && u.TenantId == tenantId);

        if (uomMaster == null)
        {
            uomMaster = new SkuUomEntity
            {
                UnitName = unitName,
                //ConversionRate = 1,
                //IsBaseUnit = true,
                TenantId = tenantId
            };
            await _dbContext.GetDbSet<SkuUomEntity>().AddAsync(uomMaster);
            await _dbContext.SaveChangesAsync();
        }

        var linkExists = await _dbContext.GetDbSet<SkuUomLinkEntity>()
            .AnyAsync(l => l.SkuId == sku.Id && l.SkuUomId == uomMaster.Id);

        if (!linkExists)
        {
            var uomLink = new SkuUomLinkEntity
            {
                SkuId = sku.Id,
                SkuUomId = uomMaster.Id,
                Sku = sku,
                SkuUom = uomMaster
            };
            await _dbContext.GetDbSet<SkuUomLinkEntity>().AddAsync(uomLink);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<SkuViewModel> GetAsync(int id)
    {
        var dbSet = _dbContext.GetDbSet<SkuEntity>();
        var entity = await dbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (entity == null) return new SkuViewModel();
        return entity.Adapt<SkuViewModel>();
    }

    public async Task<(bool flag, string msg)> UpdateAsync(SkuViewModel viewModel)
    {
        var dbSet = _dbContext.GetDbSet<SkuEntity>();
        var entity = await dbSet.FirstOrDefaultAsync(t => t.Id == viewModel.id);
        if (entity == null)
        {
            // Can't use stringLocalizer because it's not injected here. So return generic message.
            return (false, "Not exists entity");
        }

        if (await dbSet.AsNoTracking().AnyAsync(t => t.Id != viewModel.id && t.sku_code == viewModel.sku_code))
        {
            return (false, "Exists entity with this code");
        }

        entity.sku_code = viewModel.sku_code;
        entity.sku_name = viewModel.sku_name;
        entity.bar_code = viewModel.bar_code;

        // update other fields
        entity.weight = viewModel.weight;
        entity.lenght = viewModel.lenght;
        entity.width = viewModel.width;
        entity.height = viewModel.height;
        entity.volume = viewModel.volume;
        entity.unit = viewModel.unit;
        entity.cost = viewModel.cost;
        entity.price = viewModel.price;

        entity.last_update_time = DateTime.UtcNow;

        var qty = await _dbContext.SaveChangesAsync();
        if (qty > 0)
            return (true, "Save success");

        return (false, "Save failed");
    }

    /// <summary>
    /// Delete SKU
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeleteAsync(int id)
    {
        var dbSet = _dbContext.GetDbSet<SkuEntity>();
        var entity = await dbSet.FirstOrDefaultAsync(t => t.Id == id);
        if (entity == null)
        {
            return (false, "Not exists entity");
        }

        // In actual we should check references like sku_supplier, sku_uom...
        var skuSupplierSet = _dbContext.GetDbSet<SkuSupplierEntity>();
        if (await skuSupplierSet.AsNoTracking().AnyAsync(t => t.SkuId == id))
        {
            return (false, "Delete referenced");
        }

        dbSet.Remove(entity);
        var qty = await _dbContext.SaveChangesAsync();

        if (qty > 0)
            return (true, "Delete success");

        return (false, "Delete failed");
    }

    #endregion

    /// <summary>
    /// page search Sku Supplier
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="warehouseId"></param>
    /// <returns></returns>
    public async Task<(List<SkuSupplierDTO> data, int total)> PageAsync(PageSearch pageSearch, int? warehouseId, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantID = currentUser.tenant_id;
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantID);

        if (pageSearch.GetAllSearch)
        {
            var querySku = await skuList.Select(sku => new SkuSupplierDTO
            {
                SkuId = sku.Id,
                SkuName = sku.sku_name,
                SkuCode = sku.sku_code,
                UnitName = sku.unit,
            }).OrderBy(t => t.SkuName)
              .ToListAsync(cancellationToken);

            return (querySku.DistinctBy(x => x.SkuId).ToList(), querySku.Count);
        }


        var baseQuery = _dbContext.Database.SqlQuery<SkuSupplierDTO>(
          $"SELECT * FROM get_sku_inventory_report({tenantID}, {warehouseId})"
        );


        Core.DynamicSearch.QueryCollection queries = [];
        if (pageSearch.searchObjects?.Count > 0)
        {
            pageSearch.searchObjects.ForEach(s => queries.Add(s));
            var expression = queries.AsGroupedExpression<SkuSupplierDTO>();

            if (expression != null)
            {
                baseQuery = baseQuery.Where(expression);
            }
        }

        int totals = await baseQuery.CountAsync(cancellationToken);
        if (totals == 0)
        {
            return ([], 0);
        }

        var results = await baseQuery
               .OrderByDescending(s => s.AvailableQty)
               .ThenBy(s => s.SkuName)
               .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
               .Take(pageSearch.pageSize)
               .ToListAsync(cancellationToken);

        return (results, totals);
    }

    /// <summary>
    /// Sku Supplier page search with supplier filter
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <param name="supplierID"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(List<SkuSupplierDTO> data, int total)> PageSkuSupplierAsync(PageSearch pageSearch, int? supplierID, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantID = currentUser.tenant_id;
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantID);
        var supplierList = _dbContext.GetDbSet<SupplierEntity>(tenantID);
        var skuUomList = _dbContext.GetDbSet<SkuUomEntity>(tenantID);
        var skuSupplierList = _dbContext.GetDbSet<SkuSupplierEntity>();
        var skuUomLinkList = _dbContext.GetDbSet<SkuUomLinkEntity>().AsNoTracking();

        // Base UOM lookup through junction table (sku_uom_link)
        var baseUomBySku = from link in skuUomLinkList
                           join uom in skuUomList //.Where(u => u.IsBaseUnit)
                               on link.SkuUomId equals uom.Id
                           select new { link.SkuId, UomId = uom.Id, uom.UnitName };

        var query = from sku in skuList


                    join ss in skuSupplierList
                        on sku.Id equals ss.SkuId into ssGroup
                    from ss in ssGroup.DefaultIfEmpty()

                    join supplier in supplierList
                        on ss.SupplierId equals supplier.Id into suppGroup
                    from supplier in suppGroup.DefaultIfEmpty()


                    join baseUom in baseUomBySku
                        on sku.Id equals baseUom.SkuId into uomGroup
                    from baseUom in uomGroup.DefaultIfEmpty()
                        //where (supplierID.HasValue && ss.SupplierId == supplierID.Value) || ss == null

                    select new SkuSupplierDTO
                    {
                        SkuId = sku.Id,
                        SkuName = sku.sku_name,
                        SkuCode = sku.sku_code,
                        SupplierId = supplier.Id,
                        SupplierName = supplier.supplier_name,
                        SkuUomId = baseUom.UomId,
                        UnitName = baseUom.UnitName
                    };

        Core.DynamicSearch.QueryCollection queries = [];
        if (pageSearch.searchObjects.Count != 0)
        {
            pageSearch.searchObjects.ForEach(s =>
            {
                queries.Add(s);
            });
        }
        var expression = queries.AsGroupedExpression<SkuSupplierDTO>();

        if (expression != null)
        {
            query = query.Where(expression);
        }

        int totals = await query.CountAsync(cancellationToken);
        if (totals == 0)
        {
            return ([], totals);
        }
        var list = await query
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync(cancellationToken);

        var data = list.Adapt<List<SkuSupplierDTO>>();
        return (data, totals);
    }

    /// <summary>
    /// Get total count of Sku items
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<int> GetTotalItems(CurrentUser currentUser)
    {
        //TODO later: Get total count of Sku items for the current tenant
        var skuList = _dbContext.GetDbSet<SkuEntity>(currentUser.tenant_id);
        return await skuList.CountAsync();
    }

    /// <summary>
    /// Import Excel
    /// </summary>
    /// <param name="skus"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> ImportExcelData(List<InputSku> skus,
        CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (skus.Count == 0)
        {
            return 0;
        }

        var units = skus.Select(x => x.Unit).Distinct();
        var categories = skus.Select(x => x.Category).Distinct();
        var properties = skus.Select(x => x.Property).Distinct();

        int MaxQtyPerPallet = 100;
        int index = 0;
        int insertedCount = 0;
        int totalSkus = skus.Count;
        var tenantId = currentUser.tenant_id;

        var newSpecificationList = AddNewSpecifications(tenantId, skus, currentUser.user_name);
        _dbContext.GetDbSet<SpecificationEntity>().AddRange(newSpecificationList);

        ProccesingAddMatterData(tenantId, units, categories, properties, currentUser.user_name);
        await _dbContext.SaveChangesAsync();

        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantId);
        var uniqueSkus = skus.Select(x => new InputExcelBase
        {
            Code = x.Code.Trim(),
            Property = x.Property.Trim()
        }).GroupBy(x => new { x.Code, x.Property })
        .Select(g => g.First());

        do
        {
            var items = uniqueSkus
                .Skip(index).Take(SystemDefine.BatchSize)
                .Select(x =>
                {
                    var spu = MapSpuId(tenantId, x.Property);
                    return new SkuEntity
                    {
                        create_time = DateTime.UtcNow,
                        is_tracking = true,
                        sku_code = x.Code,
                        sku_name = x.Code,
                        maxQtyPerPallet = MaxQtyPerPallet,
                        spu_id = spu?.Id ?? 0,
                        Spu = spu,
                        TenantId = tenantId
                    };
                }).ToList();

            foreach (var item in items)
            {
                var data = skus.FirstOrDefault(x => x.Code == item.sku_code);
                if (data != null)
                {
                    item.sku_name = data.Name;
                    item.unit = data.Unit;
                }
            }

            try
            {
                var res = await SaveNewSkusAsync(tenantId, items, skuList, skus);
                insertedCount += res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting batch starting at index {Index}", index);
                // Optionally, you can choose to break the loop or continue with the next batch
                // break;
            }

            // Xử lý batch
            Console.WriteLine($"Batch starting at {index}, count = {items.Count}");
            // Tăng index lên batchSize
            index += SystemDefine.BatchSize;

        } while (index < totalSkus);

        return insertedCount;
    }

    private SpuEntity? MapSpuId(long tenantId, string property)
    {
        if (string.IsNullOrEmpty(property))
        {
            _logger.LogWarning("Tenant {TenantId}: Property is empty, cannot map to SPU.", tenantId);
            property = "Spu Unknown";

        }
        return _dbContext.GetDbSet<SpuEntity>(tenantId, true)
            .FirstOrDefault(x => x.spu_name == property && x.is_valid);
    }

    private async Task<int> SaveNewSkusAsync(long tenantId,
        List<SkuEntity> items, IQueryable<SkuEntity> skuList, List<InputSku> skus)
    {
        var newList = new List<SkuEntity>();

        foreach (var item in items)
        {
            var hasData = skuList.Any(s => s.sku_code == item.sku_code);
            if (!hasData)
            {
                newList.Add(item);
            }
        }

        try
        {
            _dbContext.GetDbSet<SkuEntity>().AddRange(newList);
            var res = await _dbContext.SaveChangesAsync();

            UpdateUnitAndSpecification(tenantId, skuList, newList, skus);
            await _dbContext.SaveChangesAsync();

            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving SKUs to database");
        }

        return 0;
    }

    private void UpdateUnitAndSpecification(long tenantId,
        IQueryable<SkuEntity> skuList, List<SkuEntity> newList, List<InputSku> skus)
    {
        var specificationList = _dbContext.GetDbSet<SpecificationEntity>(tenantId);
        foreach (var item in newList)
        {
            var data = skuList.FirstOrDefault(x => x.sku_code == item.sku_code);
            if (data != null)
            {
                var uom = _dbContext.GetDbSet<SkuUomEntity>(tenantId)
                    .FirstOrDefault(x => x.UnitName == data.unit);
                if (uom != null)
                {
                    _dbContext.GetDbSet<SkuUomLinkEntity>().Add(
                        new SkuUomLinkEntity
                        {
                            SkuId = data.Id,
                            SkuUomId = uom.Id
                        });
                }

                var itemData = skus.Where(x => x.Code == item.sku_code);
                foreach (var dataItem in itemData)
                {
                    var spec = specificationList
                        .FirstOrDefault(x => x.specification_code == dataItem.ProductCode &&
                        x.specification_name == dataItem.ProductName);

                    if (spec != null)
                    {
                        _dbContext.GetDbSet<SkuSpecificationEntity>()
                            .Add(new SkuSpecificationEntity
                            {
                                sku_id = data.Id,
                                specification_id = spec.Id
                            });
                    }
                }
            }
        }
    }

    private void ProccesingAddMatterData(long tenantId,
        IEnumerable<string> units,
        IEnumerable<string> categories,
        IEnumerable<string> properties,
        string userName)
    {
        var newCategoryList = AddNewCategories(tenantId, categories, userName);
        _dbContext.GetDbSet<CategoryEntity>().AddRange(newCategoryList);

        var newUnitList = AddNewUnits(tenantId, units, userName);
        _dbContext.GetDbSet<SkuUomEntity>().AddRange(newUnitList);

        var newSpuList = AddNewProperties(tenantId, properties, userName);
        _dbContext.GetDbSet<SpuEntity>().AddRange(newSpuList);
    }

    private IEnumerable<SpuEntity> AddNewProperties(long tenantId, IEnumerable<string> properties, string userName)
    {
        var spuList = _dbContext.GetDbSet<SpuEntity>(tenantId);
        var newList = new List<SpuEntity>();
        foreach (var property in properties.Where(x => !string.IsNullOrEmpty(x)))
        {
            var hasData = spuList.Any(x => x.spu_code == property);
            if (!hasData)
            {
                newList.Add(new SpuEntity
                {
                    spu_code = property,
                    spu_name = property,
                    creator = userName,
                    create_time = DateTime.UtcNow,
                    last_update_time = DateTime.UtcNow,
                    is_valid = true,
                    TenantId = tenantId
                });
            }
        }

        return newList;
    }

    private IEnumerable<SpecificationEntity> AddNewSpecifications(long tenantId, List<InputSku> skus, string userName)
    {
        var specificationList = _dbContext.GetDbSet<SpecificationEntity>(tenantId);
        var specifications = skus
            .Where(x => !string.IsNullOrEmpty(x.ProductCode))
            .Select(x => new
            {
                ItemCode = x.ProductCode,
                ItemName = x.ProductName
            })
            .Distinct();

        var newList = new List<SpecificationEntity>();
        foreach (var specification in specifications)
        {
            var hasData = specificationList.Any(x =>
                x.specification_code == specification.ItemCode
                && x.specification_name == specification.ItemName);
            if (!hasData)
            {
                newList.Add(new SpecificationEntity
                {
                    specification_code = specification.ItemCode,
                    specification_name = specification.ItemName,
                    is_duplicate = false,
                    create_time = DateTime.UtcNow,
                    update_time = DateTime.UtcNow,
                    is_delete = false,
                    TenantId = tenantId,
                });
            }
        }
        return newList;
    }

    private IEnumerable<SkuUomEntity> AddNewUnits(long tenantId, IEnumerable<string> units, string userName)
    {
        var uomList = _dbContext.GetDbSet<SkuUomEntity>(tenantId);
        var newList = new List<SkuUomEntity>();
        foreach (var unit in units.Where(x => !string.IsNullOrEmpty(x)))
        {
            var hasData = uomList.Any(x => x.UnitName == unit);
            if (!hasData)
            {
                newList.Add(new SkuUomEntity
                {
                    UnitName = unit,
                    TenantId = tenantId
                });
            }
        }

        return newList;
    }

    private IEnumerable<CategoryEntity> AddNewCategories(long tenantId,
        IEnumerable<string> categories, string userName)
    {
        var categoryList = _dbContext.GetDbSet<CategoryEntity>(tenantId);
        var newCategoryList = new List<CategoryEntity>();
        foreach (var category in categories.Where(x => !string.IsNullOrEmpty(x)))
        {
            var hasData = categoryList.Any(x => x.category_name == category);
            if (!hasData)
            {
                newCategoryList.Add(new CategoryEntity
                {
                    category_name = category,
                    create_time = DateTime.UtcNow,
                    creator = userName,
                    is_valid = true,
                    last_update_time = DateTime.UtcNow,
                    TenantId = tenantId
                });
            }
        }
        return newCategoryList;
    }

    /// <summary>
    /// Update sku
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UpdateSkuAsync(int id, UpdateSkuRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            _logger.LogInformation("UpdateSkuRequest is null for id: {Id}", id);
            return (false, "Invalid request");
        }
        var currentTenantId = currentUser.tenant_id;
        var sku = await _dbContext.GetDbSet<SkuEntity>(currentTenantId, isTracking: true)
                                  .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sku is null)
        {
            _logger.LogInformation("Sku is empty with id: {Id}", id);
            return (false, "Sku not found");
        }

        sku.sku_name = request.SkuName ?? sku.sku_name;
        sku.sku_code = request.SkuCode ?? sku.sku_code;
        sku.bar_code = request.BarCode;
        sku.maxQtyPerPallet = request.MaxQtyPerPallet;


        if (request.SkuUomId is not null && request.SkuUomId > 0)
        {
            var skuOum = await _dbContext.GetDbSet<SkuUomEntity>(currentTenantId)
                                    .FirstOrDefaultAsync(u => u.Id == request.SkuUomId.Value, cancellationToken);
            if (skuOum is null)
            {
                _logger.LogInformation("SkuUomEntity is empty with id: {SkuUomId}", request.SkuUomId.Value);
                return (false, "Sku UOM not found");
            }
            var skuUomLink = await _dbContext.GetDbSet<SkuUomLinkEntity>()
                                   .FirstOrDefaultAsync(l => l.SkuId == id, cancellationToken);

            if (skuUomLink != null)
            {
                skuUomLink.SkuUomId = request.SkuUomId.Value;
            }
            else
            {
                var newLink = new SkuUomLinkEntity
                {
                    SkuId = id,
                    SkuUomId = request.SkuUomId.Value,
                };
                await _dbContext.GetDbSet<SkuUomLinkEntity>().AddAsync(newLink, cancellationToken);
            }
            sku.unit = skuOum.UnitName;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, "Sku updated successfully");
    }

    /// <summary>
    /// Search Data
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(IEnumerable<SkuDTO> data, int total)> SearchData(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var tenantID = currentUser.tenant_id;
        var skuList = _dbContext.GetDbSet<SkuEntity>(tenantID);
        if (pageSearch.GetAllSearch)
        {
            var querySku = await skuList.Select(sku => new SkuDTO
            {
                SkuId = sku.Id,
                SkuName = sku.sku_name,
                SkuCode = sku.sku_code
            }).OrderBy(t => t.SkuName)
              .ToListAsync(cancellationToken);

            return (querySku.DistinctBy(x => x.SkuId).ToList(), querySku.Count);
        }

        var baseQuery = _dbContext.Database.SqlQuery<SkuSupplierDTO>(
          $"SELECT * FROM get_sku_inventory_report({tenantID}, {0})"
        );

        Core.DynamicSearch.QueryCollection queries = [];
        if (pageSearch.searchObjects?.Count > 0)
        {
            pageSearch.searchObjects.ForEach(s => queries.Add(s));
            var expression = queries.AsGroupedExpression<SkuSupplierDTO>();

            if (expression != null)
            {
                baseQuery = baseQuery.Where(expression);
            }
        }

        var items = await baseQuery.Select(sku => new SkuDTO
        {
            SkuId = sku.SkuId,
            SkuName = sku.SkuName,
            SkuCode = sku.SkuCode
        })
            .Distinct()
        .ToListAsync(cancellationToken);

        int totals = items.Count;
        if (totals == 0)
        {
            return ([], 0);
        }

        var results = items.OrderBy(t => t.SkuName)
               .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
               .Take(pageSearch.pageSize);

        return (results, totals);
    }
     
    /// <summary>
    /// Get Master SKU Data
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<IEnumerable<SkuMaster>> GetMasterData(CurrentUser currentUser)
    {
        var skuList = _dbContext.GetDbSet<SkuEntity>(currentUser.tenant_id);

        return await skuList.Select(x => new SkuMaster
        {
            SkuId = x.Id,
            SkuCode = x.sku_code,
            SkuName = x.sku_name
        }).ToListAsync();
    }
}
