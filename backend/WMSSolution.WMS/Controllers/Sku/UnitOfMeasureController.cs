
using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Excel;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.IServices.Sku;

namespace WMSSolution.WMS.Controllers.Sku;

/// <summary>
/// Unit Of Measure
/// </summary>
/// <param name="service"></param>
[Route("unitOfMeasure")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class UnitOfMeasureController(IUnitOfMeasureService service) : BaseController
{
    private readonly IUnitOfMeasureService _service = service;

    /// <summary>
    /// Delete Unit
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ResultModel<int>> DeleteUnitAsync(int id, CancellationToken cancellationToken)
    {
        var (flag, msg) = await _service.DeleteUnitAsync(id, CurrentUser, cancellationToken);
        if (!flag)
        {
            return ResultModel<int>.Error(msg ?? "Failed to Delete Unit");
        }

        return ResultModel<int>.Success(1);
    }

    /// <summary>
    /// Import Excel
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("import-excel")]
    public async Task<ResultModel<int>> ImportExcelData([FromBody] List<InputUnitOfMeasure> request, CancellationToken cancellationToken)
    {
        var result = await _service.ImportExcelData(request, CurrentUser, cancellationToken);

        if (result <= 0)
        {
            return ResultModel<int>.Error("Failed to Import Excel UnitOfMeasure");
        }
        return ResultModel<int>.Success(result);
    }

    /// <summary>
    /// Get All units
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    public async Task<ResultModel<List<UnitDTO>>> GetAllAsync()
    {
        var data = await _service.GetAllAsync(CurrentUser);
        if (data.Count != 0)
        {
            return ResultModel<List<UnitDTO>>.Success(data);
        }

        return ResultModel<List<UnitDTO>>.Success([]);
    }
}

