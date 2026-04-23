using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMSSolution.Core.DBContext;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared;

namespace WMSSolution.Core.Controller;

/// <summary>
/// Import Excel
/// </summary>
[Route("[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class ImportExcelController(SqlDBContext sqlDbContext) : BaseController
{
    private readonly SqlDBContext _sqlDbContext = sqlDbContext;

    ///import-excel
    [HttpPost("/skus/import-excel")]
    public async Task<ResultModel<int>> ImportExcelData([FromBody] List<InputSku> request, CancellationToken cancellationToken)
    {
        SkuExcelBackgroundService _backgroundService = new(_sqlDbContext);
        await _backgroundService.QueueImportAsync(request);
        return ResultModel<int>.Success(1);
    }
}
