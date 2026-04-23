using System.Text.Json;
using Wms.Theme.Web.Model.Reports;
using Wms.Theme.Web.Model.ShareModel;
using Wms.Theme.Web.Services.Dashboard;
using Wms.Theme.Web.Util;
using WMSSolution.Shared.MasterData;

namespace Wms.Theme.Web.Services.Reports;

public class ReportService(IHttpClientFactory httpClientFactory,
    ILogger<DashboardService> logger, IConfiguration configuration) :
    BaseApiService(httpClientFactory, logger, configuration), IReportService
{
    public async Task<(int? data, string? message)> BanVendor(long vendorId)
    {
        try
        {
            var client = CreateClient();
            var endpoint = $"/reports/ban-vendors/{vendorId}";
            var response = await client.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Response is not success");
                return (0, "Response is not success");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<int>>(responseContent, jsonOptions);
            if (result is null || !result.IsSuccess)
            {
                _logger.LogError(
                "API {Endpoint} returned error: {Error}",
                endpoint, result?.ErrorMessage ?? "Deserialization failed");
                return (0, "Deserialization failed when Delete vendor");
            }
            return (result.Data, "success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while Delete vendor {param}", vendorId);
            return (0, ex.Message);
        }
    }

    public async Task<IEnumerable<InOutStatementDto>> GetInOutStatements(InventoryReportRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/reports/inventory-in-out-statements";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to GetInOutStatements. Status Code: {statusCode}, Response: {response}", response.StatusCode, errorContent);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<InOutStatementDto>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetInOutStatements");
        }

        return [];
    }

    public async Task<IEnumerable<WarehouseInventoryReport>> GetInventories(InventoryReportRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/reports/inventories";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to GetInventories. Status Code: {statusCode}, Response: {response}", response.StatusCode, errorContent);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<WarehouseInventoryReport>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetInventories");
        }

        return [];
    }

    public async Task<IEnumerable<InventoryCardItem>> GetInventoryCards(InventoryReportRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/reports/inventory-cards";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to GetInventoryCards. Status Code: {statusCode}, Response: {response}", response.StatusCode, errorContent);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<InventoryCardItem>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetInventoryCards");
        }

        return [];
    }

    public async Task<IEnumerable<LowStockAlertDto>> GetLowStockAlerts()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/reports/low-stock-alerts";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to GetLowStockAlert. Status Code: {statusCode}, Response: {response}", response.StatusCode, errorContent);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<LowStockAlertDto>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetLowStockAlert");
        }

        return [];
    }

    public async Task<IEnumerable<ImportReportItem>> GetReportIncomingGoods(InventoryReportRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/reports/incoming-goods";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to GetReportIngoingGoods. Status Code: {statusCode}, Response: {response}", response.StatusCode, errorContent);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<ImportReportItem>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetReportIngoingGoods");
        }

        return [];
    }

    public async Task<IEnumerable<ExportReportItem>> GetReportOutgoingGoods(InventoryReportRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/reports/outgoing-goods";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to GetReportOutgoingGoods. Status Code: {statusCode}, Response: {response}", response.StatusCode, errorContent);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<ExportReportItem>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetReportOutgoingGoods");
        }

        return [];
    }

    public async Task<IEnumerable<VendorMaster>> GetVendors()
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/reports/vendors";
            var response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to GetVendors. Status Code: {statusCode}, Response: {response}", response.StatusCode, errorContent);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<VendorMaster>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetVendors");
        }

        return [];
    }

    public async Task<IEnumerable<StockOnShelfDto>> SearchStockOnShelf(InventoryReportRequest request)
    {
        try
        {
            var client = CreateClient();
            var endpoint = "/reports/search-stock-on-shelf";
            var response = await client.PostAsync(endpoint, request.ContentPretty());
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to SearchStockOnShelf. Status Code: {statusCode}, Response: {response}", response.StatusCode, errorContent);
                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultModel<IEnumerable<StockOnShelfDto>>>(content, jsonOptions);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchStockOnShelf");
        }

        return [];
    }
}
