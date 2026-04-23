using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
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
using WMSSolution.WMS.Entities.ViewModels.Customer;
using WMSSolution.WMS.IServices.Customer;

namespace WMSSolution.WMS.Services.Customer;

/// <summary>
/// customer Service
/// </summary>
/// <remarks>
/// company service constructor
/// </remarks>
/// <param name="dbContext">The DBContext</param>
/// <param name="stringLocalizer">Localization</param>
/// <param name="logger">logger</param>
public class CustomerService(
    SqlDBContext dbContext,
    IStringLocalizer<MultiLanguage> stringLocalizer,
    ILogger<CustomerService> logger
        ) : BaseService<CustomerEntity>, ICustomerService
{
    #region Args
    /// <summary>
    /// The DBContext
    /// </summary>
    private readonly SqlDBContext _dbContext = dbContext;

    /// <summary>
    /// Localization
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    /// <summary>
    /// logger
    /// </summary>
    private readonly ILogger<CustomerService> _logger = logger;

    #endregion

    #region Api
    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    public async Task<(List<CustomerResponseViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        QueryCollection queries = [];
        if (pageSearch.searchObjects.Count != 0)
        {
            pageSearch.searchObjects.ForEach(queries.Add);
        }

        var query = _dbContext.GetDbSet<CustomerEntity>(currentUser.tenant_id);
        var dtoQuery = query.Select(t => new CustomerResponseViewModel
        {
            ID = t.Id,
            CustomerName = t.customer_name,
            City = t.city,
            Address = t.address,
            Manager = t.manager,
            Email = t.email,
            ContactTel = t.contact_tel,
            Creator = t.creator,
            CreateTime = t.create_time,
            LastUpdateTime = t.last_update_time,
            IsValid = t.is_valid,
        });

        var expression = queries.AsExpression<CustomerResponseViewModel>();

        if (expression != null)
        {
            dtoQuery = dtoQuery.Where(expression);
        }

        int totals = await dtoQuery.CountAsync(cancellationToken);

        if (totals == 0)
        {
            return ([], 0);
        }

        var data = await dtoQuery.OrderByDescending(t => t.CreateTime)
                   .Skip((pageSearch.pageIndex - 1) * pageSearch.pageSize)
                   .Take(pageSearch.pageSize)
                   .ToListAsync(cancellationToken);

        return (data, totals);
    }

    /// <summary>
    /// Get all records
    /// </summary>
    /// <returns></returns>
    public async Task<List<CustomerViewModel>> GetAllAsync(CurrentUser currentUser)
    {
        var dbSet = _dbContext.GetDbSet<CustomerEntity>(currentUser.tenant_id);
        var data = await dbSet
            .OrderByDescending(t => t.create_time)
            .ToListAsync();

        var rs = data.Adapt<List<CustomerViewModel>>();
        for (int i = 0; i < data.Count; i++)
        {
            if (rs[i].id > 0) continue;
            rs[i].id = data[i].Id;

        }

        return rs;
    }

    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    public async Task<CustomerViewModel> GetAsync(int id)
    {
        var entity = await _dbContext.GetDbSet<CustomerEntity>().AsNoTracking().FirstOrDefaultAsync(t => t.Id.Equals(id));
        if (entity != null)
        {
            return entity.Adapt<CustomerViewModel>();
        }
        else
        {
            return new CustomerViewModel();
        }
    }

    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(int id, string msg)> AddAsync(CustomerResponseViewModel viewModel, CurrentUser currentUser)
    {
        var DbSet = _dbContext.GetDbSet<CustomerEntity>();
        if (await DbSet.AnyAsync(t => t.TenantId.Equals(currentUser.tenant_id) && t.customer_name.Equals(viewModel.CustomerName)))
        {
            return (0, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["customer_name"], viewModel.CustomerName));
        }
        var entity = new CustomerEntity
        {
            Id = 0,
            creator = currentUser.user_name,
            create_time = DateTime.UtcNow,
            last_update_time = DateTime.UtcNow,
            TenantId = currentUser.tenant_id,
            customer_name = viewModel.CustomerName,
            city = viewModel.City,
            address = viewModel.Address,
            manager = viewModel.Manager,
            email = viewModel.Email,
            contact_tel = viewModel.ContactTel,
            is_valid = viewModel.IsValid
        };

        await DbSet.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
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
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UpdateAsync(int id, UpdateCustomerRequest viewModel, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var customerDbSet = _dbContext.GetDbSet<CustomerEntity>();
        var entity = await customerDbSet.FirstOrDefaultAsync(t => t.Id.Equals(id) && t.TenantId == currentUser.tenant_id, cancellationToken);
        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }
        //if (await DbSet.AnyAsync(t => !t.Id.Equals(viewModel.id) && t.TenantId.Equals(entity.TenantId) && t.customer_name.Equals(viewModel.customer_name)))
        //{
        //    return (false, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["customer_name"], viewModel.customer_name));
        //}
        entity.customer_name = viewModel.CustomerName;
        entity.city = viewModel.City;
        entity.address = viewModel.Address;
        entity.email = viewModel.Email;
        entity.manager = viewModel.Manager;
        entity.contact_tel = viewModel.ContactTel;
        entity.tax_number = viewModel.TaxNumber;
        entity.customer_code = viewModel.CustomerCode;
        entity.last_update_time = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return (true, _stringLocalizer["save_success"]);

    }
    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeleteAsync(int id)
    {
        var Dispatchlists = _dbContext.GetDbSet<DispatchlistEntity>();
        if (await Dispatchlists.AsNoTracking().AnyAsync(t => t.customer_id.Equals(id)))
        {
            return (false, _stringLocalizer["delete_referenced"]);
        }
        var qty = await _dbContext.GetDbSet<CustomerEntity>().Where(t => t.Id.Equals(id)).ExecuteDeleteAsync();
        if (qty > 0)
        {
            return (true, _stringLocalizer["delete_success"]);
        }
        else
        {
            return (false, _stringLocalizer["delete_failed"]);
        }
    }

    #endregion

    #region Import
    /// <summary>
    /// import customers by excel
    /// </summary>
    /// <param name="input">excel data</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(bool flag, List<CustomerImportViewModel> errorData)> ExcelAsync(List<CustomerImportViewModel> input, CurrentUser currentUser)
    {
        var DbSet = _dbContext.GetDbSet<CustomerEntity>();
        var existsDatas = await DbSet.AsNoTracking().Where(t => t.TenantId.Equals(currentUser.tenant_id)).Select(t => new { t.customer_name }).ToListAsync();
        input.ForEach(async t =>
        {
            t.errorMsg = string.Empty;
            if (existsDatas.Any(d => d.customer_name.Equals(t.customer_name)))
            {
                t.errorMsg = string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["customer_name"], t.customer_name);
            }
            else
            {
                await DbSet.AddAsync(new CustomerEntity
                {
                    customer_name = t.customer_name,
                    city = t.city,
                    address = t.address,
                    email = t.email,
                    manager = t.manager,
                    contact_tel = t.contact_tel,
                    creator = currentUser.user_name,
                    create_time = DateTime.UtcNow,
                    last_update_time = DateTime.UtcNow,
                    is_valid = true,
                    TenantId = currentUser.tenant_id
                });
            }
        });
        if (input.Any(t => t.errorMsg.Length > 0))
        {
            return (false, input.Where(t => t.errorMsg.Length > 0).ToList());
        }
        var qty = await _dbContext.SaveChangesAsync();
        if (qty > 0)
        {
            return (true, new List<CustomerImportViewModel>());
        }
        else
        {
            return (false, new List<CustomerImportViewModel>());
        }
    }

    /// <summary>
    /// Import Excel
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<int> ImportExcelData(List<InputCustomer> request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        int index = 0;
        int insertedCount = 0;
        int totalCustomers = request.Count;
        var tenantId = currentUser.tenant_id;
        var customerList = _dbContext.GetDbSet<CustomerEntity>(tenantId);

        var uniqueCustomers = request.Select(x => new InputExcelBase
        {
            Code = x.Code.Trim(),
            Property = x.TaxNumber.Trim()
        })
            .GroupBy(x => new { x.Code, x.Property })
            .Select(g => g.First());

        do
        {
            var items = uniqueCustomers
                .Skip(index).Take(SystemDefine.BatchSize)
                .Select(x =>
                {
                    var lastCustomer = request
                        .LastOrDefault(s => s.TaxNumber == x.Property && s.Code == x.Code);

                    return new CustomerEntity
                    {
                        customer_code = x.Code,
                        create_time = DateTime.UtcNow,
                        address = lastCustomer?.Address ?? "",
                        contact_tel = lastCustomer?.Phone ?? "",
                        creator = currentUser.user_name,
                        customer_name = lastCustomer?.Name ?? "",
                        tax_number = x.Property,
                        is_valid = true,
                        TenantId = tenantId
                    };
                }).ToList();

            try
            {
                var res = await SaveNewCustomersAsync(items, customerList);
                insertedCount += res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting batch starting at index {index}", index);
                // Optionally, you can choose to break the loop or continue with the next batch
                // break;
            }

            // Xử lý batch
            Console.WriteLine($"Batch starting at {index}, count = {items.Count}");
            // Tăng index lên batchSize
            index += SystemDefine.BatchSize;

        } while (index < totalCustomers);

        return insertedCount;
    }

    private async Task<int> SaveNewCustomersAsync(List<CustomerEntity> items, IQueryable<CustomerEntity> customerList)
    {
        var newCustomers = new List<CustomerEntity>();
        foreach (var item in items)
        {
            bool exists = await customerList.AnyAsync(s => s.customer_name == item.customer_name);
            if (!exists)
            {
                newCustomers.Add(item);
            }
        }

        _dbContext.GetDbSet<CustomerEntity>().AddRange(newCustomers);
        return await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Get Master Customer Data
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ChainMaster>> GetMasterData(CurrentUser currentUser)
    {
        var query = _dbContext.GetDbSet<CustomerEntity>(currentUser.tenant_id)
            .Where(x => x.is_valid);

        return await query.Select(x => new ChainMaster
        {
            Id = x.Id,
            Code = x.customer_code,
            Name = x.customer_name
        }).ToListAsync();
    }

    #endregion
}
