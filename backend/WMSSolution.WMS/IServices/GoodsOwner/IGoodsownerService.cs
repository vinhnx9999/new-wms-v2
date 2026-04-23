using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;

namespace WMSSolution.WMS.IServices
{
    /// <summary>
    /// Interface of GoodsownerService
    /// </summary>
    public interface IGoodsownerService : IBaseService<GoodsownerEntity>
    {
        #region Api
        /// <summary>
        /// page search
        /// </summary>
        /// <param name="pageSearch">args</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(List<GoodsownerViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser);
        /// <summary>
        /// Get all records
        /// </summary>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<List<GoodsownerViewModel>> GetAllAsync(CurrentUser currentUser);
        /// <summary>
        /// Get a record by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        Task<GoodsownerViewModel> GetAsync(int id);
        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">args</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(int id, string msg)> AddAsync(GoodsownerViewModel viewModel, CurrentUser currentUser);
        /// <summary>
        /// update a record
        /// </summary>
        /// <param name="viewModel">args</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> UpdateAsync(GoodsownerViewModel viewModel);

        /// <summary>
        /// delete a record
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> DeleteAsync(int id);
        #endregion

        #region Import
        /// <summary>
        /// import goodsowners by excel
        /// </summary>
        /// <param name="input">excel data</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(bool flag, List<GoodsownerImportViewModel> errorData)> ExcelAsync(List<GoodsownerImportViewModel> input, CurrentUser currentUser);
        #endregion
    }
}
