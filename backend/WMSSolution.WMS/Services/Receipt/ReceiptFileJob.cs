using Hangfire;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WMSSolution.WMS.Services.Receipt
{
    /// <summary>
    /// Worker
    /// </summary>
    /// <param name="logger"></param>
    [Queue("wms")]
    [AutomaticRetry(Attempts = 0)]
    public sealed class ReceiptFileJob(ILogger<ReceiptFileJob> logger)
    {
        /// <summary>
        /// Task 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task WriteAsync(ReceiptFilePayload payload, CancellationToken cancellationToken)
        {
            try
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "DbFiles", "Inbound");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = $"in_{DateTime.Now.Ticks:X2}.txt";
                var filePath = Path.Combine(folderPath, fileName);
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower // fit with DB property
                };
                var json = JsonSerializer.Serialize(payload, jsonOptions);

                await File.WriteAllTextAsync(filePath, json, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Write receipt file failed. ReceiptId={ReceiptId}", payload.ReceiptId);
            }
        }
    }

    /// <summary>
    /// Represents the data required to create or process a receipt file, including receipt identifiers, warehouse and
    /// supplier information, metadata, and associated receipt details.
    /// </summary>
    /// <param name="ReceiptId">The unique identifier for the receipt.</param>
    /// <param name="ReceiptNumber">The number assigned to the receipt for tracking and reference purposes.</param>
    /// <param name="WarehouseId">The identifier of the warehouse associated with the receipt.</param>
    /// <param name="SupplierId">The identifier of the supplier related to the receipt.</param>
    /// <param name="Type">An integer value indicating the type or category of the receipt.</param>
    /// <param name="Creator">The name of the user or system that created the receipt.</param>
    /// <param name="CreateDate">The date and time when the receipt was created.</param>
    /// <param name="LastUpdateTime">The date and time when the receipt was last updated.</param>
    /// <param name="Description">An optional description providing additional context or notes about the receipt.</param>
    /// <param name="RefReceipt">An optional reference to another receipt, if applicable.</param>
    /// <param name="TenantId">The identifier of the tenant to which the receipt belongs.</param>
    /// <param name="Details">A list of details associated with the receipt, each represented by a ReceiptFileDetailPayload.</param>
    /// <param name="Status">The status of the receipt.</param>
    public sealed record ReceiptFilePayload(
      int ReceiptId,
      string ReceiptNumber,
      int WarehouseId,
      int SupplierId,
      string? Type,
      string Creator,
      DateTime CreateDate,
      DateTime LastUpdateTime,
      string? Description,
      int? RefReceipt,
      long TenantId,
      int Status,
      List<ReceiptFileDetailPayload> Details);

    /// <summary>
    /// Represents the details of a receipt file, including SKU information, quantity, unit of measure, and relevant dates.
    /// </summary>
    /// <param name="Id">The unique identifier for the receipt detail.</param>
    /// <param name="SkuId">The identifier of the SKU associated with the receipt detail.</param>
    /// <param name="Quantity">The quantity of the SKU in the receipt detail.</param>
    /// <param name="SkuUomId">The identifier of the unit of measure for the SKU.</param>
    /// <param name="CreateDate">The date and time when the receipt detail was created.</param>
    /// <param name="ExpiryDate">The date and time when the receipt detail will expire.</param>
    public sealed record ReceiptFileDetailPayload(
        int Id,
        int SkuId,
        decimal Quantity,
        int SkuUomId,
        DateTime CreateDate,
        DateTime ExpiryDate);

}
