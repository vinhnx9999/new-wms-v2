using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.Shared.Excel;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Goodslocation;

namespace WMSSolution.WMS.IServices;

/// <summary>
/// Interface of GoodslocationService
/// </summary>
public interface IGoodslocationService : IBaseService<GoodslocationEntity>
{
    #region Api
    /// <summary>
    /// get goodslocation of the warehousearea by WarehouseId and warehousearea_id
    /// </summary>
    /// <param name="warehousearea_id">warehousearea's id</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    Task<List<FormSelectItem>> GetGoodslocationByWarehouse_area_id(int warehousearea_id, CurrentUser currentUser);

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">current user</param>
    /// <returns></returns>
    Task<(List<GoodslocationViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser);
    /// <summary>
    /// Get all records
    /// </summary>
    /// <returns></returns>
    Task<List<GoodslocationViewModel>> GetAllAsync(CurrentUser currentUser);
    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <param name="id">primary key</param>
    /// <returns></returns>
    Task<GoodslocationViewModel> GetAsync(int id);
    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    Task<(int id, string msg)> AddAsync(AddLocationRequest request, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="viewModel">viewmodel</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> UpdateAsync(GoodslocationViewModel viewModel, CurrentUser currentUser);

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> DeleteAsync(int id);

    /// <summary>
    /// Get location for pick putaway logic handle for Robot excute Task
    /// </summary>
    /// <returns></returns>
    Task<List<GoodslocationViewModel>> GetLocationForPallet(
        CurrentUser currentUser,
        GetLocationPalletTypeEnum type = GetLocationPalletTypeEnum.Inbound,
        int totalPalletNeed = 1);
    /// <summary>
    /// Get Available Store Locations
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<StoreLocationViewModel>> GetAvailableStoreLocations(int warehouseId, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Page Search Location with Pallet
    /// </summary>
    /// <returns>
    /// List LocationWithPalletViewModel
    /// </returns>
    Task<List<LocationWithPalletViewModel>> GetLocationWithPalletAsync(GetLocationWithPalletRequest request, CurrentUser currentUser, CancellationToken cancellation);

    /// <summary>
    /// Get Available Location With a Sku
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<List<LocationWithPalletViewModel>> GetLocationWithSkuAsync(GetLocationWithSkuIdRequest request, CurrentUser currentUser, CancellationToken cancellation);

    /// <summary>
    /// Get location by warehouse Id
    /// </summary>
    /// <param name="warehouseId">warehouse's id</param>
    /// <param name="currentUser">current user</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    Task<List<LocationOnlyViewModel>> GetLocationsByWarehouseAsync(
        int warehouseId,
        CurrentUser currentUser,
        CancellationToken cancellationToken);

    /// <summary>
    /// Import Excel Data
    /// </summary>
    /// <param name="request">List of locations from Excel</param>
    /// <param name="currentUser">Current user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of locations imported</returns>
    Task<int> ImportExcelData(List<InputLocationExcel> request, CurrentUser currentUser, CancellationToken cancellationToken);
    #endregion
}

