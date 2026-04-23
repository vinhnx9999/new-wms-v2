using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WMSSolution.Core.Controller;
using WMSSolution.Core.Models;
using WMSSolution.Shared.Excel;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Customer;
using WMSSolution.WMS.IServices.Customer;

namespace WMSSolution.WMS.Controllers.Customer;

/// <summary>
/// customer controller
/// </summary>
/// <remarks>
/// constructor
/// </remarks>
/// <param name="customerService">customer Service</param>
/// <param name="stringLocalizer">Localizer</param>
[Route("customer")]
[ApiController]
[ApiExplorerSettings(GroupName = "Base")]
public class CustomerController(
    ICustomerService customerService
      , IStringLocalizer<MultiLanguage> stringLocalizer
        ) : BaseController
{
    #region Args

    /// <summary>
    /// customer Service
    /// </summary>
    private readonly ICustomerService _customerService = customerService;
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
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    [HttpPost("list")]
    public async Task<ResultModel<PageData<CustomerResponseViewModel>>> PageAsync(PageSearch pageSearch, CancellationToken cancellationToken)
    {
        var (data, totals) = await _customerService.PageAsync(pageSearch, CurrentUser, cancellationToken);

        return ResultModel<PageData<CustomerResponseViewModel>>.Success(new PageData<CustomerResponseViewModel>
        {
            Rows = data,
            Totals = totals
        });
    }
    /// <summary>
    /// Get all records
    /// </summary>
    /// <returns>args</returns>
    [HttpGet("all")]
    public async Task<ResultModel<List<CustomerViewModel>>> GetAllAsync()
    {
        var data = await _customerService.GetAllAsync(CurrentUser);
        if (data.Count != 0)
        {
            return ResultModel<List<CustomerViewModel>>.Success(data);
        }
        else
        {
            return ResultModel<List<CustomerViewModel>>.Success([]);
        }
    }

    /// <summary>
    /// get a record by id
    /// </summary>
    /// <returns>args</returns>
    [HttpGet]
    public async Task<ResultModel<CustomerViewModel>> GetAsync(int id)
    {
        var data = await _customerService.GetAsync(id);
        if (data != null && data.id > 0)
        {
            return ResultModel<CustomerViewModel>.Success(data);
        }
        else
        {
            return ResultModel<CustomerViewModel>.Error(_stringLocalizer["not_exists_entity"]);
        }
    }
    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ResultModel<int>> AddAsync(CustomerResponseViewModel viewModel)
    {
        var (id, msg) = await _customerService.AddAsync(viewModel, CurrentUser);
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
    /// <param name="id">Id</param>
    /// <param name="viewModel">args</param>
    /// <param name="cancellationToken" >cancellation token</param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ResultModel<bool>> UpdateAsync(int id, [FromBody] UpdateCustomerRequest viewModel, CancellationToken cancellationToken)
    {
        var (flag, msg) = await _customerService.UpdateAsync(id, viewModel, CurrentUser, cancellationToken);
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
        var (flag, msg) = await _customerService.DeleteAsync(id);
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

    #region Import
    /// <summary>
    /// import customers by excel
    /// </summary>
    /// <param name="input">excel data</param>
    /// <returns></returns>
    [HttpPost("excel")]
    public async Task<ResultModel<List<CustomerImportViewModel>>> ExcelAsync(List<CustomerImportViewModel> input)
    {
        var (flag, errorData) = await _customerService.ExcelAsync(input, CurrentUser);
        if (flag)
        {
            return ResultModel<List<CustomerImportViewModel>>.Success(errorData);
        }
        else
        {
            return ResultModel<List<CustomerImportViewModel>>.Error("", 400, errorData);
        }
    }

    /// <summary>
    /// Delete By Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ResultModel<int>> DeleteByIdAsync(int id)
    {
        var (flag, msg) = await _customerService.DeleteAsync(id);
        if (flag)
        {
            return ResultModel<int>.Success(1);
        }
        else
        {
            return ResultModel<int>.Error(msg);
        }
    }

    /// <summary>
    /// Import Excel Data
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("import-excel")]
    public async Task<ResultModel<int>> ImportExcelData([FromBody] List<InputCustomer> request, CancellationToken cancellationToken)
    {
        var result = await _customerService.ImportExcelData(request, CurrentUser, cancellationToken);

        if (result <= 0)
        {
            return ResultModel<int>.Error("Failed to Import Excel Customer");
        }
        return ResultModel<int>.Success(result);
    }

    #endregion
}
