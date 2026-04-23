
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Excel;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Supplier;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Controllers.Supplier;

/// <summary>
/// supplier controller
/// </summary>
/// <remarks>
/// constructor
/// </remarks>
/// <param name="supplierService">supplier Service</param>
/// <param name="stringLocalizer">Localizer</param>
[Route("supplier")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class SupplierController(
    ISupplierService supplierService
      , IStringLocalizer<MultiLanguage> stringLocalizer
        ) : BaseController
{
    #region Args

    /// <summary>
    /// supplier Service
    /// </summary>
    private readonly ISupplierService _supplierService = supplierService;

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
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns></returns>
    [HttpPost("list")]
    public async Task<ResultModel<PageData<SupplierVM>>> PageAsync(PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var (data, totals) = await _supplierService.PageAsync(pageSearch, CurrentUser, cancellationToken);

        return ResultModel<PageData<SupplierVM>>.Success(new PageData<SupplierVM>
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
    public async Task<ResultModel<List<SupplierViewModel>>> GetAllAsync()
    {
        var data = await _supplierService.GetAllAsync(CurrentUser);
        if (data.Any())
        {
            return ResultModel<List<SupplierViewModel>>.Success(data);
        }
        else
        {
            return ResultModel<List<SupplierViewModel>>.Success(new List<SupplierViewModel>());
        }
    }

    /// <summary>
    /// get a record by id
    /// </summary>
    /// <returns>args</returns>
    [HttpGet]
    public async Task<ResultModel<SupplierViewModel>> GetAsync(int id)
    {
        var data = await _supplierService.GetAsync(id);
        if (data != null)
        {
            return ResultModel<SupplierViewModel>.Success(data);
        }
        else
        {
            return ResultModel<SupplierViewModel>.Error(_stringLocalizer["not_exists_entity"]);
        }
    }
    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="request">args</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ResultModel<int>> AddAsync(AddSupplierRequest request, CancellationToken cancellationToken)
    {
        var (id, msg) = await _supplierService.AddAsync(request, CurrentUser, cancellationToken);
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
    /// <param name="id">id</param>
    /// <param name="viewModel">args</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ResultModel<bool>> UpdateAsync(int id, [FromBody] UpdateSupplierRequest viewModel, CancellationToken cancellationToken)
    {
        var (flag, msg) = await _supplierService.UpdateAsync(id, viewModel, CurrentUser, cancellationToken);
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
        var (flag, msg) = await _supplierService.DeleteAsync(id);
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
    /// import suppliers by excel
    /// </summary>
    /// <param name="excel_datas">excel datas</param>
    /// <returns></returns>
    [HttpPost("excel")]
    public async Task<ResultModel<string>> ExcelAsync(List<SupplierExcelImportViewModel> excel_datas)
    {
        var (flag, msg) = await _supplierService.ExcelAsync(excel_datas, CurrentUser);
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
    /// Import excel data of Sku
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("import-excel")]
    public async Task<ResultModel<int>> ImportExcelData([FromBody] List<InputSupplier> request, CancellationToken cancellationToken)
    {
        var result = await _supplierService.ImportExcelData(request, CurrentUser, cancellationToken);

        if (result <= 0)
        {
            return ResultModel<int>.Error("Failed to Import Excel Supplier");
        }
        return ResultModel<int>.Success(result);
    }

    /// <summary>
    /// Delete ById
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ResultModel<int>> DeleteByIdAsync(int id)
    {
        var (flag, msg) = await _supplierService.DeleteAsync(id);
        if (flag)
        {
            return ResultModel<int>.Success(1);
        }
        else
        {
            return ResultModel<int>.Error(msg);
        }
    }

}

