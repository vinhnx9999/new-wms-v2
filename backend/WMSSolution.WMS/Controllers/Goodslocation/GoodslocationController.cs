using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Enums.Location;
using WMSSolution.Shared.Excel;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Goodslocation;
using WMSSolution.WMS.IServices;
namespace WMSSolution.WMS.Controllers;

/// <summary>
/// goodslocation controller
/// </summary>
/// <remarks>
/// constructor
/// </remarks>
/// <param name="goodslocationService">goodslocation Service</param>
/// <param name="stringLocalizer">Localizer</param>
[Route("goodslocation")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class GoodslocationController(
             IGoodslocationService goodslocationService,
             IStringLocalizer<MultiLanguage> stringLocalizer) : BaseController
{
    #region Args

    /// <summary>
    /// goodslocation Service
    /// </summary>
    private readonly IGoodslocationService _goodslocationService = goodslocationService;

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
    [HttpGet("location-by-warehouseare_id")]
    public async Task<ResultModel<List<FormSelectItem>>> GetSelectItemsAsnyc(int warehousearea_id)
    {
        var datas = await _goodslocationService.GetGoodslocationByWarehouse_area_id(warehousearea_id, CurrentUser);
        return ResultModel<List<FormSelectItem>>.Success(datas);
    }

    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <returns></returns>
    [HttpPost("list")]
    public async Task<ResultModel<PageData<GoodslocationViewModel>>> PageAsync(PageSearch pageSearch)
    {
        var (data, totals) = await _goodslocationService.PageAsync(pageSearch, CurrentUser);

        return ResultModel<PageData<GoodslocationViewModel>>.Success(new PageData<GoodslocationViewModel>
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
    public async Task<ResultModel<List<GoodslocationViewModel>>> GetAllAsync()
    {
        var data = await _goodslocationService.GetAllAsync(CurrentUser);
        if (data.Count != 0)
        {
            return ResultModel<List<GoodslocationViewModel>>.Success(data);
        }
        else
        {
            return ResultModel<List<GoodslocationViewModel>>.Success(new List<GoodslocationViewModel>());
        }
    }

    /// <summary>
    /// get a record by id
    /// </summary>
    /// <returns>args</returns>
    [HttpGet]
    public async Task<ResultModel<GoodslocationViewModel>> GetAsync(int id)
    {
        var data = await _goodslocationService.GetAsync(id);
        if (data != null)
        {
            return ResultModel<GoodslocationViewModel>.Success(data);
        }
        else
        {
            return ResultModel<GoodslocationViewModel>.Error(_stringLocalizer["not_exists_entity"]);
        }
    }

    /// <summary>
    /// Get Available Store Locations
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    //goodslocation/available-store-locations/{warehouseId}
    [HttpGet("available-store-locations/{warehouseId}")]
    public async Task<ResultModel<List<StoreLocationViewModel>>> GetAvailableStoreLocations(int warehouseId, CancellationToken cancellationToken)
    {
        var data = await _goodslocationService.GetAvailableStoreLocations(warehouseId, CurrentUser, cancellationToken);
        if (data.Count != 0)
        {
            return ResultModel<List<StoreLocationViewModel>>.Success(data);
        }
        else
        {
            return ResultModel<List<StoreLocationViewModel>>.Success([]);
        }
    }

    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="request">args</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ResultModel<int>> AddAsync(AddLocationRequest request, CancellationToken cancellationToken)
    {
        var (id, msg) = await _goodslocationService.AddAsync(request, CurrentUser, cancellationToken);
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
    public async Task<ResultModel<bool>> UpdateAsync(GoodslocationViewModel viewModel)
    {
        var (flag, msg) = await _goodslocationService.UpdateAsync(viewModel, CurrentUser);
        if (flag)
        {
            return ResultModel<bool>.Success(flag);
        }
        else
        {
            return ResultModel<bool>.Error(msg, 400, flag);
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
        var (flag, msg) = await _goodslocationService.DeleteAsync(id);
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
    /// get available locations for pallet placement
    /// </summary>
    /// <returns>List of available locations</returns>
    [HttpGet("available-for-pallet")]
    public async Task<ResultModel<List<GoodslocationViewModel>>> GetLocationForPalletAsync(
        GetLocationPalletTypeEnum type = GetLocationPalletTypeEnum.Inbound,
        int totalPalletNeed = 1)
    {
        var data = await _goodslocationService.GetLocationForPallet(CurrentUser, type, totalPalletNeed);
        if (data.Any())
        {
            return ResultModel<List<GoodslocationViewModel>>.Success(data);
        }
        else
        {
            return ResultModel<List<GoodslocationViewModel>>.Success(new List<GoodslocationViewModel>());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("list-location-with-pallet")]
    public async Task<ResultModel<List<LocationWithPalletViewModel>>> GetLocationWithPalletAsync(GetLocationWithPalletRequest request, CancellationToken cancellationToken)
    {
        var result = await _goodslocationService.GetLocationWithPalletAsync(request, CurrentUser, cancellationToken);
        return ResultModel<List<LocationWithPalletViewModel>>.Success(result);
    }

    /// <summary>
    /// Get Available Locations For Sku
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("list-location-available-sku")]
    public async Task<ResultModel<List<LocationWithPalletViewModel>>> GetAvailableLocationForSkuAsync(GetLocationWithSkuIdRequest request, CancellationToken cancellationToken)
    {
        var result = await _goodslocationService.GetLocationWithSkuAsync(request, CurrentUser, cancellationToken);
        return ResultModel<List<LocationWithPalletViewModel>>.Success(result);
    }

    /// <summary>
    /// Get location with return only location info by warehouse id
    /// </summary>
    /// <param name="warehouseId">ID of the warehouse</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of locations</returns>
    [HttpGet("locations/{warehouseId:int}")]
    public async Task<ResultModel<List<LocationOnlyViewModel>>> GetLocationsByWarehouseAsync(
    int warehouseId,
    CancellationToken cancellationToken)
    {
        var data = await _goodslocationService.GetLocationsByWarehouseAsync(
            warehouseId,
            CurrentUser,
            cancellationToken);

        return ResultModel<List<LocationOnlyViewModel>>.Success(data);
    }

    /// <summary>
    /// Import Excel Data to Locations
    /// </summary>
    /// <param name="request">List of locations from Excel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of locations imported</returns>
    [HttpPost("import-excel")]
    public async Task<ResultModel<int>> ImportExcelData([FromBody] List<InputLocationExcel> request, CancellationToken cancellationToken)
    {
        var result = await _goodslocationService.ImportExcelData(request, CurrentUser, cancellationToken);

        if (result <= 0)
        {
            return ResultModel<int>.Error("Failed to Import Excel Location");
        }

        return ResultModel<int>.Success(result);
    }

    #endregion
}
