using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Stockadjust;
using WMSSolution.WMS.IServices.Stockadjust;

namespace WMSSolution.WMS.Controllers.Stockadjust;

/// <summary>
/// stockadjust controller
/// </summary>
/// <param name="service">stockadjust Service</param>
/// <param name="stringLocalizer">Localizer</param>
[Route("stockadjust")]
[ApiController]
[ApiExplorerSettings(GroupName = "WMS")]
public class StockadjustController(IStockadjustService service
      , IStringLocalizer<MultiLanguage> stringLocalizer) : BaseController
{
    #region Args

    /// <summary>
    /// stockadjust Service
    /// </summary>
    private readonly IStockadjustService _service = service;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    #endregion

    #region Api
    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <returns></returns>
    [HttpPost("list")]
    public async Task<ResultModel<PageData<StockadjustViewModel>>> PageAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _service.PageAsync(pageSearch, CurrentUser);

        return ResultModel<PageData<StockadjustViewModel>>.Success(new PageData<StockadjustViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// get a record by id
    /// </summary>
    /// <returns>args</returns>
    [HttpGet]
    public async Task<ResultModel<StockadjustViewModel>> GetAsync(int id)
    {
        var data = await _service.GetAsync(id);
        if (data != null)
        {
            data.id = id;
            return ResultModel<StockadjustViewModel>.Success(data);
        }

        return ResultModel<StockadjustViewModel>.Error(_stringLocalizer["not_exists_entity"]);
    }

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <returns></returns>
    [HttpPost("list-processing")]
    public async Task<ResultModel<PageData<StockprocessGetViewModel>>> PageProcessingAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _service.PageProcessingAsync(pageSearch);

        return ResultModel<PageData<StockprocessGetViewModel>>.Success(new PageData<StockprocessGetViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// Get by processing
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("processing/{id}")]
    public async Task<ResultModel<StockadjustViewModel>> GetByProcessingAsync(int id)
    {
        var data = await _service.GetByProcessingIdAsync(id);
        if (data != null)
        {
            data.id = id;
            return ResultModel<StockadjustViewModel>.Success(data);
        }

        return ResultModel<StockadjustViewModel>.Error(_stringLocalizer["not_exists_entity"]);
    }

    /// <summary>
    /// adding multiple record 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ResultModel<string>> AddStockAdjustAsync(StockAdjustRequest request)
    {
        var (total, msg) = await _service.AddAsync(request, CurrentUser);

        return total == 0 ? ResultModel<string>.Error(msg) : ResultModel<string>.Success(msg);
    }

    /// <summary>
    /// StockSources
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <returns></returns>
    [HttpPost("stocksources")]
    public async Task<ResultModel<PageData<StockSourceSelectionViewModel>>> GetStockAdjustAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _service.GetStockSourcesForChangeRequestAsync(pageSearch);

        return ResultModel<PageData<StockSourceSelectionViewModel>>
            .Success(new PageData<StockSourceSelectionViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// SKU Selection for Adjustment with Lock Info
    /// </summary>
    /// <param name="pageSearch"></param>
    /// <returns></returns>
    [HttpPost("sku-selection")]
    public async Task<ResultModel<PageData<SkuAdjustmentSelectionViewModel>>> GetSkuSelectionAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _service.GetSkuForAdjustmentSelectionAsync(pageSearch);

        return ResultModel<PageData<SkuAdjustmentSelectionViewModel>>
            .Success(new PageData<SkuAdjustmentSelectionViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// confirm processing
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("process-confirm")]
    public async Task<ResultModel<string>> ConfirmProcess(int id)
    {
        var (flag, msg) = await _service.ConfirmProcess(id, CurrentUser);

        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// confirm adjustment
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("adjustment-confirm")]
    public async Task<ResultModel<string>> ConfirmAdjustment(int id)
    {
        var (flag, msg) = await _service.ConfirmAdjustment(id, CurrentUser);

        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    [HttpDelete("processing/{id}")]
    public async Task<ResultModel<string>> DeleteAsync(int id)
    {
        var (flag, msg) = await _service.DeleteProcessingAsync(id);

        return flag ? ResultModel<string>.Success(msg) : ResultModel<string>.Error(msg);
    }

    #endregion
}

