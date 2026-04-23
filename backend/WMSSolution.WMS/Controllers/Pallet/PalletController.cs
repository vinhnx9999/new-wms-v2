using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Pallet;
using WMSSolution.WMS.IServices;

namespace WMSSolution.WMS.Controllers.Pallet;

/// <summary>
/// pallet controller
/// </summary>
/// <remarks>
/// constructor
/// </remarks>
/// <param name="palletService">pallet service</param>
/// <param name="stringLocalizer">localizer</param>
[Route("pallet")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class PalletController(
    IPalletService palletService
      , IStringLocalizer<MultiLanguage> stringLocalizer
        ) : BaseController
{
    #region Args
    /// <summary>
    /// pallet service
    /// </summary>
    private readonly IPalletService _palletService = palletService;

    /// <summary>
    /// Localizer Service
    /// </summary>
    private readonly IStringLocalizer<MultiLanguage> _stringLocalizer = stringLocalizer;

    #endregion

    #region Api
    /// <summary>
    /// get all records
    /// </summary>
    /// <returns>list of pallets</returns>
    [HttpGet("all")]
    public async Task<ResultModel<List<PalletViewModel>>> GetAllAsync()
    {
        var data = await _palletService.GetAllAsync(CurrentUser);
        if (data.Count != 0)
        {
            return ResultModel<List<PalletViewModel>>.Success(data);
        }
        else
        {
            return ResultModel<List<PalletViewModel>>.Success([]);
        }
    }

    /// <summary>
    /// Generate pallet code and create new pallet automatically
    /// </summary>
    /// <returns>generated pallet code</returns>
    [HttpPost("generate-code")]
    public async Task<ResultModel<CreatePalletResponse>> GeneratePalletCodeAsync(CancellationToken cancellationToken)
    {
        var result = await _palletService.GenaratePalletCodeAsync(CurrentUser, cancellationToken);

        if (result == null)
        {
            return ResultModel<CreatePalletResponse>.Error(_stringLocalizer["create_failed"]);
        }

        return ResultModel<CreatePalletResponse>.Success(result, _stringLocalizer["create_success"]);
    }

    /// <summary>
    /// Page search pallets
    /// </summary>
    /// <param name="pageSearch">search parameters</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>paged list of pallets</returns>
    [HttpPost("list")]
    public async Task<ResultModel<PageData<PalletPageSearchDTO>>> PageSearchPalletAsync(
        [FromBody] PageSearch pageSearch,
        CancellationToken cancellationToken)
    {
        var (data, total) = await _palletService.PageSearchPallet(pageSearch, CurrentUser, cancellationToken);

        return ResultModel<PageData<PalletPageSearchDTO>>.Success(new PageData<PalletPageSearchDTO>
        {
            Rows = data,
            Totals = total
        });
    }

    #endregion
}

