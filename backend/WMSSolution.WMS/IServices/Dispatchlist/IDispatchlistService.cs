/*
 * date：2022-12-27
 * developer：NoNo
 */
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Dispatchlist.Duy_Phat_Solution;

namespace WMSSolution.WMS.IServices
{
    /// <summary>
    /// Interface of DispatchlistService
    /// </summary>
    public interface IDispatchlistService : IBaseService<DispatchlistEntity>
    {
        #region Api
        /// <summary>
        /// page search
        /// </summary>
        /// <param name="pageSearch">args</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(List<DispatchlistViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser);

        /// <summary>
        /// advanced dispatch order page search 
        /// </summary>
        /// <param name="pageSearch">args</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(List<PreDispatchlistViewModel> data, int totals)> AdvancedDispatchlistPageAsync(PageSearch pageSearch, CurrentUser currentUser);

        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> AddAsync(List<DispatchlistAddViewModel> viewModel, CurrentUser currentUser);

        /// <summary>
        /// Dispatchlist details with available stock
        /// </summary>
        /// <param name="dispatch_no">dispatch_no</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<List<DispatchlistConfirmDetailViewModel>> ConfirmOrderCheck(string dispatch_no, CurrentUser currentUser);

        /// <summary>
        /// get dispatchlist by dispatch_no
        /// </summary>
        /// <param name="dispatch_no"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<List<DispatchlistViewModel>> GetByDispatchlistNo(string dispatch_no, CurrentUser currentUser);

        /// <summary>
        /// delete a record
        /// </summary>
        /// <param name="dispatch_no">dispatch_no</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> DeleteAsync(string dispatch_no, CurrentUser currentUser);

        /// <summary>
        /// update dispatchlist with same dispatch_no
        /// </summary>
        /// <param name="viewModels"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<(bool flag, string msg)> UpdateAsycn(List<DispatchlistViewModel> viewModels, CurrentUser currentUser);

        /// <summary>
        ///  Confirm orders and create  dispatchpicklist
        /// </summary>
        /// <param name="viewModels">viewModels</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> ConfirmOrder(List<DispatchlistConfirmDetailViewModel> viewModels, CurrentUser currentUser);

        /// <summary>
        /// confirm dispatchpicklist picked by dispatch_no
        /// </summary>
        /// <param name="dispatch_no">dispatch_no</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> ConfirmPickByDispatchNo(string dispatch_no, CurrentUser currentUser);

        /// <summary>
        /// confirm pick detail
        /// </summary>
        /// <param name="picklist_id">dispatch list pick detail id</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> ConfirmPickDetail(List<int> picklist_id, CurrentUser currentUser);

        /// <summary>
        /// cancel confirm pick detail
        /// </summary>
        /// <param name="picklist_id">dispatch list pick detail id</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> CancelConfirmPickDetail(List<int> picklist_id, CurrentUser currentUser);

        /// <summary>
        ///  package
        /// </summary>
        /// <param name="viewModels">viewModels</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        Task<(bool flag, string msg)> Package(List<DispatchlistPackageViewModel> viewModels, CurrentUser currentUser);

        /// <summary>
        ///  weight
        /// </summary>
        /// <param name="viewModels">viewModels</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        Task<(bool flag, string msg)> Weight(List<DispatchlistWeightViewModel> viewModels, CurrentUser currentUser);

        /// <summary>
        /// dispatchpicklist outbound delivery
        /// </summary>
        /// <param name="viewModels">viewModels</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        Task<(bool flag, string msg)> Delivery(List<DispatchlistDeliveryViewModel> viewModels, CurrentUser currentUser);

        /// <summary>
        ///  set dispatchlist freightfee
        /// </summary>
        /// <param name="viewModels"></param>
        /// <returns></returns>
        Task<(bool flag, string msg)> SetFreightfee(List<DispatchlistFreightfeeViewModel> viewModels);

        /// <summary>
        /// sign for arrival
        /// </summary>
        /// <param name="viewModels">viewModels</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> SignForArrival(List<DispatchlistSignViewModel> viewModels);

        /// <summary>
        /// get pick list by dispatch_id
        /// </summary>
        /// <param name="dispatch_id">dispatch_id</param>
        /// <returns></returns>
        Task<List<DispatchpicklistViewModel>> GetPickListByDispatchID(int dispatch_id);

        /// <summary>
        ///  cancel order opration 
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">current user</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> CancelOrderOpration(CancelOrderOprationViewModel viewModel, CurrentUser currentUser);

        /// <summary>
        /// cancel dispatchlist detail opration
        /// </summary>
        /// <param name="id">dispatchlist_id</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> CancelDispatchlistDetailOpration(int id);


        /// <summary>
        /// Excel Import
        /// </summary>
        /// <param name="viewModels">viewModels</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> Import(List<DispatchlistImportViewModel> viewModels, CurrentUser currentUser);

        #region Duy Phat Solution

        /// <summary>
        /// Creates a dispatch order in draft status (status = 0) with details and picklists.
        /// The order can be reviewed and modified before execution.
        /// </summary>
        /// <param name="request">The dispatch order request containing customer info and line items</param>
        /// <returns>Tuple of (dispatchId, message)</returns>
        Task<(int dispatchId, string message)> CreateDispatchDraftAsync(CreateDispatchOrderRequest request);

        /// <summary>
        /// Executes a draft dispatch order by changing its status to Processing (status = 2)
        /// and creating outbound tasks for WCS integration.
        /// </summary>
        /// <param name="id">The dispatch header ID to execute</param>
        /// <param name="currentUser">Current user context</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> ExecuteDispatchAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Creates and immediately executes a dispatch order in a single atomic operation.
        /// Combines CreateDispatchDraftAsync and ExecuteDispatchAsync into one optimized transaction.
        /// </summary>
        /// <param name="request">The dispatch order request</param>
        /// <param name="currentUser">Current user context</param>
        /// <returns>Tuple of (dispatchId, success, message)</returns>
        Task<(int dispatchId, bool success, string message)> CreateAndExecuteDispatchAsync(
            CreateDispatchOrderRequest request,
            CurrentUser currentUser);

        #endregion

        #endregion
    }
}

