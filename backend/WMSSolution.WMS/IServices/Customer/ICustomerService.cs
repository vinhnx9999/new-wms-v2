using WMSSolution.Core.JWT;
using WMSSolution.Core.Models;
using WMSSolution.Core.Services;
using WMSSolution.Shared.Excel;
using WMSSolution.Shared.MasterData;
using WMSSolution.WMS.Entities.Models;
using WMSSolution.WMS.Entities.ViewModels;
using WMSSolution.WMS.Entities.ViewModels.Customer;

namespace WMSSolution.WMS.IServices.Customer;

/// <summary>
/// Interface of CustomerService
/// </summary>
public interface ICustomerService : IBaseService<CustomerEntity>
{
    #region Api
    /// <summary>
    /// page search
    /// </summary>
    /// <param name="pageSearch">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns></returns>
    Task<(List<CustomerResponseViewModel> data, int totals)> PageAsync(PageSearch pageSearch, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get all records
    /// </summary>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    Task<List<CustomerViewModel>> GetAllAsync(CurrentUser currentUser);
    /// <summary>
    /// Get a record by id
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    Task<CustomerViewModel> GetAsync(int id);
    /// <summary>
    /// add a new record
    /// </summary>
    /// <param name="viewModel">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    Task<(int id, string msg)> AddAsync(CustomerResponseViewModel viewModel, CurrentUser currentUser);
    /// <summary>
    /// update a record
    /// </summary>
    /// <param name="id">  </param>
    /// <param name="viewModel">args</param>
    /// <param name="currentUser">currentUser</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> UpdateAsync(int id, UpdateCustomerRequest viewModel, CurrentUser currentUser, CancellationToken cancellationToken);

    /// <summary>
    /// delete a record
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    Task<(bool flag, string msg)> DeleteAsync(int id);
    #endregion

    #region Import
    /// <summary>
    /// import customers by excel
    /// </summary>
    /// <param name="input">excel data</param>
    /// <param name="currentUser">currentUser</param>
    /// <returns></returns>
    Task<(bool flag, List<CustomerImportViewModel> errorData)> ExcelAsync(List<CustomerImportViewModel> input, CurrentUser currentUser);
    /// <summary>
    /// Import Excel
    /// </summary>
    /// <param name="request"></param>
    /// <param name="currentUser"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ImportExcelData(List<InputCustomer> request, CurrentUser currentUser, CancellationToken cancellationToken);
    /// <summary>
    /// Get Master Chain Data
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    Task<IEnumerable<ChainMaster>> GetMasterData(CurrentUser currentUser);

    #endregion
}
