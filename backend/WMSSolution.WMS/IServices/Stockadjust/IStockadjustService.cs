using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Stockadjust;

namespace WMSSolution.WMS.IServices.Stockadjust;

/// <summary>
/// Interface of Stock adjust Service
/// </summary>
public interface IStockadjustService : IBaseService<StockadjustEntity>
{
    #region Api
    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    Task<(List<StockadjustViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser);
    /// <summary>
    /// Get all records
    /// </summary>
    /// <returns></returns>
    Task<List<StockadjustViewModel>> GetAllAsync(CurrentUser currentUser);
    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <param name="id">primary key</param>
    /// <returns></returns>
    Task<StockadjustViewModel?> GetAsync(int id);
    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    Task<(int total, string msg)> AddAsync(StockAdjustRequest viewModel, CurrentUser currentUser);
    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> UpdateAsync(StockadjustViewModel viewModel);

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> DeleteAsync(int id);

    /// <summary>
    /// confirm adjustment
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> ConfirmAdjustment(int id);

    #endregion

    /// <summary>
    /// Asynchronously retrieves a list of available stock sources that match the specified keyword for use in a
    /// change request.
    /// </summary>
    /// <param name="pageSearch">The keyword to filter stock sources by. This value is used to search for relevant stock sources. Can be null
    /// or empty to retrieve all available sources.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
    /// cref="StockSourceSelectionViewModel"/> objects matching the keyword. The list will be empty if no matching
    /// stock sources are found.</returns>
    /// <summary>
    /// Get Stock Sources with detailed Lock info for Adjustment Selection
    /// </summary>
    Task<(List<SkuAdjustmentSelectionViewModel> data, int totals)> GetSkuForAdjustmentSelectionAsync(PageSearch pageSearch);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <returns></returns>
    Task<(List<StockSourceSelectionViewModel> data, int totals)> GetStockSourcesForChangeRequestAsync(PageSearch pageSearch);

    /// <summary>
    /// confirm processing
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(bool flag, string msg)> ConfirmProcess(int id, CurrentUser currentUser);

    /// <summary>
    /// confirm adjustment
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> ConfirmAdjustment(int id, CurrentUser currentUser);

    /// <summary>
    /// Get By Processing Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<StockadjustViewModel?> GetByProcessingIdAsync(int id);

    /// <summary>
    /// Processing
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <returns></returns>
    Task<(List<StockprocessGetViewModel> data, int totals)> PageProcessingAsync(PageSearch pageSearch);
    /// <summary>
    /// Delete processing
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<(bool flag, string msg)> DeleteProcessingAsync(int id);
}

