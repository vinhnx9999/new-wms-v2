using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.Models.PurchaseOrders;
using WMSSolution.WMS.Entities.ViewModels.PurchaseOrders;
using WMSSolution.WMS.IServices.PurchaseOrder;

namespace WMSSolution.WMS.Controllers.PurchaseOrder;

/// <summary>
/// Purchase Order controller
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="purchaseOrderService">Purchase Order Service</param>
/// <param name="stringLocalizer">Localizer</param>
[Route("purchaseorder")]
[ApiController]
[ApiExplorerSettings(GroupName = "WMS")]
public class PurchaseOrderController(
    IPurchaseOrderService purchaseOrderService,
    IStringLocalizer<MultiLanguage> stringLocalizer) : BaseController
{
    #region Args

    /// <summary>
    /// Purchase Order Service
    /// </summary>
    private readonly IPurchaseOrderService _purchaseOrderService = purchaseOrderService;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    #endregion

    #region Api

    /// <summary>
    /// Page search purchase orders
    /// </summary>
    /// <param name="pageSearch">Search parameters</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Paginated purchase orders</returns>
    [HttpPost("list")]
    public async Task<ResultModel<PageData<PageSearchPOResponse>>> PageAsync(PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var (data, totals) = await _purchaseOrderService.GetPageAsync(pageSearch, CurrentUser, cancellationToken);

        return ResultModel<PageData<PageSearchPOResponse>>.Success(new PageData<PageSearchPOResponse>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// Get purchase order by id
    /// </summary>
    /// <param name="id">Purchase order id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Purchase order detail</returns>
    [HttpGet("{id:int}")]
    public async Task<ResultModel<PoDetailResponseDto>> GetDetailAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        var data = await _purchaseOrderService.GetDetailAsync(id, CurrentUser, cancellationToken);
        return data != null
            ? ResultModel<PoDetailResponseDto>.Success(data)
            : ResultModel<PoDetailResponseDto>.Error(_stringLocalizer["not_exists_entity"]);
    }

    /// <summary>
    /// Create new purchase order   
    /// </summary>
    /// <param name="request">Purchase order data</param>
    /// <param name="cancellationToken">Cancellation token</param>  
    /// <returns>Created purchase order id</returns>    
    [HttpPost]
    public async Task<ResultModel<int>> AddAsync(CreateNewPoRequest request, CancellationToken cancellationToken)
    {
        var (id, msg) = await _purchaseOrderService.CreateNewPoOrder(request, CurrentUser, cancellationToken);
        return id > 0 ? ResultModel<int>.Success(id, msg) : ResultModel<int>.Error(msg);
    }

    /// <summary>
    /// Update purchase order
    /// </summary>
    /// <param name="request">Purchase order data</param>
    /// <returns>Update result</returns>
    [HttpPut]
    public async Task<ResultModel<bool>> UpdateAsync(CreateNewOrderRequest request)
    {
        var (flag, msg) = await _purchaseOrderService.UpdateAsync(request, CurrentUser);
        return flag ? ResultModel<bool>.Success(flag) : ResultModel<bool>.Error(msg, 400, flag);
    }

    /// <summary>
    /// Delete purchase order
    /// </summary>
    /// <param name="id">Purchase order id</param>
    /// <param name="cancellationToken">Cancellation token</param>  
    /// <returns>Delete result</returns>
    [HttpDelete]
    public async Task<ResultModel<string>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var (flag, msg) = await _purchaseOrderService.DeleteAsync(id, CurrentUser, cancellationToken);
        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// Get open purchase orders (for import to ASN)
    /// </summary>
    /// <returns>List of open purchase orders</returns>
    [HttpGet("open-list")]
    public async Task<ResultModel<List<PurchaseOrderEntity>>> GetOpenPosAsync()
    {
        var data = await _purchaseOrderService.GetOpenPosAsync(CurrentUser);
        return ResultModel<List<PurchaseOrderEntity>>.Success(data);
    }

    /// <summary>
    /// Close purchase order (Short Close / Cancellation)
    /// </summary>
    /// <param name="id">Purchase order id</param>
    /// <returns>Close result</returns>
    [HttpPatch("close")]
    public async Task<ResultModel<string>> CloseAsync(int id)
    {
        var (success, msg) = await _purchaseOrderService.CloseAsync(id, CurrentUser);
        return success ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// Generate a new purchase order number
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New purchase order number</returns>
    [HttpGet("generate-po-no")]
    public async Task<ResultModel<string>> GeneratePoNoAsync(CancellationToken cancellationToken)
    {
        var poNo = await _purchaseOrderService.GeneratePoNoAsync(CurrentUser, cancellationToken);
        return ResultModel<string>.Success(poNo);
    }

    #endregion
}

