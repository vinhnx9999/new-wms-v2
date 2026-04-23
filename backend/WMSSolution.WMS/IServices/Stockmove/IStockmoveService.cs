/*
 * date：2022-12-27
 * developer：NoNo
 */
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;

namespace WMSSolution.WMS.IServices
{
    /// <summary>
    /// Interface of StockmoveService
    /// </summary>
    public interface IStockmoveService : IBaseService<StockmoveEntity>
    {
        #region Api
        /// <summary>
        /// page search
        /// </summary>
        /// <param name="pageSearch">args</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(List<StockmoveViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser);
        /// <summary>
        /// Get all records
        /// </summary>
        /// <returns></returns>
        Task<List<StockmoveViewModel>> GetAllAsync(CurrentUser currentUser);
        /// <summary>
        /// Get a record by id
        /// </summary>
        /// <param name="id">primary key</param>
        /// <returns></returns>
        Task<StockmoveViewModel> GetAsync(int id);
        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(int id, string msg)> AddAsync(StockmoveViewModel viewModel, CurrentUser currentUser);
        /// <summary>
        /// confirm move
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> Confirm(int id, CurrentUser currentUser);

        /// <summary>
        /// delete a record
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> DeleteAsync(int id);

        /// <summary>
        /// Get dashboard stats
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<StockMoveDashboardStats> GetDashboardStatsAsync(CurrentUser currentUser);
        #endregion
    }
}

