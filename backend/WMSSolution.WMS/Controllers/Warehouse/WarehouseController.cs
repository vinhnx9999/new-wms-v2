using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.Planning;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Warehouse;
using WMSSolution.WMS.Entities.ViewModels.Warehouse.Invertory;
using WMSSolution.WMS.IServices.Warehouse;

namespace WMSSolution.WMS.Controllers.Warehouse;

/// <summary>
/// warehouse controller
/// </summary>
/// <remarks>
/// constructor
/// </remarks>
/// <param name="warehouseService">warehouse Service</param>
/// <param name="stringLocalizer">Localizer</param>
[Route("warehouse")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class WarehouseController(IWarehouseService warehouseService,
    IStringLocalizer<MultiLanguage> stringLocalizer) : BaseController
{
    #region Args

    /// <summary>
    /// warehouse Service
    /// </summary>
    private readonly IWarehouseService _warehouseService = warehouseService;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    #endregion

    #region Api
    /// <summary>
    /// get select items
    /// </summary>
    /// <returns></returns>
    [HttpGet("select-item")]
    public async Task<ResultModel<List<FormSelectItem>>> GetSelectItemsAsnyc()
    {
        var datas = await _warehouseService.GetSelectItemsAsnyc(CurrentUser);
        return ResultModel<List<FormSelectItem>>.Success(datas);
    }

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>list of Warehouse</returns>
    [HttpPost("list")]
    public async Task<ResultModel<PageData<WarehouseViewModel>>> PageAsync(PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var (data, totals) = await _warehouseService.PageAsync(pageSearch, CurrentUser, cancellationToken);

        return ResultModel<PageData<WarehouseViewModel>>.Success(new PageData<WarehouseViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }

    /// <summary>
    /// get all records
    /// </summary>
    /// <returns>args</returns>
    [HttpGet("all")]
    public async Task<ResultModel<List<WarehouseViewModel>>> GetAllAsync()
    {
        var data = await _warehouseService.GetAllAsync(CurrentUser);
        if (data.Count != 0)
        {
            return ResultModel<List<WarehouseViewModel>>.Success(data);
        }
        else
        {
            return ResultModel<List<WarehouseViewModel>>.Success([]);
        }
    }

    /// <summary>
    /// get a record by id
    /// </summary>
    /// <returns>args</returns>
    [HttpGet]
    public async Task<ResultModel<WarehouseViewModel>> GetAsync(int id)
    {
        var data = await _warehouseService.GetAsync(id);
        if (data != null)
        {
            return ResultModel<WarehouseViewModel>.Success(data);
        }
        else
        {
            return ResultModel<WarehouseViewModel>.Error(_stringLocalizer["not_exists_entity"]);
        }
    }
    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ResultModel<int>> AddAsync(WarehouseVM viewModel, CancellationToken cancellationToken)
    {
        var (id, msg) = await _warehouseService.AddAsync(viewModel, CurrentUser, cancellationToken);
        if (id > 0)
        {
            return ResultModel<int>.Success(id);
        }
        else
        {
            return ResultModel<int>.Error(msg);
        }
    }

    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultModel<bool>> UpdateAsync(WarehouseViewModel viewModel)
    {
        var (flag, msg) = await _warehouseService.UpdateAsync(viewModel, CurrentUser);
        if (flag)
        {
            return ResultModel<bool>.Success(flag);
        }
        else
        {
            return ResultModel<bool>.Error(msg, 500, flag);
        }
    }

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<ResultModel<string>> DeleteAsync(int id)
    {
        var (flag, msg) = await _warehouseService.DeleteAsync(id, CurrentUser);
        if (flag)
        {
            return ResultModel<string>.Success(msg);
        }
        else
        {
            return ResultModel<string>.Error(msg);
        }
    }

    /// <summary>
    /// import warehouses by excel
    /// </summary>
    /// <param name="excel_datas">excel datas</param>
    /// <returns></returns>
    [HttpPost("excel")]
    public async Task<ResultModel<string>> ExcelAsync(List<WarehouseExcelImportViewModel> excel_datas)
    {
        var (flag, msg) = await _warehouseService.ExcelAsync(excel_datas, CurrentUser);
        if (flag)
        {
            return ResultModel<string>.Success(msg);
        }
        else
        {
            return ResultModel<string>.Error(msg);
        }
    }

    #endregion

    /// <summary>
    /// Create Settings
    /// </summary>
    /// <param name="viewModel"></param>
    /// <returns></returns>
    [HttpPost("create-rule-settings")]
    public async Task<ResultModel<int>> CreateRuleSettingsAsync([FromBody] WarehouseSettingsViewModel viewModel)
    {
        var (id, msg) = await _warehouseService.CreateRuleSettingsAsync(viewModel, CurrentUser);
        if (id > 0)
        {
            return ResultModel<int>.Success(id);
        }
        else
        {
            return ResultModel<int>.Error(msg);
        }
    }

    /// <summary>
    /// Get Rule Settings
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/rule-settings")]
    public async Task<ResultModel<IEnumerable<RuleSettingsViewModel>>> GetRuleSettingsAsync(int id)
    {
        var data = await _warehouseService.GetRuleSettingsAsync(id, CurrentUser);
        if (data != null)
        {
            return ResultModel<IEnumerable<RuleSettingsViewModel>>.Success(data);
        }
        else
        {
            return ResultModel<IEnumerable<RuleSettingsViewModel>>.Error(_stringLocalizer["not_exists_entity"]);
        }
    }

    /// <summary>
    /// Get Floors
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/floors")]
    public async Task<ResultModel<IEnumerable<int>>> GetFloors(int id)
    {
        var data = await _warehouseService.GetFloors(id, CurrentUser);
        if (data != null)
        {
            return ResultModel<IEnumerable<int>>.Success(data);
        }
        else
        {
            return ResultModel<IEnumerable<int>>.Error(_stringLocalizer["not_exists_entity"]);
        }
    }

    /// <summary>
    /// Delete Rule Settings
    /// </summary>
    /// <param name="id"></param>
    /// <param name="settingRuleId"></param>
    /// <returns></returns>
    [HttpDelete("{id}/rule-settings/{settingRuleId}")]
    public async Task<ResultModel<bool>> DeleteRuleSettings(int id, int settingRuleId)
    {
        var (flag, msg) = await _warehouseService.DeleteRuleSettings(id, settingRuleId, CurrentUser);
        if (flag)
        {
            return ResultModel<bool>.Success(true);
        }
        else
        {
            return ResultModel<bool>.Error(msg, 500, flag);
        }
    }
    /// <summary>
    /// Synchronous Wcs Locations
    /// </summary>
    /// <param name="storeId"></param>
    /// <param name="viewModel"></param>
    /// <returns></returns>
    [HttpPost("{storeId}/synchronous-wcs-locations")]
    public async Task<ResultModel<int>> SynchronousWcsLocationsAsync(int storeId, SyncWcsLocationViewModel viewModel)
    {
        if (storeId != viewModel.WarehouseId) return ResultModel<int>.Error(_stringLocalizer["not_exists_entity"]);

        var (id, msg) = await _warehouseService.SynchronousWcsLocationsAsync(viewModel, CurrentUser);
        if (id > 0)
        {
            return ResultModel<int>.Success(id);
        }
        else
        {
            return ResultModel<int>.Error(msg);
        }
    }

    /// <summary>
    /// Active WareHouse
    /// </summary>
    /// <param name="wareHouseId"></param>
    /// <returns></returns>
    [HttpPatch("{wareHouseId}/active")]
    public async Task<ResultModel<int>> ActiveWareHouseAsync(int wareHouseId)
    {     
        var (id, msg) = await _warehouseService.ActiveWareHouseAsync(wareHouseId, CurrentUser);
        if (id > 0)
        {
            return ResultModel<int>.Success(id);
        }
        else
        {
            return ResultModel<int>.Error(msg);
        }
    }

    /// <summary>
    /// De-active WareHouse
    /// </summary>
    /// <param name="wareHouseId"></param>
    /// <returns></returns>
    [HttpPatch("{wareHouseId}/de-active")]
    public async Task<ResultModel<int>> DeActiveWareHouseAsync(int wareHouseId)
    {
        var (id, msg) = await _warehouseService.DeActiveWareHouseAsync(wareHouseId, CurrentUser);
        if (id > 0)
        {
            return ResultModel<int>.Success(id);
        }
        else
        {
            return ResultModel<int>.Error(msg);
        }
    }

    /// <summary>
    /// General Info
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/general-info")]
    public async Task<ResultModel<WarehouseGeneralInfo>> GetGeneralInfoAsync(int id)
    {
        var data = await _warehouseService.GetGeneralInfoAsync(id, CurrentUser);
        if (data != null)
        {
            return ResultModel<WarehouseGeneralInfo>.Success(data);
        }
        else
        {
            return ResultModel<WarehouseGeneralInfo>.Error(_stringLocalizer["not_exists_entity"]);
        }
    }

    /// <summary>
    /// Inbound Info Model
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("{id}/receipt-details")]
    public async Task<ResultModel<IEnumerable<InboundInfoModel>>> GetReceiptsByWarehouseIdAsync(int id, [FromBody] PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var data = await _warehouseService.GetReceiptDetailsByIdAsync(id, pageSearch, CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<InboundInfoModel>>.Error("Receipt not found");
        }
        return ResultModel<IEnumerable<InboundInfoModel>>.Success(data);
    }

    /// <summary>
    /// Calculator Pallets
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("calculator-pallets")]
    public async Task<ResultModel<IEnumerable<AvailablePallet>>> GetCalculatorPalletsAsync([FromBody] CalculatorPalletRequest request, CancellationToken cancellationToken)
    {
        var data = await _warehouseService.GetCalculatorPalletsAsync(request, CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<AvailablePallet>>.Error("Pallets not found");
        }

        return ResultModel<IEnumerable<AvailablePallet>>.Success(data);
    }
    /// <summary>
    /// Get Orders By WarehouseId
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pageSearch"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("{id}/order-details")]
    public async Task<ResultModel<IEnumerable<OutboundInfoModel>>> GetOrdersByWarehouseIdAsync(int id, [FromBody] PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var data = await _warehouseService.GetOrderDetailsByIdAsync(id, pageSearch, CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<OutboundInfoModel>>.Error("Receipt not found");
        }
        return ResultModel<IEnumerable<OutboundInfoModel>>.Success(data);
    }

    /// <summary>
    /// Get Overview By Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pageSearch"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("{id}/inventory-overviews")]
    public async Task<ResultModel<IEnumerable<InventoryOverview>>> GetOverviewByWarehouseIdAsync(int id, [FromBody] PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var data = await _warehouseService.GetInventoryOverviewByIdAsync(id, pageSearch, CurrentUser, cancellationToken);
        if (data == null)
        {
            return ResultModel<IEnumerable<InventoryOverview>>.Error("Receipt not found");
        }
        return ResultModel<IEnumerable<InventoryOverview>>.Success(data);
    }

    /// <summary>
    /// Safety Stock Config
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/safety-stock-config")]
    public async Task<ResultModel<IEnumerable<SkuSafetyStockDto>>> GetSafetyStockConfig(int id)
    {
        var data = await _warehouseService.GetSafetyStockConfigAsnyc(CurrentUser, id);
        return ResultModel<IEnumerable<SkuSafetyStockDto>>.Success(data);
    }

    /// <summary>
    /// Import Excel safety Stock
    /// </summary>
    /// <param name="id"></param>
    /// <param name="safetyStocks"></param>
    /// <returns></returns>
    [HttpPost("{id}/import-safety-stock-config")]
    public async Task<ResultModel<int>> ImportExcelSafetyStockConfig(int id, [FromBody] List<InputSkuSafetyStock> safetyStocks)
    {
        var result = await _warehouseService.ImportExcelSafetyStockConfig(CurrentUser, id, safetyStocks);
        return ResultModel<int>.Success(result);
    }

    /// <summary>
    /// Update Qty Safety Stock Config
    /// </summary>
    /// <param name="id"></param>
    /// <param name="safetyStock"></param>
    /// <returns></returns>
    [HttpPatch("{id}/safety-stock-config")]
    public async Task<ResultModel<int>> UpdateQtySafetyStockConfig(int id, [FromBody] SkuSafetyStockDto safetyStock)
    {
        var result = await _warehouseService.UpdateQtySafetyStockConfig(CurrentUser, id, safetyStock);
        return ResultModel<int>.Success(result);
    }

    /// <summary>
    /// Delete Safety Stock Config
    /// </summary>
    /// <param name="id"></param>
    /// <param name="skuSafetyId"></param>
    /// <returns></returns>
    [HttpDelete("{id}/safety-stock-config/{skuSafetyId}")]
    public async Task<ResultModel<int>> DeleteSafetyStockConfig(int id, int skuSafetyId)
    {
        var result = await _warehouseService.DeleteSafetyStockConfig(CurrentUser, id, skuSafetyId);
        return ResultModel<int>.Success(result);
    }
}

