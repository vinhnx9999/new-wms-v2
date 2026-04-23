using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.MasterData;
using WMSSolution.Shared.Planning;
using WMSSolution.WMS.Entities.Models.Warehouse;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Dashboard;
using WMSSolution.WMS.Entities.ViewModels.Warehouse;
using WMSSolution.WMS.Entities.ViewModels.Warehouse.Invertory;

namespace WMSSolution.WMS.IServices.Warehouse;

/// <summary>
/// Interface of WarehouseService
/// </summary>
public interface IWarehouseService : IBaseService<WarehouseEntity>
{
    #region Api
    /// <summary>
    /// get select items
    /// </summary>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    Task<List<FormSelectItem>> GetSelectItemsAsnyc(CurrentUser currentUser);
    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    Task<(List<WarehouseViewModel> data, int totals)> PageAsync(PageSearch pageSearch, 
        CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get all records
    /// </summary>
    /// <returns></returns>
    Task<List<WarehouseViewModel>> GetAllAsync(CurrentUser currentUser);
    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <param name="id">primary key</param>
    /// <returns></returns>
    Task<WarehouseViewModel> GetAsync(int id);
    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellation">cancellation token</param>
    /// <returns></returns>
    Task<(int id, string msg)> AddAsync(WarehouseVM viewModel, CurrentUser currentUser, CancellationToken cancellation);
    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> UpdateAsync(WarehouseViewModel viewModel, CurrentUser currentUser);

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> DeleteAsync(int id, CurrentUser currentUser);

    /// <summary>
    /// import warehouses by excel
    /// </summary>
    /// <param name="datas">excel datas</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> ExcelAsync(List<WarehouseExcelImportViewModel> datas, CurrentUser currentUser);
    /// <summary>
    /// CreateRuleSettings a new warehouse settings entry using the specified view model and user context.
    /// </summary>
    /// <remarks>The caller must ensure that the provided view model is valid and that the current
    /// user has the necessary permissions to create warehouse settings. If the user lacks sufficient permissions or
    /// the view model is invalid, the operation may fail.</remarks>
    /// <param name="viewModel">The view model containing the details for the warehouse settings to be created. Cannot be null.</param>
    /// <param name="currentUser">The current user context, used to determine permissions and ownership for the new settings entry. Cannot be
    /// null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the ID of the newly
    /// created settings entry and a message indicating the outcome of the operation.</returns>
    Task<(int id, string msg)> CreateRuleSettingsAsync(WarehouseSettingsViewModel viewModel, CurrentUser currentUser);

    /// <summary>
    /// Get Rule Settings
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<IEnumerable<RuleSettingsViewModel>> GetRuleSettingsAsync(int id, CurrentUser currentUser);

    /// <summary>
    /// Synchronous Wcs Locations
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(int id, string msg)> SynchronousWcsLocationsAsync(SyncWcsLocationViewModel viewModel, CurrentUser currentUser);
    /// <summary>
    /// Delete Rule Settings
    /// </summary>
    /// <param name="id"></param>
    /// <param name="settingRuleId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(bool flag, string msg)> DeleteRuleSettings(int id, int settingRuleId, CurrentUser currentUser);
    /// <summary>
    /// Get Warehouse Info
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<InventoryDTO> GetWarehouseInfo(CurrentUser currentUser);
    /// <summary>
    /// Active WareHouse
    /// </summary>
    /// <param name="wareHouseId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(int id, string msg)> ActiveWareHouseAsync(int wareHouseId, CurrentUser currentUser);
    /// <summary>
    /// De-Active WareHouse
    /// </summary>
    /// <param name="wareHouseId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<(int id, string msg)> DeActiveWareHouseAsync(int wareHouseId, CurrentUser currentUser);
    /// <summary>
    /// General Info
    /// </summary>
    /// <param name="wareHouseId"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<WarehouseGeneralInfo> GetGeneralInfoAsync(int wareHouseId, CurrentUser currentUser);
    /// <summary>
    /// Get Receipts
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pageSearch"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<InboundInfoModel>> GetReceiptDetailsByIdAsync(int id, 
        PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Retrieves the details of an outbound order specified by its unique identifier, applying optional pagination and
    /// search criteria.
    /// </summary>
    /// <remarks>Throws an exception if the order is not found or if the current user does not have permission
    /// to access the order details.</remarks>
    /// <param name="id">The unique identifier of the order to retrieve. Must be a positive integer.</param>
    /// <param name="pageSearch">An object containing pagination and search parameters used to filter and limit the returned order details.</param>
    /// <param name="currentUser">The user requesting the order details. Determines access permissions and visibility of the returned data.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see
    /// cref="OutboundInfoModel"/> objects representing the details of the specified order.</returns>
    Task<IEnumerable<OutboundInfoModel>> GetOrderDetailsByIdAsync(int id, 
        PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Inventory Overview
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pageSearch"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<InventoryOverview>> GetInventoryOverviewByIdAsync(int id, 
        PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get Safety Stock Config
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="warehouseId"></param>
    /// <returns></returns>
    Task<IEnumerable<SkuSafetyStockDto>> GetSafetyStockConfigAsnyc(CurrentUser currentUser, int warehouseId);
    /// <summary>
    /// Import Excel Safety Stock
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="warehouseId"></param>
    /// <param name="safetyStocks"></param>
    /// <returns></returns>
    Task<int> ImportExcelSafetyStockConfig(CurrentUser currentUser, 
        int warehouseId, List<InputSkuSafetyStock> safetyStocks);
    /// <summary>
    /// Update Qty Safety Stock Config
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="warehouseId"></param>
    /// <param name="safetyStock"></param>
    /// <returns></returns>
    Task<int> UpdateQtySafetyStockConfig(CurrentUser currentUser, int warehouseId, SkuSafetyStockDto safetyStock);
    /// <summary>
    /// Delete Safety Stock Config
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="warehouseId"></param>
    /// <param name="skuSafetyId"></param>
    /// <returns></returns>
    Task<int> DeleteSafetyStockConfig(CurrentUser currentUser, int warehouseId, int skuSafetyId);
    /// <summary>
    /// Calculator Pallets
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<AvailablePallet>> GetCalculatorPalletsAsync(CalculatorPalletRequest request, 
        CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get Floors
    /// </summary>
    /// <param name="id"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<IEnumerable<int>> GetFloors(int id, CurrentUser currentUser);
    /// <summary>
    /// Get Master Location Data
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<IEnumerable<LocationMaster>> GetMasterData(CurrentUser currentUser);

    #endregion
}


