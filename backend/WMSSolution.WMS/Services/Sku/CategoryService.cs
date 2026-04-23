using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Services.Sku;

/// <summary>
///  Category Service
/// </summary>
/// <remarks>
///Category  constructor
/// </remarks>
/// <param name="dBContext">The DBContext</param>
/// <param name="stringLocalizer">Localizer</param>
public class CategoryService(SqlDBContext dBContext
      , IStringLocalizer<MultiLanguage> stringLocalizer
        ) : BaseService<CategoryEntity>, ICategoryService
{
    #region Args
    /// <summary>
    /// The DBContext
    /// </summary>
    private readonly SqlDBContext _dBContext = dBContext;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    #endregion

    #region Api
    /// <summary>
    /// Get all records
    /// </summary>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<List<CategoryViewModel>> GetAllAsync(CurrentUser currentUser)
    {
        var DbSet = _dBContext.GetDbSet<CategoryEntity>();
        var data = await DbSet.AsNoTracking()
           .Where(t => t.TenantId.Equals(currentUser.tenant_id))
           .ToListAsync();

        var result = data.Adapt<List<CategoryViewModel>>();
        for (int i = 0; i < data.Count; i++)
        {
            if (result[i].id < 1) result[i].id = data[i].Id;
        }

        return result;
    }

    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <returns></returns>
    public async Task<CategoryViewModel> GetAsync(int id)
    {
        var DbSet = _dBContext.GetDbSet<CategoryEntity>();
        var entity = await DbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Id.Equals(id));
        if (entity == null)
        {
            return new CategoryViewModel();
        }
        return entity.Adapt<CategoryViewModel>();
    }
    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    public async Task<(int id, string msg)> AddAsync(CategoryViewModel viewModel, CurrentUser currentUser)
    {
        var DbSet = _dBContext.GetDbSet<CategoryEntity>();
        if (await DbSet.AsNoTracking().AnyAsync(t => t.TenantId.Equals(currentUser.tenant_id) && t.category_name.Equals(viewModel.category_name)))
        {
            return (0, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["category_name"], viewModel.category_name));
        }
        var entity = viewModel.Adapt<CategoryEntity>();
        entity.Id = 0;
        entity.creator = currentUser.user_name;
        entity.create_time = DateTime.UtcNow;
        entity.last_update_time = DateTime.UtcNow;
        entity.TenantId = currentUser.tenant_id;
        entity.is_valid = viewModel.is_valid;
        await DbSet.AddAsync(entity);
        await _dBContext.SaveChangesAsync();
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
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> UpdateAsync(CategoryViewModel viewModel)
    {
        var DbSet = _dBContext.GetDbSet<CategoryEntity>();
        var entity = await DbSet.FirstOrDefaultAsync(t => t.Id.Equals(viewModel.id));
        if (entity == null)
        {
            return (false, _stringLocalizer["not_exists_entity"]);
        }
        if (await DbSet.AsNoTracking().AnyAsync(t => !t.Id.Equals(viewModel.id) && t.TenantId.Equals(entity.TenantId) && t.category_name.Equals(viewModel.category_name)))
        {
            return (false, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["category_name"], viewModel.category_name));
        }
        entity.Id = viewModel.id;
        entity.category_name = viewModel.category_name;
        entity.parent_id = viewModel.parent_id;
        entity.last_update_time = DateTime.UtcNow;
        if (!viewModel.is_valid.Equals(entity.is_valid))
        {
            var entities = await DbSet.Where(t => t.parent_id > 0).ToListAsync();
            List<CategoryEntity> children = [];
            GetChildren(entities, entity.Id, ref children);
            if (children.Count != 0)
            {
                children.ForEach(c =>
                {
                    c.is_valid = viewModel.is_valid;
                    c.last_update_time = DateTime.UtcNow;
                });
            }
        }
        entity.is_valid = viewModel.is_valid;
        var qty = await _dBContext.SaveChangesAsync();
        if (qty > 0)
        {
            return (true, _stringLocalizer["save_success"]);
        }
        else
        {
            return (false, _stringLocalizer["save_failed"]);
        }
    }
    private void GetChildren(List<CategoryEntity> entities, int parentId, ref List<CategoryEntity> children)
    {
        var data = entities.Where(t => t.parent_id == parentId).ToList();
        if (data.Count != 0)
        {
            foreach (var item in data)
            {
                children.Add(item);
                if (entities.Any(t => t.parent_id.Equals(item.Id)))
                {
                    GetChildren(entities, item.parent_id, ref children);
                }
            }
        }
    }
    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    public async Task<(bool flag, string msg)> DeleteAsync(int id)
    {
        var DbSet = _dBContext.GetDbSet<CategoryEntity>();
        var entities = await DbSet.Where(t => t.parent_id.Equals(id)).ToListAsync();
        List<CategoryEntity> children = [];
        GetChildren(entities, id, ref children);
        List<int> idList = [id];
        if (children.Count != 0)
        {
            idList.AddRange([.. children.Select(t => t.Id)]);
        }
        // Check if referenced
        var Spus = _dBContext.GetDbSet<SpuEntity>();
        if (await Spus.AsNoTracking().AnyAsync(t => idList.Contains(t.category_id.GetValueOrDefault())))
        {
            return (false, _stringLocalizer["delete_referenced"]);
        }
        var qty = await _dBContext.GetDbSet<CategoryEntity>().Where(t => idList.Contains(t.Id)).ExecuteDeleteAsync();
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
}

