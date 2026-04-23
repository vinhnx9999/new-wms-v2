using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels;

namespace WMSSolution.WMS.IServices.Sku
{
    public interface ISkuUomService : IBaseService<SkuUomEntity>
    {
        #region Dat
        Task<List<SkuUomDTO>> GetAllAsync(long tenantId);
        Task<SkuUomDTO> GetAsync(int id, long tenantId);
        Task<(int id, string msg)> AddAsync(SkuUomDTO viewModel, long tenantId);
        Task<(bool flag, string msg)> UpdateAsync(SkuUomDTO viewModel, long tenantId);
        Task<(bool flag, string msg)> DeleteAsync(int id, long tenantId);
        #endregion
    }
}
