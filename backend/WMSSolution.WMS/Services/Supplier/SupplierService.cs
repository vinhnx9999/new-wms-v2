
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.DynamicSearch;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.MasterData;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Supplier;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Services.Supplier;

/// <summary>
///  Supplier Service
/// </summary>
/// <remarks>
///Supplier  constructor
/// </remarks>
/// <param name="dBContext">The DBContext</param>
/// <param name="logger"></param>
/// <param name="stringLocalizer">Localizer</param>
public class SupplierService(SqlDBContext dBContext,
    ILogger<SupplierService> logger,
    IStringLocalizer<MultiLanguage> stringLocalizer) :
    BaseService<SupplierEntity>, ISupplierService
{
    #region Args

    private readonly SqlDBContext _dBContext = dBContext;
    private readonly ILogger<SupplierService> _logger = logger;
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    #endregion

    #region Api
    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns></returns>
    public async Task<(List<SupplierVM> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Any())
        {
            pageSearch.searchObjects.ForEach(s =>
            {
                queries.Add(s);
            });
        }
        var query = _dBContext.GetDbSet<SupplierEntity>(currentUser.tenant_id)
                              .Where(t => t.is_valid)
                              .Select(item => new SupplierVM
                              {
                                  Id = item.Id,
                                  SupplierName = item.supplier_name,
                                  City = item.city,
                                  Address = item.address,
                                  Email = item.email,
                                  Manager = item.manager,
                                  ContactTel = item.contact_tel,
                                  Creator = item.creator,
                                  CreateTime = item.create_time
                              });

        var expression = queries.AsGroupedExpression<SupplierVM>();
        if (expression != null)
        {
            query = query.Where(expression);
        }

        int totals = await query.CountAsync(cancellationToken);
        if (totals == 0)
        {
            return ([], totals);
        }
        var list = await query.OrderByDescending(t => t.CreateTime)
                              .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                              .Take(pageSearch.pageSize)
                              .ToListAsync(cancellationToken);
        return (list, totals);
    }

    /// <summary>
    /// Get all records
    /// </summary>
    /// <returns></returns>
    public async Task<List<SupplierViewModel>> GetAllAsync(CurrentUser currentUser)
    {
        var DbSet = _dBContext.GetDbSet<SupplierEntity>();
        List<SupplierEntity>? data = await DbSet.AsNoTracking().Where(t => t.TenantId.Equals(currentUser.tenant_id)).ToListAsync();
        return data.Adapt<List<SupplierViewModel>>();
    }

    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <returns></returns>
    public async Task<SupplierViewModel> GetAsync(int id)
    {
        var DbSet = _dBContext.GetDbSet<SupplierEntity>();
        var entity = await DbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Id.Equals(id));
        if (entity == null)
        {
            return new SupplierViewModel();
        }
        return entity.Adapt<SupplierViewModel>();
    }
    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns></returns>
    public async Task<(int id, string msg)> AddAsync(AddSupplierRequest viewModel, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var DbSet = _dBContext.GetDbSet<SupplierEntity>();
        var entity = viewModel.Adapt<SupplierEntity>();
        entity.Id = 0;
        entity.create_time = DateTime.UtcNow;
        entity.creator = currentUser.user_name;
        entity.last_update_time = DateTime.UtcNow;
        entity.is_valid = true;
        entity.TenantId = currentUser.tenant_id;
        await DbSet.AddAsync(entity, cancellationToken);
        if (await DbSet.AnyAsync(t => t.supplier_name.ToLower() == viewModel.SupplierName.ToLower() && t.TenantId == currentUser.tenant_id, cancellationToken))
        {
            return (0, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["supplier_name"], viewModel.SupplierName));
        }
        await _dBContext.SaveChangesAsync(cancellationToken);
        if (entity.Id > 0)
        {
            return (entity.Id, _stringLocalizer["save_success"]);
        }
        else
        {
            return (0, _stringLocalizer["save_failed"]);
        }
    }
    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="viewModel">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <param name="cancellationToken" >cancellationToken</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UpdateAsync(int id, UpdateSupplierRequest viewModel, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var supplierDbSet = _dBContext.GetDbSet<SupplierEntity>();
        var entity = await supplierDbSet.FirstOrDefaultAsync(t => t.Id.Equals(id) && t.TenantId == currentUser.tenant_id, cancellationToken);
        if (entity is null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }

        entity.supplier_name = viewModel.SupplierName;
        entity.supplier_code = viewModel.SupplierCode;
        entity.city = viewModel.City;
        entity.address = viewModel.Address;
        entity.email = viewModel.Email;
        entity.manager = viewModel.Manager;
        entity.contact_tel = viewModel.ContactTel;
        entity.tax_number = viewModel.TaxNumber;
        entity.last_update_time = DateTime.UtcNow;

        await _dBContext.SaveChangesAsync(cancellationToken);

        return (true, _stringLocalizer["save_success"]);

    }
    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeleteAsync(int id)
    {
        var qty = await _dBContext.GetDbSet<SupplierEntity>().Where(t => t.Id.Equals(id)).ExecuteDeleteAsync();
        if (qty > 0)
        {
            return (true, _stringLocalizer["delete_success"]);
        }
        else
        {
            return (false, _stringLocalizer["delete_failed"]);
        }
    }

    /// <summary>
    /// import suppliers by excel
    /// </summary>
    /// <param name="datas">excel datas</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> ExcelAsync(List<SupplierExcelImportViewModel> datas, CurrentUser currentUser)
    {
        StringBuilder sb = new();
        var DbSet = _dBContext.GetDbSet<SupplierEntity>();
        var supplier_name_repeat_excel = datas.GroupBy(t => t.supplier_name).Select(t => new { supplier_name = t.Key, cnt = t.Count() }).Where(t => t.cnt > 1).ToList();
        foreach (var repeat in supplier_name_repeat_excel)
        {
            sb.AppendLine(string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["supplier_name"], repeat.supplier_name));
        }
        if (supplier_name_repeat_excel.Count > 0)
        {
            return (false, sb.ToString());
        }

        var supplier_name_repeat_exists = await DbSet.Where(t => t.TenantId == currentUser.tenant_id).Where(t => datas.Select(t => t.supplier_name).ToList().Contains(t.supplier_name)).Select(t => t.supplier_name).ToListAsync();
        foreach (var repeat in supplier_name_repeat_exists)
        {
            sb.AppendLine(string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["supplier_name"], repeat));
        }
        if (supplier_name_repeat_exists.Count > 0)
        {
            return (false, sb.ToString());
        }

        var entities = datas.Adapt<List<SupplierEntity>>();
        entities.ForEach(t =>
        {
            t.creator = currentUser.user_name;
            t.TenantId = currentUser.tenant_id;
            t.create_time = DateTime.UtcNow;
            t.last_update_time = DateTime.UtcNow;
            t.is_valid = true;
        });
        await DbSet.AddRangeAsync(entities);
        var res = await _dBContext.SaveChangesAsync();
        if (res > 0)
        {
            return (true, _stringLocalizer["save_success"]);
        }
        return (false, _stringLocalizer["save_failed"]);
    }

    /// <summary>
    /// Import Excel
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> ImportExcelData(List<InputSupplier> request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        int index = 0;
        int insertedCount = 0;
        int totalSuppliers = request.Count;
        var tenantId = currentUser.tenant_id;
        var supplierList = _dBContext.GetDbSet<SupplierEntity>(tenantId);

        var uniqueSuppliers = request.Select(x => new InputExcelBase
        {
            Code = x.Code.Trim(),
            Property = x.TaxNumber.Trim()
        })
            .GroupBy(x => new { x.Code, x.Property })
            .Select(g => g.First());

        do
        {
            var items = uniqueSuppliers
                .Skip(index).Take(SystemDefine.BatchSize)
                .Select(x =>
                {
                    var lastSupplier = request
                        .LastOrDefault(s => s.TaxNumber == x.Property && s.Code == x.Code);

                    return new SupplierEntity
                    {
                        supplier_code = x.Code,
                        create_time = DateTime.UtcNow,
                        address = lastSupplier?.Address ?? "",
                        contact_tel = lastSupplier?.Phone ?? "",
                        creator = currentUser.user_name,
                        supplier_name = lastSupplier?.Name ?? "",
                        tax_number = x.Property,
                        is_valid = true,
                        TenantId = tenantId
                    };
                }).ToList();

            try
            {
                var res = await SaveNewSuppliersAsync(items, supplierList);
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

        } while (index < totalSuppliers);

        return insertedCount;
    }

    private async Task<int> SaveNewSuppliersAsync(List<SupplierEntity> items, IQueryable<SupplierEntity> supplierList)
    {
        var newSuppliers = new List<SupplierEntity>();
        foreach (var item in items)
        {
            bool exists = await supplierList.AnyAsync(s => s.supplier_name == item.supplier_name);
            if (!exists)
            {
                newSuppliers.Add(item);
            }
        }

        _dBContext.GetDbSet<SupplierEntity>().AddRange(newSuppliers);
        return await _dBContext.SaveChangesAsync();
    }

    /// <summary>
    /// Get Master Data
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ChainMaster>> GetMasterData(CurrentUser currentUser)
    {
        var query = _dBContext.GetDbSet<SupplierEntity>(currentUser.tenant_id)
            .Where(x => x.is_valid);

        return await query.Select(x => new ChainMaster
        {
            Id = x.Id,
            Code = x.supplier_code,
            Name = x.supplier_name
        }).ToListAsync();
    }

    #endregion
}

