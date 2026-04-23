using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.IServices.Sku;

namespace WMSSolution.WMS.Services.Sku
{
    /// <summary>
    /// Sku Uom service
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="stringLocalizer"></param>
    public class SkuUomService(SqlDBContext dbContext, IStringLocalizer<MultiLanguage> stringLocalizer) : BaseService<SkuUomEntity>, ISkuUomService
    {
        private readonly SqlDBContext _dbContext = dbContext;
        private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

        #region Dat
        public async Task<List<SkuUomDTO>> GetAllAsync(long tenantId)
        {
            var dbSet = _dbContext.GetDbSet<SkuUomEntity>();
            var data = await dbSet.AsNoTracking().Where(t => t.TenantId == tenantId).ToListAsync();
            return data.Adapt<List<SkuUomDTO>>();
        }

        public async Task<SkuUomDTO> GetAsync(int id, long tenantId)
        {
            var dbSet = _dbContext.GetDbSet<SkuUomEntity>();
            var entity = await dbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
            if (entity == null) return new SkuUomDTO();
            return entity.Adapt<SkuUomDTO>();
        }

        public async Task<(int id, string msg)> AddAsync(SkuUomDTO viewModel, long tenantId)
        {
            var dbSet = _dbContext.GetDbSet<SkuUomEntity>();

            if (await dbSet.AsNoTracking().AnyAsync(t => t.TenantId == tenantId && t.UnitName == viewModel.UnitName))
            {
                return (0, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["UnitName"], viewModel.UnitName));
            }

            var entity = new SkuUomEntity
            {
                UnitName = viewModel.UnitName,                
                TenantId = tenantId
            };

            await dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            if (entity.Id > 0)
                return (entity.Id, _stringLocalizer["save_success"]);

            return (0, _stringLocalizer["save_failed"]);
        }

        public async Task<(bool flag, string msg)> UpdateAsync(SkuUomDTO viewModel, long tenantId)
        {
            var dbSet = _dbContext.GetDbSet<SkuUomEntity>();
            var entity = await dbSet.FirstOrDefaultAsync(t => t.Id == viewModel.Id && t.TenantId == tenantId);

            if (entity == null)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }

            if (await dbSet.AsNoTracking().AnyAsync(t => t.Id != viewModel.Id && t.TenantId == tenantId && t.UnitName == viewModel.UnitName))
            {
                return (false, string.Format(_stringLocalizer["exists_entity"], _stringLocalizer["UnitName"], viewModel.UnitName));
            }

            entity.UnitName = viewModel.UnitName;
            entity.Description = viewModel.Description;

            var qty = await _dbContext.SaveChangesAsync();
            if (qty > 0)
                return (true, _stringLocalizer["save_success"]);

            return (false, _stringLocalizer["save_failed"]);
        }

        public async Task<(bool flag, string msg)> DeleteAsync(int id, long tenantId)
        {
            var dbSet = _dbContext.GetDbSet<SkuUomEntity>();
            var entity = await dbSet.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

            if (entity == null)
            {
                return (false, _stringLocalizer["not_exists_entity"]);
            }

            // check if reference?
            var skuUomLinkSet = _dbContext.GetDbSet<SkuUomLinkEntity>();
            if (await skuUomLinkSet.AsNoTracking().AnyAsync(t => t.SkuUomId == id))
            {
                return (false, _stringLocalizer["delete_referenced"]);
            }

            dbSet.Remove(entity);

            var qty = await _dbContext.SaveChangesAsync();
            if (qty > 0)
                return (true, _stringLocalizer["delete_success"]);

            return (false, _stringLocalizer["delete_failed"]);
        }
        #endregion
    }
}
