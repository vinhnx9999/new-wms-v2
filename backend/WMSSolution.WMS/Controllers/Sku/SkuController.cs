using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.MasterData;
using WMSSolution.WMS.Entities.ViewModels.Sku;
using WMSSolution.WMS.IServices.Sku;

namespace WMSSolution.WMS.Controllers.Sku;

/// <summary>
/// Sku controller 
/// </summary>
[Route("sku")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class SkuController(ISkuService skuService) : BaseController
{
    private readonly ISkuService _skuService = skuService;

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">param</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <param name="warehouseId">warehouseId</param>   
    /// <returns >page Data</returns>   
    [HttpPost("list")]
    public async Task<ResultModel<PageData<SkuSupplierDTO>>> PageAsync([FromQuery] int? warehouseId, PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var (data, total) = await _skuService.PageAsync(pageSearch, warehouseId, CurrentUser, cancellationToken);

        return ResultModel<PageData<SkuSupplierDTO>>.Success(new PageData<SkuSupplierDTO>
        {
            Rows = data,
            Totals = total
        });
    }

    /// <summary>
    /// page search for Sku include Supplier
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <param name="supplierID"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("list/{supplierID}")]
    public async Task<ResultModel<PageData<SkuSupplierDTO>>> PageSkuSupplierAsync([FromRoute] int? supplierID, PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var (data, total) = await _skuService.PageSkuSupplierAsync(pageSearch, supplierID, CurrentUser, cancellationToken);

        return ResultModel<PageData<SkuSupplierDTO>>.Success(new PageData<SkuSupplierDTO>
        {
            Rows = data,
            Totals = total
        });
    }

    /// <summary>
    /// Search Data
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("search-data")]
    public async Task<ResultModel<PageData<SkuDTO>>> SearchData(PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var (data, total) = await _skuService.SearchData(pageSearch, CurrentUser, cancellationToken);

        return ResultModel<PageData<SkuDTO>>.Success(new PageData<SkuDTO>
        {
            Rows = [.. data],
            Totals = total
        });
    }

    /// <summary>
    /// Master Data
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("master-data")]
    public async Task<ResultModel<IEnumerable<SkuMaster>>> MasterData(CancellationToken cancellationToken)
    {
        var data = await _skuService.GetMasterData(CurrentUser);
        return ResultModel<IEnumerable<SkuMaster>>.Success(data);
    }

    /// <summary>
    /// Add new record of Sku
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ResultModel<int>> AddSkuAsync(SkuCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _skuService.CreateSkuAsync(request, CurrentUser, cancellationToken);
        if (result <= 0)
        {
            return ResultModel<int>.Error("Failed to create Sku");
        }
        return ResultModel<int>.Success(result);
    }

    /// <summary>
    /// Import excel data of Sku
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("import-excel")]
    public async Task<ResultModel<int>> ImportExcelData([FromBody] List<InputSku> request, CancellationToken cancellationToken)
    {
        var result = await _skuService.ImportExcelData(request, CurrentUser, cancellationToken);

        if (result <= 0)
        {
            return ResultModel<int>.Error("Failed to Import Excel Sku");
        }
        return ResultModel<int>.Success(result);
    }

    /// <summary>
    /// Delete Sku
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ResultModel<int>> DeleteSkuAsync(int id, CancellationToken cancellationToken)
    {
        var result = await _skuService.DeleteAsync(id);
        if (!result.flag)
        {
            return ResultModel<int>.Error("Failed to Delete Sku");
        }

        return ResultModel<int>.Success(1);
    }

    /// <summary>
    /// Handle Update Sku
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ResultModel<bool>> UpdateSkuAsync(int id, UpdateSkuRequest request, CancellationToken cancellationToken)
    {
        var result = await _skuService.UpdateSkuAsync(id, request, CurrentUser, cancellationToken);
        if (!result.flag)
        {
            return ResultModel<bool>.Error(result.msg);
        }
        return ResultModel<bool>.Success(true);
    }

}