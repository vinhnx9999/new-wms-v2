using WMSSolution.WMS.Entities.ViewModels.Receipt;

namespace WMSSolution.WMS.Entities.ViewModels.Planning;

/// <summary>
/// Planning Picking
/// </summary>
public class PickingDTO: BaseReceiptDetailDto
{
    /// <summary>
    /// Receipt Id
    /// </summary>
    public int? ReceiptId { get; set; } = 0;
    /// <summary>
    /// Receipt Number
    /// </summary>
    public string ReceiptNo { get; set; } = string.Empty;
    /// <summary>
    /// Warehouse ID
    /// </summary>
    public int WarehouseId { get; set; }
    /// <summary>
    /// Warehouse Name
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;
    /// <summary>
    /// Warehouse Address
    /// </summary>
    public string WarehouseAddress { get; set; } = string.Empty;
    /// <summary>
    /// Expected Ship Date
    /// </summary>
    public DateTime? ExpectedShipDate { get; set; }
    /// <summary>
    /// Start Picking Time
    /// </summary>
    public DateTime? StartPickingTime { get; set; }
    /// <summary>
    /// Sku Code
    /// </summary>
    public string SkuCode { get; set; } = string.Empty;

    /// <summary>
    /// Sku Name
    /// </summary>
    public string SkuName { get; set; } = string.Empty;

    /// <summary>
    /// Unit name
    /// </summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// LocationName 
    /// </summary>
    public string? LocationName { get; set; }

    /// <summary>
    /// Pallet Code
    /// </summary>
    public string? PalletCode { get; set; }

    /// <summary>
    /// Expire Date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    /// <summary>
    /// GatewayId
    /// </summary>
    public int? GatewayId { get; set; }
    /// <summary>
    /// IsVirtualLocation
    /// </summary>
    public bool? IsVirtualLocation { get; set; }

}
