using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Sku;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.MasterData;

namespace Wms.Theme.Web.Services.Sku;

public interface ISkuService
{
    /// <summary>
    /// Page search 
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <returns></returns>
    Task<List<SkuSupplierDTO>> PageSearch(PageSearchRequest pageSearch, int? warehouseId);

    /// <summary>
    /// Page search base on the supplier id, the search condition is the same as PageSearch method
    /// </summary>
    /// <param name="supplierId"></param>
    /// <param name="pageSearch"></param>
    /// <returns></returns>
    Task<List<SkuSupplierDTO>> PageSearchSkuSupplier(int? supplierId, PageSearchRequest pageSearch);

    /// <summary>
    /// Create a new sku
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>

    Task<int> CreateSkuAsync(SkuCreateRequest request);
    /// <summary>
    /// Impot excel from sku Data
    /// </summary>
    /// <param name="inputSkus"></param>
    /// <returns></returns>
    Task<(int? data, string? message)> ImportExcelData(List<InputSku> inputSkus);
    /// <summary>
    /// Delete Sku
    /// </summary>
    /// <param name="skuId"></param>
    /// <returns></returns>
    Task<(int? data, string? message)> DeleteSku(int skuId);

    /// <summary>
    /// Update Sku Async
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<(bool isSucess, string message)> UpdateSkuAsync(int id, UpdateSkuRequest request);
    Task<IEnumerable<SkuDTO>> SearchData(PageSearchRequest pageSearch, int? warehouseId);
    Task<IEnumerable<SkuMaster>> GetMasterData();
}
