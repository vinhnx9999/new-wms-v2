
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;

using WMSSolution.WMS.Entities.ViewModels.Asn;

namespace WMSSolution.WMS.IServices
{
    /// <summary>
    /// Interface of AsnService
    /// </summary>
    public interface IAsnService : IBaseService<AsnEntity>
    {
        #region Api
        /// <summary>
        /// page search
        /// </summary>
        /// <param name="pageSearch">args</param>
        /// <param name="currentUser">current user</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns></returns>
        Task<(List<AsnViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a record by id
        /// </summary>
        /// <param name="id">primary key</param>
        /// <returns></returns>
        Task<AsnViewModel> GetAsync(int id);
        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(int id, string msg)> AddAsync(AsnViewModel viewModel, CurrentUser currentUser);
        /// <summary>
        /// update a record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> UpdateAsync(AsnViewModel viewModel);

        /// <summary>
        /// delete a record
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> DeleteAsync(int id);

        /// <summary>
        /// Bulk modify Goodsowner
        /// </summary>
        /// <param name="viewModel">args</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> BulkModifyGoodsownerAsync(AsnBulkModifyGoodsOwnerViewModel viewModel);
        #endregion

        #region Flow Api
        /// <summary>
        /// Confirm Delivery
        /// change the asn_status from 0 to 1
        /// </summary>
        /// <param name="viewModels">args</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> ConfirmAsync(List<AsnConfirmInputViewModel> viewModels);

        /// <summary>
        /// Cancel confirm, change asn_status 1 to 0
        /// </summary>
        /// <param name="idList">id list</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> ConfirmCancelAsync(List<int> idList);

        /// <summary>
        /// Unload
        /// change the asn_status from 1 to 2
        /// </summary>
        /// <param name="viewModels">args</param>
        /// <param name="user">user</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> UnloadAsync(List<AsnUnloadInputViewModel> viewModels, CurrentUser user);

        /// <summary>
        /// Cancel unload
        /// change the asn_status from 2 to 1
        /// </summary>
        /// <param name="idList">id list</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> UnloadCancelAsync(List<int> idList);

        /// <summary>
        /// sorting， add a new asnsort record and update asn sorted_qty
        /// </summary>
        /// <param name="viewModels">args</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> SortingAsync(List<AsnsortInputViewModel> viewModels, CurrentUser currentUser);

        /// <summary>
        /// get asnsorts list by asn_id
        /// </summary>
        /// <param name="asn_id">asn id</param>
        /// <returns></returns>
        Task<List<AsnsortViewModel>> GetAsnsortsAsync(int asn_id);

        /// <summary>
        /// update or delete asnsorts data
        /// </summary>
        /// <param name="entities">data</param>
        /// <param name="user">CurrentUser</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> ModifyAsnsortsAsync(List<AsnsortEntity> entities, CurrentUser user);

        /// <summary>
        /// Sorted
        /// change the asn_status from 2 to 3
        /// </summary>
        /// <param name="idList">id list</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> SortedAsync(List<int> idList);

        /// <summary>
        /// Cancel sorted
        /// change the asn_status from 3 to 2
        /// </summary>
        /// <param name="idList">id list</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> SortedCancelAsync(List<int> idList);

        /// <summary>
        /// get pending putaway data by asn_id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<List<AsnPendingPutawayViewModel>> GetPendingPutawayDataAsync(int id);

        /// <summary>
        /// PutAway
        /// </summary>
        /// <param name="viewModels">args</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> PutAwayAsync(List<AsnPutAwayInputViewModel> viewModels, CurrentUser currentUser);

        /// <summary>
        /// Confirm Robot has Success To putaway
        /// the status has changes from 4 to 5
        /// This method is temporary for bypass the robot status 
        /// this method will be update soon in Intergration with WCS phase 
        /// </summary>
        /// <param name="idList"></param>
        /// <param name="tenantId">current user</param>
        /// <returns></returns>
        Task<bool> ConfirmRobotHasSuccessAsync(List<int> idList, long tenantId = 1);

        #endregion


        #region Arrival list 
        /// <summary>
        /// Arrival list
        /// </summary>
        /// <param name="pageSearch">args</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(List<AsnmasterBothViewModel> data, int totals)> PageAsnMasterAsync(PageSearch pageSearch, CurrentUser currentUser);
        /// <summary>
        /// get Arrival list
        /// </summary>
        /// <param name="id"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<AsnmasterBothViewModel> GetAsnmasterAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(int id, string msg)> AddAsnmasterAsync(AsnmasterBothViewModel viewModel, CurrentUser currentUser);
        /// <summary>
        /// add a new record
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> UpdateAsnmasterAsync(AsnmasterBothViewModel viewModel, CurrentUser currentUser);

        /// <summary>
        /// delete a record
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        Task<(bool flag, string msg)> DeleteAsnmasterAsync(int id);

        /// <summary>
        /// Save draft ASN master (create ASN master + details only)
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(int id, string msg)> SaveDraftAsync(AsnmasterBothViewModel viewModel, CurrentUser currentUser);

        /// <summary>
        /// Submit ASN master (create ASN master + details + receipt + integration)
        /// </summary>
        /// <param name="viewModel">viewmodel</param>
        /// <param name="currentUser">currentUser</param>
        /// <returns></returns>
        Task<(int id, string msg)> SubmitOrderAsync(AsnmasterBothViewModel viewModel, CurrentUser currentUser);

        /// <summary>
        /// Mark an ASN master order as completed and create WCS tasks/history.
        /// </summary>
        /// <param name="id">asn master id</param>
        /// <param name="currentUser">current user</param>
        /// <returns>(flag, message)</returns>
        Task<(bool flag, string msg)> UpdateOrderToCompletedAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Created AsnNo by asn no generator
        /// </summary>
        /// <returns></returns>
        Task<string> GetNextAsnNo();

        /// <summary>
        /// Retry Inbound with new location
        /// </summary>
        /// <param name="request"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<(bool, string)> SubmitInboundRetryAsync(RetryInboundItemRequest request, CurrentUser currentUser);
        #endregion

        #region print series number
        /// <summary>
        /// print series number
        /// </summary>
        /// <param name="input">selected asn id</param>
        /// <returns></returns>
        Task<List<AsnPrintSeriesNumberViewModel>> GetAsnPrintSeriesNumberAsync(List<int> input);
        #endregion

        #region count Record
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<Dictionary<int, int>> GetAsnStatusTotalRecordAsync(CurrentUser currentUser);
        #endregion
    }
}

