using Microsoft.AspNetCore.Http;
using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.MasterData;
using WMSSolution.WMS.Entities.Models.Sku;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Sku;

namespace WMSSolution.WMS.IServices.Sku;

/// <summary>
/// Interface of SkuService
/// </summary>
public interface ISkuService : IBaseService<SkuEntity>
{
    /// <summary>
    /// Get all Sku Uoms
    /// </summary>
    /// <returns></returns>
    Task<List<SkuUomDTO>> GetAllSkuUomsAsync(long tenantId);
    /// <summary>
    /// Calculates the total number of items associated with the specified user.
    /// </summary>
    /// <remarks>This method may throw an exception if the user is not found in the system.</remarks>
    /// <param name="currentUser">The user for whom the total items are being calculated. This parameter cannot be null.</param>
    /// <returns>The total number of items associated with the specified user. Returns 0 if the user has no associated items.</returns>
    Task<int> GetTotalItems(CurrentUser currentUser);

    /// <summary>
    /// Create Sku async
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>    
    /// <returns></returns>
    Task<int> CreateSkuAsync(SkuCreateRequest request, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Create Sku With Excel Async 
    /// </summary>
    /// <param name="file"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<List<SkuEntity>> CreateSkuWithExcelAsync(IFormFile file, CurrentUser currentUser);

    /// <summary>
    /// page search List Sku
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="warehouseId"></param>
    /// <returns></returns>
    Task<(List<SkuSupplierDTO> data, int total)> PageAsync(PageSearch pageSearch, int? warehouseId, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// Page sku list by supplier id
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <param name="supplierID"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(List<SkuSupplierDTO> data, int total)> PageSkuSupplierAsync(PageSearch pageSearch, int? supplierID, CurrentUser currentUser, CancellationToken cancellationToken);

    #region Dat
    /// <summary>
    /// Get async 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<SkuViewModel> GetAsync(int id);

    /// <summary>
    /// update Async 
    /// </summary>
    /// <param name="viewModel"></param>
    /// <returns></returns>
    Task<(bool flag, string msg)> UpdateAsync(SkuViewModel viewModel);

    /// <summary>
    /// Delete Async
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<(bool flag, string msg)> DeleteAsync(int id);
    /// <summary>
    /// Import Excel
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ImportExcelData(List<InputSku> request,
        CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// update sku async
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool flag, string msg)> UpdateSkuAsync(int id, UpdateSkuRequest request, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Search Data
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(IEnumerable<SkuDTO> data, int total)> SearchData(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Master SKU Data
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<IEnumerable<SkuMaster>> GetMasterData(CurrentUser currentUser);

    #endregion
}
