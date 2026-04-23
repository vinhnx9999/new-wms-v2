using Wms.Theme.Web.Model.GoodLocation;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Warehouse;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.GoodLocations;

public interface IGoodLocationService
{
    Task<List<GoodLocationDto>> GetGoodsLocationPageAsync(PageSearchRequest pageSearchRequest);
    Task<ResultModel<PageData<GoodLocationDto>>> GetGoodLocationPageAsyncWithFormat(PageSearchRequest pageSearchRequest);
    Task<List<GoodLocationDto>> GetAvailableLocationsForPalletAsync();
    Task<List<GoodLocationDto>> GetAvailableLocationsForPalletAsync(int totalPalletNeed);
    Task<IEnumerable<StoreLocationDto>> GetStoreLocationsAsync(int warehouseId);
    Task<List<LocationWithPalletViewModel>> GetGoodLocationWithPalletAsync(GetLocationWithPalletRequest request);
    Task<List<LocationWithPalletViewModel>> SearchLocationWithSkuAsync(GetLocationWithSkuIdRequest request);
    Task<IEnumerable<WcsBlockLocationDto>> GetWcsBlocksAsnyc();
    /// <summary>
    /// Get Pallet and Location from wcs 
    /// </summary>
    /// <param name="blockId">block id </param>
    /// <returns>list pallet and location address</returns>
    Task<List<PalletLocactionDTO>> GetWcsPalletlocationAsync(string blockId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<int> CreateLocationAsync(AddLocationRequest request);

    /// <summary>
    /// Get location by warehouse id
    /// </summary>          
    /// <param name="warehouseId">The id of the warehouse</param>
    /// <returns>A list of locations for the specified warehouse</returns>
    Task<IEnumerable<LocationOnlyDto>> GetLocationsByWarehouseAsync(int warehouseId);

    /// <summary>
    /// Import Excel
    /// </summary>
    /// <param name="inputLocations"></param>
    /// <returns></returns>
    Task<(int? data, string? message)> ImportExcelData(List<InputLocationExcel> inputLocations);
}