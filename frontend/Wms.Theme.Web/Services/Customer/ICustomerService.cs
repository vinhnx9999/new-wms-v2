using Wms.Theme.Web.Model.Customer;
using Wms.Theme.Web.Model.ShareModel;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.Customer
{
    public interface ICustomerService
    {
        Task<List<CustomerDTO>> GetAllCustomersAsync();
        Task<List<CustomerResponseViewModel>> PageSearchAsync(PageSearchRequest pageSearch);

        /// <summary>
        /// Add Customer
        /// </summary>
        /// <param name="request">The request object containing customer details</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task<(int? Id, string? Message)> AddAsync(AddCustomerRequest request);
        Task<(int? data, string? message)> ImportExcelData(List<InputCustomer> inputSuppliers);
        Task<(int? data, string? message)> DeleteCustomer(int supplierId);

        /// <summary>
        /// update customer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<(bool isSuccess, string? message)> UpdateCustomerAsync(int id, UpdateCustomerRequest request);
    }
}
