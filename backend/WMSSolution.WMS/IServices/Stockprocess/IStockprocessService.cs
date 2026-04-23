/*
 * date：2022-12-23
 * developer：NoNo
 */
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Stockprocess;

namespace WMSSolution.WMS.IServices
{
    /// <summary>
    /// Interface of StockprocessService
    /// </summary>
    public interface IStockprocessService : IBaseService<StockprocessEntity>
    {
        #region Api
        /// <summary>
        /// page search
        /// </summary>
        /// <param name="pageSearch">args</param>
        /// <returns></returns>
        Task<(List<StockprocessGetViewModel> data, int totals)> PageAsync(PageSearch pageSearch);
        /// <summary>
        /// Get all records
        /// </summary>
        /// <returns></returns>
        Task<List<StockprocessGetViewModel>> GetAllAsync(CurrentUser currentUser);
        /// <summary>
        /// Get a record by id
        /// </summary>
        /// <param name="id">primary key</param>
        /// <returns></returns>
        Task<StockprocessWithDetailViewModel> GetAsync(int id);
        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(int id, string msg)> AddAsync(StockprocessViewModel viewModel, CurrentUser currentUser);
        /// <summary>
        /// update a record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> UpdateAsync(StockprocessViewModel viewModel);

        /// <summary>
        /// delete a record
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> DeleteAsync(int id);

        /// <summary>
        /// confirm processing
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> ConfirmProcess(int id, CurrentUser currentUser);


        /// <summary>
        /// confirm adjustment
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> ConfirmAdjustment(int id, CurrentUser currentUser);
        
        #endregion

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        /// <returns></returns>
        Task<StockProcessDashboardStatsViewModel> GetDashboardStatsAsync();
    }
}

