using Wms.Theme.Web.Model.ASN;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Model.Supplier;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Services.Supplier
{
    public interface ISupplierService
    {
        /// <summary>
        /// Get all Supplier
        /// </summary>
        /// <returns></returns>
        Task<List<SupplierDTO>> GetAllSuppliers();

        /// <summary>
        /// Page Search Supplier
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<List<SupplierVM>> PageSearchAsync(PageSearchRequest request);

        /// <summary>
        /// Add a new supplier
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<int> AddSuplierAsync(AddSupplierRequest request);
        /// <summary>
        /// im
        /// </summary>
        /// <param name="inputSuppliers"></param>
        /// <returns></returns>
        Task<(int? data, string? message)> ImportExcelData(List<InputSupplier> inputSuppliers);
        Task<(int? data, string? message)> DeleteSupplier(int supplierId);
        Task<(bool isSuccess, string? message)> UpdateSupplierAsync(int id, UpdateSupplierRequest request);
    }
}
