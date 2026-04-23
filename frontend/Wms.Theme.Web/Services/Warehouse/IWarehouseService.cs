using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Sku;
using Wms.Theme.Web.Model.Warehouse;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.Planning;

namespace Wms.Theme.Web.Services.Warehouse;

public interface IWarehouseService
{
    Task<List<FormSelectItem>> GetSelectItemsAsnyc();
    Task<ResultModel<List<WarehouseViewModel>>> GetAllAsync();
    Task<IEnumerable<WcsLocationDto>> GetWcsLocationsAsnyc(string wcsBlockId);
    Task<IEnumerable<WcsLocationDto>> GetWcsLocationsAsnyc(Guid? guid = null);
    Task<(int Id, string Message)> CreateSettingsAsync(CreateStoreSettingsRequest request);
    Task<IEnumerable<StoreRuleSettingsDto>> GetSettingsAsync(int storeId);
    Task<(int Id, string Message)> SaveWcsLocations(int storeId,
        IEnumerable<WcsLocationDto> wcsLocations, string blockId);
    Task<bool> DeleteRuleSettings(int storeId, int settingRuleId);

    // PageSearchWarehouse is used for warehouse management page
    Task<IEnumerable<WarehouseVM>> PageSearchWarehouse(PageSearchRequest pageSearchRequest);

    /// <summary>
    /// Add Warehouse
    /// </summary>
    /// <param name="request">The request object containing warehouse details</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task<(int? Id, string? Message)> AddAsync(AddWareHouseRequest request);
    Task<(int? data, string? message)> ActiveWareHouse(int wareHouseId);
    Task<(int? data, string? message)> DeActiveWareHouse(int wareHouseId);
    Task<WarehouseGeneralInfo> GetGeneralInfoAsync(int wareHouseId);
    Task<IEnumerable<InboundInfoModel>> GetInboundByWarehouse(int warehouseId, PageSearchRequest condition);
    Task<IEnumerable<OutboundInfoModel>> GetOutboundByWarehouse(int id, PageSearchRequest condition);
    Task<IEnumerable<InventoryOverview>> GetInventoryOverview(int id, PageSearchRequest condition);
    Task<IEnumerable<SkuSafetyStockDto>> GetSafetyStockConfig(int warehouseId);
    Task<(int? data, string? message)> ImportExcelSafetyStock(int warehouseId,
        List<InputSkuSafetyStock> inputSkuSafety);
    Task<(int? data, string? message)> UpdateSkuSafetyStock(int warehouseId, SkuSafetyStockDto skuSafety);
    Task<(int? data, string? message)> DeleteSafetyData(int warehouseId, int skuSafetyId);
    Task<IEnumerable<AvailablePallet>> CalculatorPallets(CalculatorPalletRequest request);
    Task<IEnumerable<int>> GetFloors(int warehouseId);
}
